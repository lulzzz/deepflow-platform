using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Deepflow.Platform.Abstractions.Series;
using Deepflow.Platform.Core.Tools;
using Newtonsoft.Json;

namespace Deepflow.Platform.Agent.Provider
{
    public class PiSimDataProvider : ISourceDataProvider
    {
        private readonly PiSimConfiguration _configuration;
        private readonly HttpClient _client = new HttpClient();

        public PiSimDataProvider(PiSimConfiguration configuration)
        {
            _configuration = configuration;
        }

        public async Task<AggregatedDataRange> FetchAggregatedData(string sourceName, TimeRange timeRange, int aggregationSeconds)
        {
            var uri = new Uri(_configuration.BaseUrl, $"Tags/{sourceName}/Aggregations/{aggregationSeconds}/Data?minTime={timeRange.Min.FromSecondsSince1970Utc()}&maxTime={timeRange.Min.FromSecondsSince1970Utc()}");
            var response = await _client.GetAsync(uri);
            var responseText = await response.Content.ReadAsStringAsync();
            return JsonConvert.DeserializeObject<AggregatedDataRange>(responseText);
        }

        public async Task<RawDataRange> FetchRawData(string sourceName, TimeRange timeRange)
        {
            var uri = new Uri(_configuration.BaseUrl, $"Tags/{sourceName}/Raw/Data?minTime={timeRange.Min.FromSecondsSince1970Utc()}&maxTime={timeRange.Min.FromSecondsSince1970Utc()}");
            var response = await _client.GetAsync(uri);
            var responseText = await response.Content.ReadAsStringAsync();
            return JsonConvert.DeserializeObject<RawDataRange>(responseText);
        }

        public Task SubscribeForRawData(string sourceName, CancellationToken cancellationToken, Func<RawDataRange, Task> onReceive)
        {
            return Task.CompletedTask;
        }
    }
}
