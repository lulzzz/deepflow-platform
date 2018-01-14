using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Deepflow.Common.Model.Model;
using Deepflow.Ingestion.Service.Processing;
using Deepflow.Platform.Abstractions.Realtime;
using Deepflow.Platform.Abstractions.Realtime.Messages;
using Deepflow.Platform.Abstractions.Realtime.Messages.Data;
using Deepflow.Platform.Abstractions.Realtime.Messages.Subscriptions;
using Deepflow.Platform.Abstractions.Series;
using Deepflow.Platform.Common.Data.Persistence;
using Deepflow.Platform.Core.Tools;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace Deepflow.Ingestion.Service.Realtime
{
    public class RealtimeIngestionReceiver : IWebsocketsReceiver, IDataMessenger, IRealtimeSubscriptions
    {

        private readonly IPersistentDataProvider _persistence;
        private readonly ILogger<RealtimeIngestionReceiver> _logger;
        private readonly TripCounterFactory _trip;
        private IWebsocketsSender _sender;
        private readonly AsyncCollection<(string socket, string message)> _queue = new AsyncCollection<(string socket, string message)>(null, 1000);
        private readonly ConcurrentDictionary<string, Dictionary<Guid, HashSet<Guid>>> _subscriptions = new ConcurrentDictionary<string, Dictionary<Guid, HashSet<Guid>>>();

        public RealtimeIngestionReceiver(IPersistentDataProvider persistence, ILogger<RealtimeIngestionReceiver> logger, TripCounterFactory trip)
        {
            _persistence = persistence;
            _logger = logger;
            _trip = trip;

            Enumerable.Range(0, 10).ForEach(x => Task.Run(ProcessLoop));
        }

        public Task OnConnected(string socketId)
        {
            return Task.FromResult(0);
        }

        public Task OnDisconnected(string socketId)
        {
            _subscriptions.TryRemove(socketId, out Dictionary<Guid, HashSet<Guid>> _);
            return Task.FromResult(0);
        }

        public async Task OnReceive(string socketId, string messageString)
        {
            await _trip.Run("RealtimeIngestionReceiver.OnReceive", async () =>
            {
                await _queue.AddAsync(new ValueTuple<string, string>(socketId, messageString));
            });
        }

        private async Task ProcessLoop()
        {
            while (true)
            {
                (string socket, string message) item = new ValueTuple<string, string>();
                try
                {
                    item = await _queue.TakeAsync();
                    await _trip.Run("RealtimeIngestionReceiver.ProcessLoop", async () =>
                    {
                        var message = JsonConvert.DeserializeObject<IncomingMessage>(item.message, JsonSettings.Setttings);
                        if (message.MessageClass == IncomingMessageClass.Request)
                        {
                            var totalStopwatch = Stopwatch.StartNew();
                            var request = JsonConvert.DeserializeObject<RequestMessage>(item.message, JsonSettings.Setttings);
                            var processStopwatch = Stopwatch.StartNew();
                            var response = await ReceiveRequest(request, item.message, item.socket);
                            var processMs = $"\"processMs\": {processStopwatch.ElapsedMilliseconds}";
                            var responseText = JsonConvert.SerializeObject(response, JsonSettings.Setttings);
                            var totalServerMs = $"\"totalServerMs\": {totalStopwatch.ElapsedMilliseconds}";
                            var timingJson = $" {processMs}, {totalServerMs}, ";
                            var fullResponseText = responseText.Insert(1, timingJson);
                            await _sender.Send(item.socket, fullResponseText).ConfigureAwait(false);
                        }
                    });
                }
                catch (Exception exception)
                {
                    _logger.LogError(new EventId(109), exception, "Error processing request from queue");
                    await _queue.AddAsync(item);
                }
            }
        }

        private async Task<ResponseMessage> ReceiveRequest(RequestMessage request, string messageString, string socketId)
        {
            /*if (request.RequestType == RequestType.AddAggregatedAttributeData)
            {
                var addRequest = JsonConvert.DeserializeObject<AddAggregatedAttributeDataRequest>(messageString, JsonSettings.Setttings);
                return await ReceiveAddAttributeSubscriptionsRequest(addRequest);
            }
            if (request.RequestType == RequestType.AddAggregatedHistoricalAttributeData)
            {
                var addHistoricalRequest = JsonConvert.DeserializeObject<AddAggregatedAttributeHistoricalDataRequest>(messageString, JsonSettings.Setttings);
                return await ReceiveAddAttributeHistoricalSubscriptionsRequest(addHistoricalRequest);
            }*/
            if (request.RequestType == RequestType.UpdateAttributeSubscriptions)
            {
                var updateRequest = JsonConvert.DeserializeObject<UpdateAttributeSubscriptionsRequest>(messageString, JsonSettings.Setttings);
                return await ReceiveUpdateAttributeSubscriptionsRequest(updateRequest, socketId).ConfigureAwait(false);
            }
            if (request.RequestType == RequestType.FetchAggregatedAttributeData)
            {
                var fetchRequest = JsonConvert.DeserializeObject<FetchAggregatedAttributeDataRequest>(messageString, JsonSettings.Setttings);
                return await ReceiveFetchAggregatedAttributeDataRequest(fetchRequest).ConfigureAwait(false);
            }
            
            return null;
        }

        /*private async Task<AddAggregatedAttributeDataResponse> ReceiveAddAttributeSubscriptionsRequest(AddAggregatedAttributeDataRequest request)
        {
            var (entity, attribute) = await _model.ResolveEntityAndAttribute(request.DataSource, request.SourceName);
            await _processor.ReceiveRealtimeData(entity, attribute, request.AggregatedDataRange, request.RawDataRange);
            return new AddAggregatedAttributeDataResponse
            {
                ActionId = request.ActionId,
                MessageClass = OutgoingMessageClass.Response,
                ResponseType = ResponseType.AddAggregatedAttributeData,
                Succeeded = true
            };
        }

        private async Task<AddAggregatedAttributeHistoricalDataResponse> ReceiveAddAttributeHistoricalSubscriptionsRequest(AddAggregatedAttributeHistoricalDataRequest request)
        {
            var (entity, attribute) = await _model.ResolveEntityAndAttribute(request.DataSource, request.SourceName);
            await _processor.ReceiveHistoricalData(entity, attribute, request.AggregatedDataRange);
            return new AddAggregatedAttributeHistoricalDataResponse
            {
                ActionId = request.ActionId,
                MessageClass = OutgoingMessageClass.Response,
                ResponseType = ResponseType.AddAggregatedHistoricalAttributeData,
                Succeeded = true
            };
        }*/
        
        private Task<UpdateAttributeSubscriptionsResponse> ReceiveUpdateAttributeSubscriptionsRequest(UpdateAttributeSubscriptionsRequest request, string socketId)
        {
            var nextSubscriptions = request.EntitySubscriptions.ToDictionary(x => x.EntityGuid, x => x.AttributeGuids);
            _subscriptions.AddOrUpdate(socketId, nextSubscriptions, (id, old) => nextSubscriptions);
            return Task.FromResult(new UpdateAttributeSubscriptionsResponse
            {
                ActionId = request.ActionId,
                MessageClass = OutgoingMessageClass.Response,
                ResponseType = ResponseType.UpdateAttributeSubscriptions,
                Succeeded = true
            });
        }

        private async Task<FetchAggregatedAttributeDataResponse> ReceiveFetchAggregatedAttributeDataRequest(FetchAggregatedAttributeDataRequest request)
        {
            var dataRanges = await _persistence.GetData(request.EntityGuid, request.AttributeGuid, request.AggregationSeconds, request.TimeRange);
            return new FetchAggregatedAttributeDataResponse
            {
                ActionId = request.ActionId,
                MessageClass = OutgoingMessageClass.Response,
                ResponseType = ResponseType.FetchAggregatedAttributeData,
                Ranges = dataRanges,
                Succeeded = true
            };
        }

        public void SetSender(IWebsocketsSender sender)
        {
            _sender = sender;
        }

        public Task NotifyRaw(Guid entity, Guid attribute, RawDataRange dataRange)
        {
            string message = null;

            foreach (var subscription in _subscriptions)
            {
                if (!subscription.Value.TryGetValue(entity, out HashSet<Guid> attributes))
                {
                    continue;
                }

                if (!attributes.Contains(attribute))
                {
                    continue;
                }

                if (message == null)
                {
                    var notification = new ReceiveAttributeRawDataNotification
                    {
                        MessageClass = OutgoingMessageClass.Notification,
                        NotificationType = NotificationType.AttributeRawData,
                        EntityGuid = entity,
                        AttributeGuid = attribute,
                        DataRange = dataRange
                    };
                    message = JsonConvert.SerializeObject(notification, JsonSettings.Setttings);
                }
                
                _sender.Send(subscription.Key, message);
            }

            return Task.CompletedTask;
        }

        public Task NotifyAggregated(Guid entity, Guid attribute, Dictionary<int, AggregatedDataRange> dataRanges)
        {
            string message = null;

            foreach (var subscription in _subscriptions)
            {
                if (!subscription.Value.TryGetValue(entity, out HashSet<Guid> attributes))
                {
                    continue;
                }

                if (!attributes.Contains(attribute))
                {
                    continue;
                }

                if (message == null)
                {
                    var notification = new ReceiveAttributeAggregatedDataNotification
                    {
                        MessageClass = OutgoingMessageClass.Notification,
                        NotificationType = NotificationType.AttributeAggregatedData,
                        EntityGuid = entity,
                        AttributeGuid = attribute,
                        AggregatedRanges = dataRanges
                    };
                    message = JsonConvert.SerializeObject(notification, JsonSettings.Setttings);
                }

                _sender.Send(subscription.Key, message);
            }

            return Task.CompletedTask;
        }

        public ConcurrentDictionary<string, Dictionary<Guid, HashSet<Guid>>> GetSubscriptions()
        {
            return _subscriptions;
        }
    }
}
