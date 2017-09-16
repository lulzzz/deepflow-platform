using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Deepflow.Platform.Abstractions.Series;
using Deepflow.Platform.Abstractions.Sources;
using Deepflow.Platform.Agent.Core;
using Deepflow.Platform.Agent.Processor;
using Deepflow.Platform.Core.Tools;
using Deepflow.Platform.Realtime;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace Deepflow.Platform.Agent.Client
{
    public class IngestionClient : IIngestionClient
    {
        private readonly ILogger<IngestionClient> _logger;
        private readonly Core.AgentIngestionConfiguration _configuration;
        private readonly IAgentProcessor _processor;
        private WebsocketClientManager _listenClient;
        private readonly RetryingHttpClient _pushClient;
        private readonly HttpClient _pullClient = new HttpClient();
        private SourceSeriesList _sourceSeriesList;
        private CancellationTokenSource _listenCancellationTokenSource = new CancellationTokenSource();
        private CancellationToken _listenCancellationToken;

        public IngestionClient(ILogger<IngestionClient> logger, Core.AgentIngestionConfiguration configuration, IAgentProcessor processor)
        {
            _logger = logger;
            _configuration = configuration;
            _processor = processor;
            _processor.SetClient(this);
            _pushClient = new RetryingHttpClient(configuration.PushFailedRetryCount, _configuration.PushFailedPauseSeconds, "Unable to push data to ingestion API", logger);
        }

        public async Task Start()
        {
            var uri = new Uri(_configuration.RealtimeBaseUrl, "ws/v1");
            _listenClient = new WebsocketClientManager(uri, "Ingestion API", _configuration.ClientRetrySeconds, ListenConnected, ListenReceive, _logger);

            _listenCancellationTokenSource = new CancellationTokenSource();
            _listenCancellationToken = _listenCancellationTokenSource.Token;
            await _listenClient.ListenForServer(_listenCancellationToken);
        }

        public void Stop()
        {
            _listenCancellationTokenSource.Cancel();
        }

        private async Task ListenConnected()
        {
            _sourceSeriesList = await FetchSourceSeriesList();
            _processor.SetSourceSeriesList(_sourceSeriesList);
        }

        private Task ListenReceive(string messageString)
        {
            var message = JsonConvert.DeserializeObject<IngestionClientMessage>(messageString);
            if (message.Type == IngestionClientMessage.SourceSeriesListMessage)
            {
                var sourceSeriesListMessage = JsonConvert.DeserializeObject<SourceSeriesListMessage>(messageString);
                _processor.SetSourceSeriesList(sourceSeriesListMessage.SourceSeriesList);
            }
            return Task.FromResult(0);
        }

        public async Task<SourceSeriesList> FetchSourceSeriesList()
        {
            while (true)
            {
                var uri = new Uri(_configuration.ApiBaseUrl, $"api/v1/DataSources/{_configuration.DataSource}/Series");
                try
                {
                    var responseString = await _pullClient.GetStringAsync(uri);
                    return JsonConvert.DeserializeObject<SourceSeriesList>(responseString);
                }
                catch (Exception exception)
                {
                    _logger.LogDebug(new EventId(1001), exception, $"Error fetching source series list from Ingestion API at {uri}, trying again in {_configuration.ClientRetrySeconds} seconds...");
                    await Task.Delay(_configuration.ClientRetrySeconds * 1000, _listenCancellationToken);
                }
            }
        }

        public Task SendAggregatedRange(string name, DataRange dataRange, int aggregationSeconds)
        {
            return SendAggregatedRanges(name, new List<DataRange> { dataRange }, aggregationSeconds);
        }

        public Task SendRawRange(string name, DataRange dataRange)
        {
            return SendRawRanges(name, new List<DataRange> { dataRange });
        }

        public async Task SendRawRanges(string name, IEnumerable<DataRange> dataRanges)
        {
            var uri = new Uri(_configuration.ApiBaseUrl, $"/api/v1/DataSources/{_configuration.DataSource}/Series/{name}/Raw/Data");
            var body = JsonConvert.SerializeObject(dataRanges);
            await _pushClient.PostAsync(uri, new StringContent(body, Encoding.UTF8, "application/json"));
        }

        public async Task SendAggregatedRanges(string name, IEnumerable<DataRange> dataRanges, int aggregationSeconds)
        {
            var uri = new Uri(_configuration.ApiBaseUrl, $"/api/v1/DataSources/{_configuration.DataSource}/Series/{name}/Aggregations/{aggregationSeconds}/Data");
            var body = JsonConvert.SerializeObject(dataRanges);
            await _pushClient.PostAsync(uri, new StringContent(body, Encoding.UTF8, "application/json"));
        }
    }
}
