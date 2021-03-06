﻿using System;
using System.Collections.Concurrent;
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
        private readonly TripCounterFactory _tripCounterFactory;
        private WebsocketClientManager _listenClient;
        private readonly RetryingHttpClient _pushClient;
        private readonly HttpClient _pullClient = new HttpClient();
        private SourceSeriesList _sourceSeriesList;
        private CancellationTokenSource _listenCancellationTokenSource = new CancellationTokenSource();
        private CancellationToken _listenCancellationToken;

        public IngestionClient(ILogger<IngestionClient> logger, AgentIngestionConfiguration configuration, IAgentProcessor processor, TripCounterFactory tripCounterFactory)
        {
            _logger = logger;
            _configuration = configuration;
            _processor = processor;
            _tripCounterFactory = tripCounterFactory;
            _processor.SetClient(this);
            _pushClient = new RetryingHttpClient(configuration.PushFailedRetryCount, _configuration.PushFailedPauseSeconds, "Unable to push data to ingestion API", logger);

            _logger.LogDebug($"Starting ingestion client with {configuration.SendParallelism} send parallelism");
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
            if (_configuration.Disabled)
            {
                return;
            }
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
                var uri = new Uri(_configuration.ApiBaseUrl, $"api/v1/DataSources/{_configuration.DataSource}/Tags");
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

        public async Task SendRealtimeAggregatedData(string name, AggregatedDataRange dataRange)
        {
            await _tripCounterFactory.Run("IngestionClient.SendRealtimeAggregatedData", async () =>
            {
                _logger.LogDebug("About to send realtime data to ingestion API");

                var message = JsonConvert.SerializeObject(dataRange, JsonSettings.Setttings);
                var uri = new Uri(_configuration.ApiBaseUrl, $"api/v1/DataSources/{_configuration.DataSource}/Tags/{name}/Data/Aggregations/{dataRange.AggregationSeconds}/Realtime");
                await _pushClient.PostAsync(uri, new StringContent(message, Encoding.UTF8, "application/json"));
            });
        }

        public async Task SendRealtimeRawData(string name, RawDataRange dataRange)
        {
            await _tripCounterFactory.Run("IngestionClient.SendRealtimeRawData", async () =>
            {
                _logger.LogDebug("About to send realtime raw data to ingestion API");

                var message = JsonConvert.SerializeObject(dataRange, JsonSettings.Setttings);
                var uri = new Uri(_configuration.ApiBaseUrl, $"api/v1/DataSources/{_configuration.DataSource}/Tags/{name}/Data/Raw/Realtime");
                await _pushClient.PostAsync(uri, new StringContent(message, Encoding.UTF8, "application/json"));
            });
        }

        public async Task SendHistoricalData(string name, AggregatedDataRange aggregatedDataRange)
        {
            await _tripCounterFactory.Run("IngestionClient.SendHistoricalData", async () =>
            {
                _logger.LogDebug("About to send historical data to ingestion API");

                var message = JsonConvert.SerializeObject(aggregatedDataRange, JsonSettings.Setttings);
                var uri = new Uri(_configuration.ApiBaseUrl, $"api/v1/DataSources/{_configuration.DataSource}/Tags/{name}/Data/Aggregations/{aggregatedDataRange.AggregationSeconds}/Historical");
                await _pushClient.PostAsync(uri, new StringContent(message, Encoding.UTF8, "application/json"));

                _logger.LogDebug("Sent data to ingestion API");

                /*var message = new AddAggregatedAttributeHistoricalDataRequest()
                {
                    ActionId = _nextActionId++,
                    MessageClass = IncomingMessageClass.Request,
                    RequestType = RequestType.AddAggregatedHistoricalAttributeData,
                    DataSource = _configuration.DataSource,
                    SourceName = name,
                    AggregatedDataRange = aggregatedDataRange
                };

                var messageString = JsonConvert.SerializeObject(message, JsonSettings.Setttings);
                _logger.LogDebug("About to send message to ingestion API");

                try
                {
                    _logger.LogDebug("Waiting to send data to ingestion API");
                    await _sendSemaphore.WaitAsync();
                    _logger.LogDebug("Sending data to ingestion API");
                    await _listenClient.SendMessage(messageString);
                    _logger.LogDebug("Sent data to ingestion API");
                }
                finally
                {
                    _sendSemaphore.Release();
                }*/
            });
        }
    }
}
