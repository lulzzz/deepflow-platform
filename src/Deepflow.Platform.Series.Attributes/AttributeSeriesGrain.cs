using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Deepflow.Platform.Abstractions.Series;
using Microsoft.Extensions.Logging;
using Orleans;

namespace Deepflow.Platform.Series.Attributes
{
    public class AttributeSeriesGrain : Grain, IAttributeSeriesGrain
    {
        private Guid _entity;
        private Guid _attribute;
        private Dictionary<int, Guid> _seriesGuids;
        private readonly ISeriesKnower _seriesKnower;
        private readonly IDataProvider _dataProvider;
        private readonly IDataAggregator _aggregator;
        private readonly IDataMerger _merger;
        private readonly IDataFilterer _filterer;
        private readonly IDataValidator _validator;
        private readonly ISeriesConfiguration _seriesConfiguration;
        private readonly ILogger<AttributeSeriesGrain> _logger;
        private readonly ObserverSubscriptionManager<ISeriesObserver> _subscriptions = new ObserverSubscriptionManager<ISeriesObserver>();

        public AttributeSeriesGrain(ISeriesKnower seriesKnower, IDataProvider dataProvider, IDataAggregator aggregator, IDataMerger merger, IDataFilterer filterer, IDataValidator validator, ISeriesConfiguration seriesConfiguration, ILogger<AttributeSeriesGrain> logger)
        {
            _seriesKnower = seriesKnower;
            _dataProvider = dataProvider;
            _aggregator = aggregator;
            _merger = merger;
            _filterer = filterer;
            _validator = validator;
            _seriesConfiguration = seriesConfiguration;
            _logger = logger;
        }

        public override async Task OnActivateAsync()
        {
            var key = this.GetPrimaryKeyString();
            var parts = key.Split(':');
            _entity = Guid.Parse(parts[0]);
            _attribute = Guid.Parse(parts[1]);
            _seriesGuids = await _seriesKnower.GetAttributeSeriesGuids(_entity, _attribute);
            await base.OnActivateAsync();
        }

        public async Task<IEnumerable<DataRange>> GetData(TimeRange timeRange, int aggregationSeconds)
        {
            if (!_seriesGuids.TryGetValue(aggregationSeconds, out Guid seriesGuid))
            {
                throw new Exception($"Cannot find series GUID for attribute {_entity}:{_attribute}:{aggregationSeconds}");
            }
            var data = await _dataProvider.GetAttributeRanges(seriesGuid, timeRange);
            return data.ToList();
        }

        public async Task AddAggregatedData(IEnumerable<DataRange> dataRanges, int aggregationSeconds)
        {
            if (!_validator.AreValidDataRanges(dataRanges))
            {
                _logger.LogWarning($"Invalid range could not be added");
                return;
            }

            if (aggregationSeconds != _seriesConfiguration.LowestAggregationSeconds)
            {
                throw new Exception($"Data to be added must be aggregated to the lowest aggregation level which is '{_seriesConfiguration.LowestAggregationSeconds}'");
            }

            _logger.LogWarning($"Adding aggregated data");
            var tasks = dataRanges.Select(AddAggregatedData);
            await Task.WhenAll(tasks.ToArray());
        }

        public Task NotifyRawData(IEnumerable<DataRange> dataRanges)
        {
            _subscriptions.Notify(observer => observer.ReceiveRawData(_entity, _attribute, dataRanges));
            return Task.FromResult(0);
        }

        private async Task AddAggregatedData(DataRange dataRange)
        {
            if (!_validator.IsValidDataRange(dataRange))
            {
                return;
            }

            // Validate incoming data is at lowest aggregation
            _validator.ValidateAggregation(dataRange, _seriesConfiguration.LowestAggregationSeconds);

            // Work out quantised range
            var quantisedRange = dataRange.TimeRange.Quantise(_seriesConfiguration.HighestAggregationSeconds);

            // Fetch lowest aggregation from provider
            var series = await _seriesKnower.GetAttributeSeriesGuid(_entity, _attribute, _seriesConfiguration.LowestAggregationSeconds);
            var lowestAggregationExistingData = CleanDataRanges(await _dataProvider.GetAttributeRanges(series, quantisedRange));

            // Add incoming data to lowest aggregation
            var merged = _merger.MergeDataRangeWithRanges(lowestAggregationExistingData, dataRange);
            _validator.ValidateAtLeastOneDataRange(merged, "Incoming data merged with existing data didn't produce a single ranges");
            _validator.ValidateSingleOrLessDataRange(merged, "Incomning data merged with existing data produced multiple ranges");

            // Recalculate higher aggregations
            IEnumerable<AggregatedDataRange> aggregations = _aggregator.Aggregate(merged.Single(), _seriesConfiguration.AggregationsSecondsDescending);

            _logger.LogWarning($"Adding aggregated data");
            var tasks = aggregations.Select(SaveAggregatedRange);
            await Task.WhenAll(tasks.ToArray());
            _logger.LogWarning($"Added aggregated data, notifying subscribers");

            // Obtain affected data
            var affectedAggregations = aggregations.Select(aggregatedDataRange => FilterAggregatedRange(aggregatedDataRange, quantisedRange));

            // Notify subscribers with affected data
            _subscriptions.Notify(observer => observer.ReceiveAggregatedData(_entity, _attribute, affectedAggregations));
        }

        private async Task SaveAggregatedRange(AggregatedDataRange aggregatedDataRange)
        {
            var series = await _seriesKnower.GetAttributeSeriesGuid(_entity, _attribute, aggregatedDataRange.AggregationSeconds);
            await _dataProvider.SaveAttributeRange(series, aggregatedDataRange.DataRange);
        }

        private AggregatedDataRange FilterAggregatedRange(AggregatedDataRange aggregatedDataRange, TimeRange timeRange)
        {
            var filtered = _filterer.FilterDataRangeEndTimeInclusive(aggregatedDataRange.DataRange, timeRange);
            return new AggregatedDataRange(filtered, aggregatedDataRange.AggregationSeconds);
        }

        public Task Subscribe(ISeriesObserver observer)
        {
            _subscriptions.Subscribe(observer);
            return Task.FromResult(0);
        }

        public Task Unsubscribe(ISeriesObserver observer)
        {
            _subscriptions.Unsubscribe(observer);
            return Task.FromResult(0);
        }

        private IEnumerable<DataRange> CleanDataRanges(IEnumerable<DataRange> dataRanges)
        {
            foreach (var dataRange in dataRanges)
            {
                if (dataRange != null && !dataRange.TimeRange.IsZeroLength() && dataRange.Data.Any())
                {
                    yield return dataRange;
                }
            }
        }
    }
}
