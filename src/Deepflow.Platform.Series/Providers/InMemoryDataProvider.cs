using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Deepflow.Platform.Abstractions.Series;

namespace Deepflow.Platform.Series.Providers
{
    public abstract class InMemoryDataProvider : IDataProvider
    {
        private readonly IDataMerger _merger;
        private readonly IDataFilterer _dataFilterer;
        private readonly ITimeFilterer _timeFilterer;
        private readonly ConcurrentDictionary<Guid, IEnumerable<DataRange>> _seriesRanges = new ConcurrentDictionary<Guid, IEnumerable<DataRange>>();

        protected abstract Task<IEnumerable<DataRange>> ProduceAttributeRanges(Guid series, IEnumerable<TimeRange> timeRanges);

        public InMemoryDataProvider(IDataMerger merger, IDataFilterer dataFilterer, ITimeFilterer timeFilterer)
        {
            _merger = merger;
            _dataFilterer = dataFilterer;
            _timeFilterer = timeFilterer;
        }

        public async Task<IEnumerable<DataRange>> GetAttributeRanges(Guid series, IEnumerable<TimeRange> timeRanges)
        {
            var ranges = await Task.WhenAll(timeRanges.Select(timeRange => GetAttributeRanges(series, timeRange)));
            return ranges.SelectMany(x => x);
        }

        public async Task<IEnumerable<DataRange>> GetAttributeRanges(Guid series, TimeRange timeRange)
        {
            if (!_seriesRanges.TryGetValue(series, out IEnumerable<DataRange> dataRanges))
            {
                return new List<DataRange>();
            }

            var existingRanges = _dataFilterer.FilterDataRanges(dataRanges, timeRange);
            var rangesToProduce = _timeFilterer.SubtractTimeRangesFromRange(timeRange, existingRanges.Select(x => x.TimeRange));
            var producedRanges = await ProduceAttributeRanges(series, rangesToProduce);
            var mergedInRange = _merger.MergeDataRangesWithRanges(existingRanges, producedRanges);

            var merged = _merger.MergeDataRangesWithRanges(dataRanges, producedRanges);
            _seriesRanges.AddOrUpdate(series, mergedInRange, (guid, ranges) => merged);

            return mergedInRange;
        }

        public Task SaveAttributeRange(Guid series, DataRange incomingDataRange)
        {
            if (!_seriesRanges.TryGetValue(series, out IEnumerable<DataRange> existingRanges))
            {
                existingRanges = new List<DataRange>();
            }

            var merged = _merger.MergeDataRangeWithRanges(existingRanges, incomingDataRange);
            _seriesRanges.AddOrUpdate(series, merged, (guid, ranges) => merged);

            return Task.FromResult(0);
        }
    }
}
