using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Deepflow.Platform.Abstractions.Series;
using Deepflow.Platform.Abstractions.Series.Attribute;
using Deepflow.Platform.Core.Async;
using Deepflow.Platform.Core.Tools;
using Microsoft.Extensions.Logging;

namespace Deepflow.Platform.Series.Attributes
{
    public class AttributeDataProvider : IAttributeDataProvider
    {
        private readonly Guid _entity;
        private readonly Guid _attribute;
        private readonly ISeriesConfiguration _configuration;
        private readonly ILogger<AttributeDataProvider> _logger;
        private readonly TripCounterFactory _tripCounterFactory;
        private readonly IDataStore _store;
        private readonly IDataValidator _validator;
        private readonly IDataAggregator _aggregator;
        private readonly ISeriesKnower _seriesKnower;
        private readonly IRangeFilterer<AggregatedDataRange> _filterer;
        private readonly IRangeMerger<AggregatedDataRange> _aggregatedMerger;
        private readonly IRangeMerger<TimeRange> _timeMerger;
        private readonly object _timeRangesCacheLock = new object();
        private readonly SemaphoreSlim _timeRangesCacheSemaphore = new SemaphoreSlim(1);
        private ConcurrentDictionary<Guid, List<TimeRange>> _timeRangesCache;

        public AttributeDataProvider(Guid entity, Guid attribute, IDataStore store, IDataValidator validator, IDataAggregator aggregator, ISeriesKnower seriesKnower, IRangeFilterer<AggregatedDataRange> filterer, IRangeMerger<AggregatedDataRange> aggregatedMerger, IRangeMerger<TimeRange> timeMerger, ISeriesConfiguration configuration, ILogger<AttributeDataProvider> logger, TripCounterFactory tripCounterFactory)
        {
            _entity = entity;
            _attribute = attribute;
            _configuration = configuration;
            _logger = logger;
            _tripCounterFactory = tripCounterFactory;
            _store = store;
            _validator = validator;
            _aggregator = aggregator;
            _seriesKnower = seriesKnower;
            _filterer = filterer;
            _aggregatedMerger = aggregatedMerger;
            _timeMerger = timeMerger;
        }

        public async Task<List<AggregatedDataRange>> GetData(TimeRange timeRange, int aggregationSeconds)
        {
            var series = await _seriesKnower.GetAttributeSeriesGuid(_entity, _attribute, aggregationSeconds);
            var dataTask = _store.LoadData(series, timeRange);
            var timeRangesTask = GetTimeRanges(aggregationSeconds);

            var data = await dataTask;
            var timeRanges = await timeRangesTask;

            return ToDataRanges(timeRanges, data, timeRange, aggregationSeconds).ToList();
        }

        public async Task<List<TimeRange>> GetTimeRanges(int aggregationSeconds)
        {
            var series = await _seriesKnower.GetAttributeSeriesGuid(_entity, _attribute, aggregationSeconds);
            var cache = await GetTimeRangesCache();
            if (cache.TryGetValue(series, out List<TimeRange> timeRanges))
            {
                return timeRanges;
            }
            return new List<TimeRange>();
        }

        public async Task<List<AggregatedDataRange>> AddData(AggregatedDataRange dataRange)
        {
            using (_tripCounterFactory.Create("AttributeDataProvider.AddData"))
            {
                _validator.ValidateDataRangesIsOfAggregation(dataRange, _configuration.LowestAggregationSeconds, $"Data to be added must be aggregated to the lowest aggregation level which is {_configuration.LowestAggregationSeconds} seconds");

                // Work out quantised range
                var quantisedRange = dataRange.TimeRange.Quantise(_configuration.HighestAggregationSeconds);

                // Fetch lowest aggregation from provider
                var existingRanges = await GetData(quantisedRange, _configuration.LowestAggregationSeconds);

                // Add incoming data to lowest aggregation
                _logger.LogDebug($"Merging {existingRanges.Count} existing ranges with new lowest aggregation range");
                var merged = _aggregatedMerger.MergeRangeWithRanges(existingRanges, dataRange);
                _validator.ValidateAtLeastOneDataRange(merged, "Incoming data merged with existing data didn't produce a single ranges");

                // Recalculate higher aggregations

                _logger.LogDebug($"Aggregating {merged.Sum(x => x.Data.Count / 2)} points to higher bins");
                var aggregations = _aggregator.Aggregate(merged, quantisedRange, _configuration.AggregationsSecondsDescending);
                var affectedAggregatedPoints = aggregations.Select(x => FilterAggregationToNewPoint(x.Value, dataRange.TimeRange)).ToList();
                var aggregationsToSave = await Task.WhenAll(affectedAggregatedPoints.Select(PrepareToSaveAggregation));

                _logger.LogDebug($"Adding aggregated data");
                await _store.SaveData(aggregationsToSave);

                await AddRangesToTimeRangesCache(affectedAggregatedPoints);

                await PersistTimeRangesCache();

                // Obtain affected data
                return affectedAggregatedPoints;
            }
        }

