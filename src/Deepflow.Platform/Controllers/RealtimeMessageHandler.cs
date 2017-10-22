using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Deepflow.Platform.Abstractions.Ingestion;
using Deepflow.Platform.Abstractions.Realtime;
using Deepflow.Platform.Abstractions.Realtime.Messages;
using Deepflow.Platform.Abstractions.Realtime.Messages.Data;
using Deepflow.Platform.Abstractions.Realtime.Messages.Subscriptions;
using Deepflow.Platform.Abstractions.Series;
using Deepflow.Platform.Abstractions.Series.Attribute;
using Deepflow.Platform.Core.Tools;
using Deepflow.Platform.Series;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Orleans;

namespace Deepflow.Platform.Controllers
{
    public class RealtimeMessageHandler : IWebsocketsReceiver, IDataSender
    {
        private readonly ILogger<RealtimeMessageHandler> _logger;
        private readonly IIngestionProcessor _ingestionProcessor;
        private IWebsocketsSender _sender;
        private readonly ConcurrentDictionary<string, SocketSubscriptions> _socketSubscriptions = new ConcurrentDictionary<string, SocketSubscriptions>();

        public RealtimeMessageHandler(ILogger<RealtimeMessageHandler> logger, IIngestionProcessor ingestionProcessor)
        {
            _logger = logger;
            _ingestionProcessor = ingestionProcessor;
        }

        public Task OnConnected(string socketId)
        {
            return Task.FromResult(0);
        }

        public async Task OnDisconnected(string socketId)
        {
            if (_socketSubscriptions.TryGetValue(socketId, out SocketSubscriptions subscriptions))
            {
                await Task.WhenAll(subscriptions.Subscriptions.Select(UnsubscribeFromAttribute));
            }
        }

        public async Task OnReceive(string socketId, string messageString)
        {
            var message = JsonConvert.DeserializeObject<IncomingMessage>(messageString, JsonSettings.Setttings);
            if (message.MessageClass == IncomingMessageClass.Request)
            {
                var request = JsonConvert.DeserializeObject<RequestMessage>(messageString, JsonSettings.Setttings);
                var response = await ReceiveRequest(request, socketId, messageString);
                var responseText = JsonConvert.SerializeObject(response, JsonSettings.Setttings);
                await _sender.Send(socketId, responseText).ConfigureAwait(false);
            }

            //_logger.LogDebug($"Received message for attribute {request.Request.Entity}:{request.Request.Attribute}");
        }

        private async Task<ResponseMessage> ReceiveRequest(RequestMessage request, string socketId, string messageString)
        {
            if (request.RequestType == RequestType.FetchAggregatedAttributeData)
            {
                var fetchRequest = JsonConvert.DeserializeObject<FetchAggregatedAttributeDataRequest>(messageString, JsonSettings.Setttings);
                return await ReceiveFetchAggregatedAttributeDataRequest(fetchRequest).ConfigureAwait(false);
            }
            if (request.RequestType == RequestType.UpdateAttributeSubscriptions)
            {
                var updateRequest = JsonConvert.DeserializeObject<UpdateAttributeSubscriptionsRequest>(messageString, JsonSettings.Setttings);
                return await ReceiveUpdateAttributeSubscriptionsRequest(updateRequest, socketId).ConfigureAwait(false);
            }
            if (request.RequestType == RequestType.AddAggregatedAttributeData)
            {
                var addRequest = JsonConvert.DeserializeObject<AddAggregatedAttributeDataRequest>(messageString, JsonSettings.Setttings);
                return await ReceiveAddAttributeSubscriptionsRequest(addRequest).ConfigureAwait(false);
            }
            return null;
        }

        private async Task<FetchAggregatedAttributeDataResponse> ReceiveFetchAggregatedAttributeDataRequest(FetchAggregatedAttributeDataRequest request)
        {
            var series = GrainClient.GrainFactory.GetGrain<IAttributeSeriesGrain>(SeriesIdHelper.ToAttributeSeriesId(request.EntityGuid, request.AttributeGuid));
            var dataRanges = await series.GetAggregatedData(request.TimeRange, request.AggregationSeconds).ConfigureAwait(false);
            return new FetchAggregatedAttributeDataResponse
            {
                ActionId = request.ActionId,
                MessageClass = OutgoingMessageClass.Response,
                ResponseType = ResponseType.FetchAggregatedAttributeData,
                Ranges = dataRanges,
                Succeeded = true
            };
        }

