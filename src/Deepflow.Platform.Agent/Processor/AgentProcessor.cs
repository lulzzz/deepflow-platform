using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Deepflow.Platform.Abstractions.Series;
using Deepflow.Platform.Abstractions.Sources;
using Deepflow.Platform.Agent.Client;
using Deepflow.Platform.Agent.Provider;
using Deepflow.Platform.Core.Tools;
using Microsoft.Extensions.Logging;

namespace Deepflow.Platform.Agent.Processor
{
    public class AgentProcessor : IAgentProcessor
    {
        private readonly ILogger<AgentProcessor> _logger;
        private readonly Core.AgentIngestionConfiguration _configuration;
        private readonly ISourceDataProvider _provider;
        private IIngestionClient _client;
        private readonly ConcurrentDictionary<string, CancellationTokenSource> _subscriptions = new ConcurrentDictionary<string, CancellationTokenSource>();
        private ConcurrentDictionary<string, AggregationWorker> _workers = new ConcurrentDictionary<string, AggregationWorker>();
        private readonly object _subscriptionsLock = new object();

        private QueueWrapper _fetchQueue = new QueueWrapper();

        public AgentProcessor(ILogger<AgentProcessor> logger, Core.AgentIngestionConfiguration configuration, ISourceDataProvider provider)
        {
            _logger = logger;
            _configuration = configuration;
            _provider = provider;

            Task.Run(async () =>
            {
                while (true)
                {
                    await Task.Delay(3000);
                    _logger.LogWarning($"{_fetchQueue.Queue.Count} remaining in queue");
                }
            });
        }

        public void SetClient(IIngestionClient client)
        {
            _client = client;
        }

        public void Start()
        {
            Task.Run(() => FetchLoop());
        }

        public void SetSourceSeriesList(SourceSeriesList sourceSeriesList)
        {
            lock (_subscriptionsLock)
            {
                var nextSourceNames = new HashSet<string>(sourceSeriesList.Series.Where(x => x.Realtime).Select(x => x.SourceName));

                var newSourceNames = nextSourceNames.Where(nextSourceName => !_subscriptions.ContainsKey(nextSourceName));
                var newSubscriptions = newSourceNames.Select(x => new NewSubscription { CancellationTokenSource = new CancellationTokenSource(), SourceName = x });
                foreach (var newSubscription in newSubscriptions)
                {
                    _subscriptions.TryAdd(newSubscription.SourceName, newSubscription.CancellationTokenSource);
                    _provider.SubscribeForRawData(newSubscription.SourceName, newSubscription.CancellationTokenSource.Token, dataRange => ReceiveRealtimeRaw(newSubscription.SourceName, dataRange));
                    _workers.GetOrAdd(newSubscription.SourceName, sourceName => new AggregationWorker(sourceName, _configuration.AggregationSeconds, _configuration.MaximumSourceDelaySeconds, _provider, dataRange => ReceiveRealtimeAggregates(sourceName, dataRange)));
                }

                var removeSourceNames = _subscriptions.Keys.Where(existingSourceName => !nextSourceNames.Contains(existingSourceName));
                foreach (var sourceName in removeSourceNames)
                {
                    _subscriptions.TryRemove(sourceName, out CancellationTokenSource cancellationTokenSource);
                    cancellationTokenSource.Cancel();
                }
            }

            var nextQueue = PrepareNextQueue(sourceSeriesList);
            SwapToNextQueue(nextQueue);
        }

        private class NewSubscription
        {
            public CancellationTokenSource CancellationTokenSource;
            public string SourceName;
        }

        public async Task ReceiveRealtimeAggregates(string sourceName, AggregatedDataRange dataRange)
        {
            await _client.SendRealtimeAggregatedData(sourceName, dataRange);
        }

        public async Task ReceiveRealtimeRaw(string sourceName, RawDataRange dataRange)
        {
            await _client.SendRealtimeRawData(sourceName, dataRange);
        }

        private QueueWrapper PrepareNextQueue(SourceSeriesList sourceSeriesList)
        {
            var fetches = ListToFetches(sourceSeriesList);
            //fetches.Shuffle(); // Randomise the fetch order so we aren't always prioritising the same series

            var queue = new QueueWrapper();
            foreach (var ingestionFetch in fetches)
            {
                queue.Queue.Add(ingestionFetch);
            }
            return queue;
        }

        private void SwapToNextQueue(QueueWrapper newQueue)
        {
            _logger.LogInformation($"Switching to new queue with {newQueue.Queue.Count} items");

            var oldQueue = _fetchQueue;
            _fetchQueue = newQueue;
            oldQueue.CancellationTokenSource.Cancel();
        }

        private async void FetchLoop()
        {
            while (true)
            {
                try
                {
                    var queue = _fetchQueue;
                    IngestionFetch fetch = queue.Queue.Take(queue.CancellationToken);
                    try
                    {
                        var data = await _provider.FetchAggregatedData(fetch.SourceName, fetch.TimeRange, _configuration.AggregationSeconds);
                        await _client.SendHistoricalData(fetch.SourceName, data);
                        await Task.Delay(_configuration.BetweenFetchPauseSeconds);
                    }
                    catch (Exception exception)
                    {
                        _logger.LogDebug(new EventId(1003), exception, $"Error fetching data from source {_configuration.DataSourceFriendlyName}, placing back in the queue and trying next one after {_configuration.FetchFailedPauseSeconds} seconds...");
                        queue.Queue.Add(fetch);
                        await Task.Delay(_configuration.FetchFailedPauseSeconds * 1000);
                    }
                }
                catch (OperationCanceledException)
                {
                    // Queue was swapped. This happens all the time, like when we get a new source series list
                }
            }
        }

        private IList<IngestionFetch> ListToFetches(SourceSeriesList sourceSeriesList)
        {
            return sourceSeriesList.Series.Select(BuildFetchesForSeries).MergeMany().ToList();
        }

        private IEnumerable<IngestionFetch> BuildFetchesForSeries(SourceSeriesFetchRequests series)
        {
            return series.TimeRanges.SelectMany(range => BuildFetchesForTimeRange(series.SourceName, range)).Reverse();
        }

        private IEnumerable<IngestionFetch> BuildFetchesForTimeRange(string sourceName, TimeRange timeRange)
        {
            var quantisedRange = timeRange.Quantise(_configuration.MaxFetchSpanSeconds);
            return quantisedRange.Chop(_configuration.MaxFetchSpanSeconds).Select(range => new IngestionFetch(sourceName, range));
        }
    }

    public class IngestionFetch
    {
        public IngestionFetch(string sourceName, TimeRange timeRange)
        {
            SourceName = sourceName;
            TimeRange = timeRange;
        }

        public string SourceName { get; set; }
        public TimeRange TimeRange { get; set; }
    }

    public class QueueWrapper
    {
        public QueueWrapper()
        {
            CancellationTokenSource = new CancellationTokenSource();
            CancellationToken = CancellationTokenSource.Token;
        }

        public CancellationTokenSource CancellationTokenSource { get; }
        public CancellationToken CancellationToken { get; }
        public BlockingCollection<IngestionFetch> Queue = new BlockingCollection<IngestionFetch>();
    }

    public class SubscriptionWrapper
    {
        
    }
}
