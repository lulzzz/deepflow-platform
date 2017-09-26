using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Deepflow.Platform.Abstractions.Series;
using Deepflow.Platform.Core.Tools;
using Newtonsoft.Json;
using Deepflow.Platform.Realtime;
using Microsoft.Extensions.Logging;

namespace Deepflow.Platform.Agent.Provider
{
    public class FakeSourceDataProvider : ISourceDataProvider
    {
        private readonly FakeSourceConfiguration _configuration;
        private readonly ILogger<FakeSourceDataProvider> _logger;
        private readonly HttpClient _client = new HttpClient();

        public FakeSourceDataProvider(FakeSourceConfiguration configuration, ILogger<FakeSourceDataProvider> logger)
        {
            _configuration = configuration;
            _logger = logger;
        }

        public async Task<AggregatedDataRange> FetchAggregatedData(string sourceName, TimeRange timeRange, int aggregationSeconds)
        {
            var uri = new Uri(_configuration.ApiBaseUrl, $"api/v1/Data/{sourceName}/aggregations/{aggregationSeconds}?minTimeUtc={timeRange.Min.FromSecondsSince1970Utc():s}&maxTimeUtc={timeRange.Max.FromSecondsSince1970Utc():s}");
            var responseMessage = await _client.GetStringAsync(uri);
            return JsonConvert.DeserializeObject<AggregatedDataRange>(responseMessage);
        }

        public Task<RawDataRange> FetchRawData(string sourceName, TimeRange timeRange)
        {
            throw new NotImplementedException();
        }

        public async Task SubscribeForRawData(string sourceName, CancellationToken cancellationToken, Func<RawDataRange, Task> onReceive)
        {
            var uri = new Uri(_configuration.RealtimeBaseUrl, "ws/v1");
            WebsocketClientManager client = null;
            client = new WebsocketClientManager(uri, "Fake Source", 1, () => OnConnected(client, sourceName), messageString => OnReceive(messageString, onReceive), _logger);
            await client.ListenForServer(cancellationToken);
        }

        private async Task OnConnected(WebsocketClientManager client, string sourceName)
        {
            var message = JsonConvert.SerializeObject(new { SourceName = sourceName });
            await client.SendMessage(message);
        }

        private async Task OnReceive(string messageString, Func<RawDataRange, Task> onReceive)
        {
            var dataRange = JsonConvert.DeserializeObject<RawDataRange>(messageString);
            await onReceive(dataRange);
        }
    }
}