        private async Task<UpdateAttributeSubscriptionsResponse> ReceiveUpdateAttributeSubscriptionsRequest(UpdateAttributeSubscriptionsRequest request, string socketId)
        {
            var existingSubscriptions = _socketSubscriptions.GetOrAdd(socketId, new SocketSubscriptions());
            var incomingSubscriptions = request.EntitySubscriptions.SelectMany(entitySubscriptions => entitySubscriptions.AttributeGuids.Select(attributeGuid => new DataSubscription { Entity = entitySubscriptions.EntityGuid, Attribute = attributeGuid }));

            try
            {
                await existingSubscriptions.Semaphore.WaitAsync().ConfigureAwait(false);

                var subscriptionsToAdd = incomingSubscriptions.Except(existingSubscriptions.Subscriptions);
                var subscriptionsToRemove = new HashSet<DataSubscription>(existingSubscriptions.Subscriptions.Except(incomingSubscriptions));

                var addTasks = Task.WhenAll(subscriptionsToAdd.Select(subscription => SubscribeToAttribute(socketId, subscription)).ToList());
                var removeTasks = Task.WhenAll(subscriptionsToRemove.Select(UnsubscribeFromAttribute).ToList());

                var addSubscriptions = await addTasks;
                await removeTasks;

                existingSubscriptions.Subscriptions = existingSubscriptions.Subscriptions.Where(existingSubscription => !subscriptionsToRemove.Contains(existingSubscription)).Concat(addSubscriptions).ToList();

                return new UpdateAttributeSubscriptionsResponse
                {
                    ActionId = request.ActionId,
                    MessageClass = OutgoingMessageClass.Response,
                    ResponseType = ResponseType.UpdateAttributeSubscriptions,
                    Succeeded = true
                };
            }
            finally
            {
                existingSubscriptions.Semaphore.Release();
            }
        }

        private async Task<AddAggregatedAttributeDataResponse> ReceiveAddAttributeSubscriptionsRequest(AddAggregatedAttributeDataRequest request)
        {
            await _ingestionProcessor.AddData(request.DataSource, request.SourceName, request.AggregatedDataRange);
            return new AddAggregatedAttributeDataResponse
            {
                ActionId = request.ActionId,
                MessageClass = OutgoingMessageClass.Response,
                ResponseType = ResponseType.AddAggregatedAttributeData,
                Succeeded = true
            };
        }

        private void SendNotification(NotificationMessage notification, string socketId)
        {
            var message = JsonConvert.SerializeObject(notification, JsonSettings.Setttings);
            _sender.Send(socketId, message);
        }

        public void SetSender(IWebsocketsSender sender)
        {
            _sender = sender;
        }

        private async Task<DataSubscription> SubscribeToAttribute(string socketId, DataSubscription subscription)
        {
            var series = GrainClient.GrainFactory.GetGrain<IAttributeSeriesGrain>(SeriesIdHelper.ToAttributeSeriesId(subscription.Entity, subscription.Attribute));
            subscription.Observer = new SeriesObserver(socketId, this);
            var observerRef = await GrainClient.GrainFactory.CreateObjectReference<ISeriesObserver>(subscription.Observer);
            await series.Subscribe(observerRef);
            return subscription;
        }

        private async Task UnsubscribeFromAttribute(DataSubscription subscription)
        {
            var series = GrainClient.GrainFactory.GetGrain<IAttributeSeriesGrain>(SeriesIdHelper.ToCalculationSeriesId(subscription.Entity, subscription.Attribute));
            var observerRef = await GrainClient.GrainFactory.CreateObjectReference<ISeriesObserver>(subscription.Observer);
            await series.Unsubscribe(observerRef);
        }

        public void SendData(string socketId, Guid entity, Guid attribute, Dictionary<int, AggregatedDataRange> aggregatedRanges)
        {
            var notification = new ReceiveAttributeDataNotification
            {
                MessageClass = OutgoingMessageClass.Notification,
                NotificationType = NotificationType.AttributeData,
                EntityGuid = entity,
                AttributeGuid = attribute,
                Notification = new AttributeDataNotification
                {
                    AggregatedRanges = aggregatedRanges
                }
            };
            
            _logger.LogDebug($"Sent notification for ${entity}:{attribute}");
            SendNotification(notification, socketId);
        }

        private class SocketSubscriptions
        {
            public IEnumerable<DataSubscription> Subscriptions = new List<DataSubscription>();
            public readonly SemaphoreSlim Semaphore = new SemaphoreSlim(1);
        }
    }
}
