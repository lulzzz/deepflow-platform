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
            if (string.IsNullOrEmpty(sourceName))
            {
                throw new ArgumentNullException(nameof(sourceName));
            }
            if (timeRange == null)
            {
                throw new ArgumentNullException(nameof(timeRange));
            }
            if (aggregationSeconds == 0)
            {
                throw new InvalidOperationException("aggregationSeconds is 0");
            }

            var uri = new Uri(_configuration.BaseUrl, $"api/v1/Tags/{sourceName}/Aggregations/{aggregationSeconds}/Data?minTime={timeRange.Min.ToDateTime()}&maxTime={timeRange.Min.ToDateTime()}");
            var response = await _client.GetAsync(uri);
            response.EnsureSuccessStatusCode();
            if (response.Content == null)
            {
                throw new Exception("Response content from PI Sim Cassandra was null");
            }
            var responseText = await response.Content.ReadAsStringAsync();
            if (string.IsNullOrEmpty(responseText))
            {
                throw new Exception("Response content from PI Sim Cassandra was empty");
            }
            return JsonConvert.DeserializeObject<AggregatedDataRange>(responseText);
        }

        public async Task<RawDataRange> FetchRawDataWithEdges(string sourceName, TimeRange timeRange)
        {
            var uri = new Uri(_configuration.BaseUrl, $"api/v1/Tags/{sourceName}/Raw/Data?minTime={timeRange.Min.ToDateTime()}&maxTime={timeRange.Min.ToDateTime()}");
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
