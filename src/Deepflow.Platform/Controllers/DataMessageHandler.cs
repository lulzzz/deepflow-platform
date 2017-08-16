using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Deepflow.Platform.Abstractions.Realtime;
using Deepflow.Platform.Abstractions.Series;
using Deepflow.Platform.Realtime;
using Deepflow.Platform.Series;
using Newtonsoft.Json;
using Orleans;

namespace Deepflow.Platform.Controllers
{
    public class DataMessageHandler : IWebsocketsReceiver, IDataSender
    {
        private readonly IWebsocketsSender _sender;
        private readonly ConcurrentDictionary<string, IEnumerable<DataSubscription>> _socketSubscriptions = new ConcurrentDictionary<string, IEnumerable<DataSubscription>>();

        public DataMessageHandler(IWebsocketsSender sender)
        {
            _sender = sender;
        }

        public Task OnConnected(string socketId)
        {
            return Task.FromResult(0);
        }

        public Task OnDisconnected(string socketId)
        {
            return Task.FromResult(0);
        }

        public Task OnReceive(string socketId, string message)
        {
            var request = JsonConvert.DeserializeObject<DataRequestMessage>(message);

            var subscriptions = _socketSubscriptions.GetOrAdd(socketId, new List<DataSubscription>());

            lock (subscriptions)
            {
                var subscriptionsToAdd = request.Subscriptions.Except(subscriptions);
                var subscriptionsToRemove = subscriptions.Except(request.Subscriptions);

                var addTasks = subscriptionsToAdd.Select(subscription => Subscribe(socketId, subscription));
                var removeTasks = subscriptionsToRemove.Select(Unsubscribe);

                return Task.WhenAll(addTasks.Concat(removeTasks));
            }
        }

        private async Task Subscribe(string socketId, DataSubscription subscription)
        {
            var series = GrainClient.GrainFactory.GetGrain<ISeriesGrain>(SeriesIdHelper.ToSeriesId(subscription.Entity, subscription.Attribute));
            var observer = new SeriesObserver(socketId, this);
            var observerRef = await GrainClient.GrainFactory.CreateObjectReference<ISeriesObserver>(observer);
            await series.Subscribe(observerRef);
        }

        private async Task Unsubscribe(DataSubscription subscription)
        {
            var series = GrainClient.GrainFactory.GetGrain<ISeriesGrain>(SeriesIdHelper.ToSeriesId(subscription.Entity, subscription.Attribute));
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
