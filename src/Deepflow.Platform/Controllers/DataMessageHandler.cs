﻿using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Deepflow.Platform.Abstractions.Realtime;
using Deepflow.Platform.Abstractions.Series;
using Deepflow.Platform.Series;
using Newtonsoft.Json;
using Orleans;

namespace Deepflow.Platform.Controllers
{
    public class DataMessageHandler : IWebsocketsReceiver, IDataSender
    {
        private IWebsocketsSender _sender;
        private readonly ConcurrentDictionary<string, IEnumerable<DataSubscription>> _socketSubscriptions = new ConcurrentDictionary<string, IEnumerable<DataSubscription>>();
        
        public Task OnConnected(string socketId)
        {
            return Task.FromResult(0);
        }

        public Task OnDisconnected(string socketId)
        {
            return Task.FromResult(0);
        }

        public async Task OnReceive(string socketId, string message)
        {
            var request = JsonConvert.DeserializeObject<DataRequestMessage>(message);

            /*var subscriptions = _socketSubscriptions.GetOrAdd(socketId, new List<DataSubscription>());

            lock (subscriptions)
            {
                var subscriptionsToAdd = request.Subscriptions.Except(subscriptions);
                var subscriptionsToRemove = subscriptions.Except(request.Subscriptions);

                var addTasks = subscriptionsToAdd.Select(subscription => SubscribeToAttribute(socketId, subscription));
                var removeTasks = subscriptionsToRemove.Select(UnsubscribeFromAttribute);

                return Task.WhenAll(addTasks.Concat(removeTasks));
            }*/

            if (request.Request != null)
            {
                var response = await HandleDataRequest(request.Request).ConfigureAwait(false);
                var responseText = JsonConvert.SerializeObject(response);
                await _sender.Send(socketId, responseText).ConfigureAwait(false);
            }
        }

        public void SetSender(IWebsocketsSender sender)
        {
            _sender = sender;
        }

        public Task<DataResponse> HandleDataRequest(DataRequest request)
        {
            if (request.Type == DataRequestType.Attribute)
            {
                return HandleAttributeDataRequest(request);
            }
            throw new Exception($"Cannot handle data request for {request.Type}");
        }

        public async Task<DataResponse> HandleAttributeDataRequest(DataRequest request)
        {
            var series = GrainClient.GrainFactory.GetGrain<IAttributeSeriesGrain>(SeriesIdHelper.ToAttributeSeriesId(request.Entity, request.Attribute));
            var data = await series.GetData(new TimeRange(request.MinSeconds, request.MaxSeconds), request.AggregationSeconds).ConfigureAwait(false);
            return new DataResponse { Id = request.Id, Data = data };
        }

        private async Task SubscribeToAttribute(string socketId, DataSubscription subscription)
        {
            var series = GrainClient.GrainFactory.GetGrain<IAttributeSeriesGrain>(SeriesIdHelper.ToAttributeSeriesId(subscription.Entity, subscription.Attribute));
            var observer = new SeriesObserver(socketId, this);
            var observerRef = await GrainClient.GrainFactory.CreateObjectReference<ISeriesObserver>(observer);
            await series.Subscribe(observerRef);
        }

        private async Task UnsubscribeFromAttribute(DataSubscription subscription)
        {
            var series = GrainClient.GrainFactory.GetGrain<IAttributeSeriesGrain>(SeriesIdHelper.ToCalculationSeriesId(subscription.Entity, subscription.Attribute));
            var observerRef = await GrainClient.GrainFactory.CreateObjectReference<ISeriesObserver>(subscription.Observer);
            await series.Unsubscribe(observerRef);
        }

        public void SendData(string socketId, Guid entity, Guid attribute, IEnumerable<AggregatedDataRange> dataRanges)
        {
            var message = JsonConvert.SerializeObject(new DataSubscriptionMessage { Entity = entity, Attribute = attribute, DataRanges = dataRanges });
            _sender.Send(socketId, message);
        }
    }
}
