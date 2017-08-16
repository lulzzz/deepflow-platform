using System;
using System.Collections.Generic;
using System.Linq;
using Deepflow.Platform.Abstractions.Series;
using Deepflow.Platform.Core.Tools;

namespace Deepflow.Platform.Series
{
    public class DataAggregator : IDataAggregator
    {
        private readonly IDataMerger _merger;
        private readonly IDataFilterer _filterer;

        public DataAggregator(IDataMerger merger, IDataFilterer filterer)
        {
            _merger = merger;
            _filterer = filterer;
        }

        public IDictionary<int, IEnumerable<DataRange>> AddToAggregations(IDictionary<int, IEnumerable<DataRange>> aggregations, IEnumerable<DataRange> rawDataRanges, HashSet<int> levels)
        {
            foreach (var rawDataRange in rawDataRanges)
            {
                aggregations = AddRawRange(aggregations, rawDataRange, levels);
            }

            return aggregations;
        }

        public IDictionary<int, IEnumerable<DataRange>> AddRawRange(IDictionary<int, IEnumerable<DataRange>> aggregations, DataRange rawDataRange, HashSet<int> levels)
        {
            if (!aggregations.TryGetValue(0, out IEnumerable<DataRange> rawRanges))
            {
                rawRanges = new List<DataRange>();
                aggregations.Add(0, rawRanges);
            }
            
            var joinedRawRanges = _merger.MergeDataRangeWithRanges(rawRanges, rawDataRange);
            aggregations[0] = joinedRawRanges;
            
            foreach (var level in levels)
            {
                var lowerLevel = levels.Contains(level / 2) ? level / 2 : level / 3;

                if (!aggregations.TryGetValue(lowerLevel, out IEnumerable<DataRange> aggregatedRanges))
                {
                    aggregatedRanges = new List<DataRange>();
                    aggregations.Add(level, aggregatedRanges);
                }

                var newAggregatedRanges = AddToAggregation(aggregatedRanges, level, aggregatedRanges, rawDataRange.TimeRange);
                aggregations[level] = newAggregatedRanges;
            }

            return aggregations;
        }

        private IEnumerable<DataRange> AddToAggregation(IEnumerable<DataRange> aggregatedRanges, int aggregationSeconds, IEnumerable<DataRange> lowerAggregatedRanges, TimeRange timeRange)
        {
            var lowerRanges = _filterer.FilterDataRanges(lowerAggregatedRanges, timeRange);
            if (!lowerRanges.Any())
            {
                return aggregatedRanges;
            }

            var minTime = (int) Math.Ceiling((double)timeRange.MinSeconds / aggregationSeconds) * aggregationSeconds;
            var maxTime = (int) Math.Ceiling((double)timeRange.MaxSeconds / aggregationSeconds) * aggregationSeconds;
            var endTimes = Enumerable.Range(minTime, maxTime - minTime);
            
            var dataEnumerator = lowerRanges.GetData();

            var aggregated = new List<DataRange>();

            var dataRange = new DataRange(timeRange);
            foreach (var endTime in endTimes)
            {
                var startTime = endTime - aggregationSeconds;
                var data = dataEnumerator.SkipWhile(x => x.Time < startTime).TakeWhile(x => x.Time <= endTime);

                var sum = 0.0;
                var count = 0;
                foreach (var datum in data)
                {
                    sum += datum.Time;
                    count++;
                }

                if (count > 0)
                {
                    dataRange.Data.Add(endTime);
                    dataRange.Data.Add(sum / count);
                }
                else
                {
                    if (dataRange.Data.Count > 0)
                    {
                        aggregated.Add(dataRange);
                    }
                    dataRange = new DataRange(timeRange);
                }
            }

            if (dataRange.Data.Count > 0)
            {
                aggregated.Add(dataRange);
            }

            return _merger.MergeDataRangesWithRanges(aggregatedRanges, aggregated);
        }
    }
}
