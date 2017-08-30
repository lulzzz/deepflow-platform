/*using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Deepflow.Platform.Abstractions.AttributeSeries;
using Orleans;

namespace Deepflow.Platform.AttributeSeries
{
    public class SeriesGrain : Grain, ISeriesGrain
    {
        private Guid _entity;
        private Guid _attribute;
        private readonly SeriesSettings _config;
        private readonly IDataAggregator _aggregator;
        private readonly IDataFilterer _dataFilterer;
        private readonly IDataProvider _provider;
        private readonly ITimeFilterer _timeFilterer;
        private readonly ISeriesKnower _seriesKnower;
        private readonly IDataJoiner _dataJoiner;
        private IDictionary<int, IEnumerable<DataRange>> _aggregations = new Dictionary<int, IEnumerable<DataRange>>();
        private readonly ObserverSubscriptionManager<ISeriesObserver> _subscriptions = new ObserverSubscriptionManager<ISeriesObserver>();

        public SeriesGrain(SeriesSettings config, IDataAggregator aggregator, IDataFilterer dataFilterer, IDataJoiner dataJoiner, IDataProvider provider, ITimeFilterer timeFilterer, ISeriesKnower seriesKnower)
        {
            _config = config;
            _aggregator = aggregator;
            _dataFilterer = dataFilterer;
            _dataJoiner = dataJoiner;
            _provider = provider;
            _timeFilterer = timeFilterer;
            _seriesKnower = seriesKnower;
        }

        public override Task OnActivateAsync()
        {
            SetKey(this.GetPrimaryKeyString());
            return base.OnActivateAsync();
        }
        
        public Task AddData(IEnumerable<DataRange> dataRanges)
        {
            _aggregations = _aggregator.AddToAggregations(_aggregations, dataRanges, _config.Aggregations);
            return Task.FromResult(0);
        }

        public async Task<IEnumerable<DataRange>> GetAggregatedData(TimeRange timeRange, int aggregationSeconds)
        {
            IEnumerable<DataRange> dataRanges;
            if (!_aggregations.TryGetValue(aggregationSeconds, out dataRanges))
            {
                dataRanges = new List<DataRange>();
                _aggregations.Add(aggregationSeconds, dataRanges);
                //return dataRanges;
            }

            var timeRanges = _timeFilterer.SubtractTimeRangesFromRange(timeRange, dataRanges.Select(x => x.TimeRange));
            var seriesGuid = await _seriesKnower.GetSeriesGuid(_entity, _attribute, aggregationSeconds);
            var loadedRanges = await _provider.GetAttributeRanges(seriesGuid, timeRanges);
            dataRanges = _dataJoiner.JoinDataRangesToDataRanges(dataRanges, loadedRanges);

            return _dataFilterer.FilterDataRanges(dataRanges, timeRange);
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

        private void SetKey(string key)
        {
            var parts = key.Split(':');
            if (!Guid.TryParse(parts[0], out _entity))
            {
                throw new Exception("Could not parse entity GUID");
            }
            if (!Guid.TryParse(parts[1], out _attribute))
            {
                throw new Exception("Could not parse attribute GUID");
            }
        }
    }
}*/