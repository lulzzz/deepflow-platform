using System;
using System.Collections.Concurrent;
using System.Threading.Tasks;
using Deepflow.Platform.Abstractions.Realtime;
using Deepflow.Platform.Abstractions.Series;
using Deepflow.Platform.Series;
using Deepflow.Platform.Sources.FakeSource.Data;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace Deepflow.Platform.Sources.FakeSource.Realtime
{
    public class SubscriptionManager : IWebsocketsReceiver
    {
        private readonly ILogger<SubscriptionManager> _logger;
        private readonly ILogger<RangeJoiner<RawDataRange>> _rangeLogger;
        private readonly GeneratorConfiguration _configuration;
        private readonly IDataGenerator _generator;
        private IWebsocketsSender _sender;
        private readonly ConcurrentDictionary<string, RealtimeGenerator> _generatorBySocketId = new ConcurrentDictionary<string, RealtimeGenerator>();

        public SubscriptionManager(ILogger<SubscriptionManager> logger, ILogger<RangeJoiner<RawDataRange>> rangeLogger, GeneratorConfiguration configuration, IDataGenerator generator)
        {
            _logger = logger;
            _rangeLogger = rangeLogger;
            _configuration = configuration;
            _generator = generator;
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
                _logger.LogDebug($"Subscription removed, now {_generatorBySocketId.Count}");
            }
            return Task.FromResult(0);
        }

        public Task OnReceive(string socketId, string message)
        {
            var subscriptionRequest = JsonConvert.DeserializeObject<SubscriptionRequest>(message);
            var generator = _generatorBySocketId.GetOrAdd(socketId, new RealtimeGenerator(subscriptionRequest.SourceName, _configuration.SecondsInterval, async dataRange => await SendPoint(socketId, dataRange), _rangeLogger, _generator));
            generator.Start();
            _logger.LogDebug($"Subscription added, now {_generatorBySocketId.Count}");
            return Task.FromResult(0);
        }

        public void SetSender(IWebsocketsSender sender)
        {
            _sender = sender;
        }

        private async Task SendPoint(string socketId, RawDataRange dataRange)
        {
            try
            {
                _logger.LogDebug("Sending raw point...");
                var message = JsonConvert.SerializeObject(dataRange);
                await _sender.Send(socketId, message);
            }
            catch (Exception exception)
            {
                _logger.LogError(new EventId(106), exception, "Error sending raw point");
            }
        }
    }
}