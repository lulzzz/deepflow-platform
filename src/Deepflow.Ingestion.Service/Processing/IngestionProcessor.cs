using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Deepflow.Common.Model;
using Deepflow.Common.Model.Model;
using Deepflow.Ingestion.Service.Metrics;
using Deepflow.Platform.Abstractions.Series;
using Deepflow.Ingestion.Service.Realtime;
using Deepflow.Platform.Abstractions.Series.Validators;
using Deepflow.Platform.Common.Data.Persistence;
using Deepflow.Platform.Core.Tools;
using FluentValidation;
using Microsoft.Extensions.Logging;

namespace Deepflow.Ingestion.Service.Processing
{
    public class IngestionProcessor : IIngestionProcessor
    {
        private readonly IPersistentDataProvider _persistence;
        private readonly IDataAggregator _aggregator;
        private readonly IModelProvider _model;
        private readonly IDataMessenger _messenger;
        private readonly IRangeMerger<AggregatedDataRange> _aggregatedMerger;
        private readonly IRangeMerger<TimeRange> _timeMerger;
        private readonly IRangeFilterer<AggregatedDataRange> _filterer;
        private readonly SeriesConfiguration _configuration;
        private readonly ILogger<IngestionProcessor> _logger;

        private readonly TripCounterFactory _tripCounterFactory;
        private readonly IMetricsReporter _metrics;
        private readonly ConcurrentDictionary<Guid, SemaphoreSlim> _seriesSemaphores = new ConcurrentDictionary<Guid, SemaphoreSlim>();
        private readonly AggregatedDataRangeValidator _aggregatedDataRangeValidator = new AggregatedDataRangeValidator();
        private int _id = 0;

        public IngestionProcessor(IPersistentDataProvider persistence, IDataAggregator aggregator, IModelProvider model, IDataMessenger messenger, IRangeMerger<AggregatedDataRange> aggregatedMerger, IRangeMerger<TimeRange> timeMerger, IRangeFilterer<AggregatedDataRange> filterer, SeriesConfiguration configuration, ILogger<IngestionProcessor> logger, TripCounterFactory tripCounterFactory, IMetricsReporter metrics)
        {
            _persistence = persistence;
            _aggregator = aggregator;
            _model = model;
            _messenger = messenger;
            _aggregatedMerger = aggregatedMerger;
            _timeMerger = timeMerger;
            _filterer = filterer;
            _configuration = configuration;
            _logger = logger;
            _tripCounterFactory = tripCounterFactory;
            _metrics = metrics;
        }

        public async Task ReceiveRealtimeRawData(Guid entity, Guid attribute, RawDataRange rawDataRange)
        {
            _logger.LogDebug("Received realtime raw data");

            await _metrics.Run("realtimesubmissions", async () =>
            {
                await _messenger.NotifyRaw(entity, attribute, rawDataRange);
            });

            _logger.LogDebug("Saved realtime raw data");
        }

        public async Task ReceiveRealtimeAggregatedData(Guid entity, Guid attribute, AggregatedDataRange dataRange)
        {
            _logger.LogDebug("Received realtime aggregated data");

            await _metrics.Run("realtimesubmissions", async () =>
            {
                var affectedAggregatedPoints = await SaveHistoricalData(entity, attribute, dataRange);
                await _messenger.NotifyAggregated(entity, attribute, affectedAggregatedPoints);
            });

            _logger.LogDebug("Saved realtime aggregated data");
        }

        public async Task ReceiveHistoricalData(Guid entity, Guid attribute, AggregatedDataRange dataRange)
        {
            await _metrics.Run("historicalsubmissions", async () =>
            {
                _logger.LogDebug($"Received historical data with {dataRange?.Data.Count / 2} points");
                await SaveHistoricalData(entity, attribute, dataRange);
            });
        }

