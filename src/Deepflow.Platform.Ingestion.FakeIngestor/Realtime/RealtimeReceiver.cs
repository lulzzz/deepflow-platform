using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Deepflow.Platform.Abstractions.Realtime;
using Deepflow.Platform.Abstractions.Series;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace Deepflow.Platform.Ingestion.FakeIngestor.Realtime
{
    public class RealtimeReceiver : IWebsocketsReceiver
    {
        private readonly ILogger<RealtimeReceiver> _logger;

        public RealtimeReceiver(ILogger<RealtimeReceiver> logger)
        {
            _logger = logger;
        }

        public Task OnConnected(string socketId)
        {
            _logger.LogInformation("Socket connected");
            return Task.FromResult(0);
        }

        public Task OnDisconnected(string socketId)
        {
            _logger.LogInformation("Socket disconnected");
            return Task.FromResult(0);
        }

        public Task OnReceive(string socketId, string messageString)
        {
            var message = JsonConvert.DeserializeObject<IEnumerable<DataRange>>(messageString);
            _logger.LogInformation($"Received message with {message.Sum(x => x.Data.Count / 2)} points");
            return Task.FromResult(0);
        }

        public void SetSender(IWebsocketsSender sender)
        {
            
        }
    }
}