        private async Task<(Guid series, List<double> data)> PrepareToSaveAggregation(AggregatedDataRange dataRange)
        {
            var series = await _seriesKnower.GetAttributeSeriesGuid(_entity, _attribute, dataRange.AggregationSeconds);
            return (series, dataRange.Data);
        }

        private async Task AddRangesToTimeRangesCache(IEnumerable<AggregatedDataRange> dataRanges)
        {
            var seriesGuids = (await Task.WhenAll(dataRanges.Select(dataRange => _seriesKnower.GetAttributeSeriesGuid(_entity, _attribute, dataRange.AggregationSeconds)))).ToList();

            var timeRangesCache = await GetTimeRangesCache();
            lock (_timeRangesCacheLock)
            {
                var i = 0;
                foreach (var dataRange in dataRanges)
                {
                    var series = seriesGuids[i];
                    var timeRanges = timeRangesCache.GetOrAdd(series, new List<TimeRange>());
                    _logger.LogDebug($"Merging {timeRanges.Count} with data range");
                    timeRangesCache[series] = _timeMerger.MergeRangeWithRanges(timeRanges, dataRange.TimeRange).ToList();
                    i++;
                }
            }
        }

        private AggregatedDataRange FilterAggregationToNewPoint(IEnumerable<AggregatedDataRange> dataRanges, TimeRange timeRange)
        {
            var filtered = _filterer.FilterRanges(dataRanges, timeRange);
            _validator.ValidateExactlyOneDataRange(filtered, $"Filtered aggregated data was expected to be exactly one data range but was {filtered.Count()}");
            return filtered.Single();
        }

        private async Task PersistTimeRangesCache()
        {
            var cache = await GetTimeRangesCache();
            var timeRanges = cache.Select(x =>
            {
                var series = x.Key;
                var ranges = x.Value;
                return (series, ranges);
            });
            await _store.SaveTimeRanges(timeRanges);
        }

        private IEnumerable<AggregatedDataRange> ToDataRanges(IEnumerable<TimeRange> timeRanges, List<double> data, TimeRange filterTimeRange, int aggregationSeconds)
        {
            var index = 0;
            foreach (var timeRange in timeRanges)
            {
                if (timeRange.Max <= filterTimeRange.Min || timeRange.Min >= filterTimeRange.Max)
                {
                    continue;
                }

                var thisTimeRange = new TimeRange(timeRange.Min, timeRange.Max);

                var rangeData = new List<double>();

                while (index < data.Count && data[index] <= timeRange.Min)
                {
                    index++;
                }

                while (index < data.Count && data[index] <= timeRange.Max)
                {
                    rangeData.Add(data[index]);
                    rangeData.Add(data[index + 1]);
                    index += 2;
                }

                if (rangeData.Count > 0)
                {
                    yield return new AggregatedDataRange(thisTimeRange, rangeData, aggregationSeconds);
                }
            }
        }

        private async Task<ConcurrentDictionary<Guid, List<TimeRange>>> GetTimeRangesCache()
        {
            if (_timeRangesCache != null)
            {
                return _timeRangesCache;
            }

            try
            {
                await _timeRangesCacheSemaphore.WaitAsync();
                if (_timeRangesCache != null)
                {
                    return _timeRangesCache;
                }

                var guids = await _seriesKnower.GetAttributeSeriesGuids(_entity, _attribute);
                var timeRanges = await _store.LoadTimeRanges(guids.Select(x => x.Value));
                return _timeRangesCache = new ConcurrentDictionary<Guid, List<TimeRange>>(timeRanges);
            }
            finally
            {
                _timeRangesCacheSemaphore.Release();
            }
        }
    }
}