        private async Task<Dictionary<int, AggregatedDataRange>> SaveHistoricalData(Guid entity, Guid attribute, AggregatedDataRange dataRange)
        {
            _aggregatedDataRangeValidator.ValidateAndThrow(dataRange);

            var minAggregationSeconds = _configuration.AggregationsSeconds.Min();
            if (dataRange.AggregationSeconds != minAggregationSeconds)
            {
                throw new Exception($"Must be lowest aggregation {minAggregationSeconds}");
            }

            var series = await _model.ResolveSeries(entity, attribute, minAggregationSeconds).ConfigureAwait(false);
            _logger.LogDebug("Resolved series");
            var maxAggregationSeconds = _configuration.AggregationsSeconds.Max();
            var quantised = dataRange.TimeRange.Quantise(maxAggregationSeconds);

            var semaphore = _seriesSemaphores.GetOrAdd(series, s => new SemaphoreSlim(1));
            await semaphore.WaitAsync().ConfigureAwait(false);
            try
            {
                //var cached = await _cache.GetData(series, quantised, minAggregationSeconds);
                var persisted = await _tripCounterFactory.Run("Persistence.GetData", _persistence.GetData(entity, attribute, dataRange.AggregationSeconds, quantised));
                var existingTimeRanges = await _tripCounterFactory.Run("Persistence.GetAllTimeRanges", _persistence.GetAllTimeRanges(entity, attribute)).ConfigureAwait(false);
                _logger.LogDebug("Got cached");
                var merged = _aggregatedMerger.MergeRangeWithRanges(persisted, dataRange);
                var aggregations = _aggregator.Aggregate(merged, quantised, _configuration.AggregationsSeconds);
                var affectedAggregatedPoints = aggregations.Select(x => FilterAggregationToAffectedRange(x.Value, dataRange.TimeRange, x.Key)).Where(x => x != null).ToList();
                var rangesToPersist = affectedAggregatedPoints.Select(range => new ValueTuple<Guid, Guid, int, IEnumerable<AggregatedDataRange>>(entity, attribute, range.AggregationSeconds, new List<AggregatedDataRange> { range }));
                await _persistence.SaveData(rangesToPersist);

                //await Task.WhenAll(_tripCounterFactory.Run("Persistence.Save", aggregations.Select(async x => _persistence.SaveData(await _model.ResolveSeries(entity, attribute, x.Key).ConfigureAwait(false), x.Value)))).ConfigureAwait(false);
                //var existingTimeRanges = await existingTimeRangesTask.ConfigureAwait(false); ;
                var afterTimeRanges = _timeMerger.MergeRangeWithRanges(existingTimeRanges, dataRange.TimeRange);
                await _tripCounterFactory.Run("Persistence.SaveTimeRanges", _persistence.SaveTimeRanges(entity, attribute, afterTimeRanges)).ConfigureAwait(false);

                //File.WriteAllText(_id++ + ".json", $"Before: {JsonConvert.SerializeObject(existingTimeRanges)} {Environment.NewLine}{Environment.NewLine}Adding:{JsonConvert.SerializeObject(dataRange.TimeRange)}{Environment.NewLine}{Environment.NewLine}After: {JsonConvert.SerializeObject(afterTimeRanges)}");

                //var lowestAggregation = aggregations.OrderBy(x => x.Key).First();
                //var cache = _tripCounterFactory.Run("Cache.Save", _cache.SaveData(series, lowestAggregation.Value));
                //await Task.WhenAll(dataPersistence, timeRangePersistence).ConfigureAwait(false); ;
                _logger.LogDebug("Saved persistence");

                return affectedAggregatedPoints.ToDictionary(x => x.AggregationSeconds, x => x);
            }
            finally
            {
                semaphore.Release();
            }
        }

        private AggregatedDataRange FilterAggregationToAffectedRange(IEnumerable<AggregatedDataRange> dataRanges, TimeRange timeRange, int aggregationSeconds)
        {
            var filtered = _filterer.FilterRanges(dataRanges, timeRange.Quantise(aggregationSeconds));
            return filtered.SingleOrDefault();
        }
    }
}
