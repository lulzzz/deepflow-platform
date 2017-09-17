﻿/*
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
        private readonly ConcurrentDictionary<Guid, IEnumerable<RawDataRange>> _seriesRanges = new ConcurrentDictionary<Guid, IEnumerable<RawDataRange>>();

        protected abstract Task<IEnumerable<RawDataRange>> ProduceAttributeRanges(Guid series, IEnumerable<TimeRange> timeRanges);

        public InMemoryDataProvider(IDataMerger merger, IDataFilterer dataFilterer, ITimeFilterer timeFilterer)
        {
            _merger = merger;
            _dataFilterer = dataFilterer;
            _timeFilterer = timeFilterer;
        }

        public async Task<IEnumerable<AggregatedDataRange>> GetAggregatedRanges(Guid series, TimeRange timeRange, int aggregationSeconds)
        {
            if (!_seriesRanges.TryGetValue(series, out IEnumerable<RawDataRange> dataRanges))
            {
                return new List<AggregatedDataRange>();
            }

            var existingRanges = _dataFilterer.FilterDataRanges(dataRanges, timeRange);
            var rangesToProduce = _timeFilterer.SubtractTimeRangesFromRange(timeRange, existingRanges.Select(x => x.TimeRange));
            var producedRanges = await ProduceAttributeRanges(series, rangesToProduce);
            var mergedInRange = _merger.MergeDataRangesWithRanges(existingRanges, producedRanges);

            var merged = _merger.MergeDataRangesWithRanges(dataRanges, producedRanges);
            _seriesRanges.AddOrUpdate(series, mergedInRange, (guid, ranges) => merged);

            return mergedInRange;
        }

        public Task SaveAggregatedRanges(Guid series, IEnumerable<AggregatedDataRange> dataRanges)
        {
            throw new NotImplementedException();
        }

        public Task SaveAggregatedRange(Guid series, AggregatedDataRange dataRange)
        {
            throw new NotImplementedException();
        }

        public Task SaveAggregatedRange(Guid series, RawDataRange incomingDataRange)
        {
            if (!_seriesRanges.TryGetValue(series, out IEnumerable<RawDataRange> existingRanges))
            {
                existingRanges = new List<RawDataRange>();
            }

            var merged = _merger.MergeDataRangeWithRanges(existingRanges, incomingDataRange);
            _seriesRanges.AddOrUpdate(series, merged, (guid, ranges) => merged);

            return Task.FromResult(0);
        }

        public Task<IEnumerable<TimeRange>> GetSavedAggregatedTimeRanges(Guid series)
        {
            throw new NotImplementedException();
        }
    }
}
*/
