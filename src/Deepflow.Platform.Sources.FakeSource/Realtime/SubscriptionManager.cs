using System;
using System.Collections.Concurrent;
using System.Threading.Tasks;
using Deepflow.Platform.Abstractions.Realtime;
using Deepflow.Platform.Abstractions.Series;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace Deepflow.Platform.Sources.FakeSource.Realtime
{
    public class SubscriptionManager : IWebsocketsReceiver
    {
        private readonly ILogger<SubscriptionManager> _logger;
        private IWebsocketsSender _sender;
        private readonly ConcurrentDictionary<string, RealtimeGenerator> _generatorBySocketId = new ConcurrentDictionary<string, RealtimeGenerator>();

        public SubscriptionManager(ILogger<SubscriptionManager> logger)
        {
            _logger = logger;
        }

        public Task OnConnected(string socketId)
        {
            return Task.FromResult(0);
        }

        public Task OnDisconnected(string socketId)
        {
            _generatorBySocketId.TryRemove(socketId, out RealtimeGenerator generator);
            
            if (generator != null)
            {
                generator.Stop();
                _logger.LogInformation($"Subscription removed, now {_generatorBySocketId.Count}");
            }
            return Task.FromResult(0);
        }

        public Task OnReceive(string socketId, string message)
        {
            var subscriptionRequest = JsonConvert.DeserializeObject<SubscriptionRequest>(message);
            var generator = _generatorBySocketId.GetOrAdd(socketId, new RealtimeGenerator(subscriptionRequest.SourceName, 30, async dataRange => await SendPoint(socketId, dataRange)));
            generator.Start();
            _logger.LogInformation($"Subscription added, now {_generatorBySocketId.Count}");
            return Task.FromResult(0);
        }

        public void SetSender(IWebsocketsSender sender)
        {
            _sender = sender;
        }

        private async Task SendPoint(string socketId, DataRange dataRange)
        {
            try
            {
                _logger.LogInformation("Sending raw point...");
                var message = JsonConvert.SerializeObject(dataRange);
                await _sender.Send(socketId, message);
            }
            catch (Exception exception)
            {
                _logger.LogError(null, exception, "Error sending raw point");
            }
        }
    }
}