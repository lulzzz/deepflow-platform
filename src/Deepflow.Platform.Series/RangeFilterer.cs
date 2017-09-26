﻿using System;
using System.Collections.Generic;
using System.Linq;
using Deepflow.Platform.Abstractions.Series;
using Deepflow.Platform.Abstractions.Series.Extensions;

namespace Deepflow.Platform.Series
{
    public class RangeFilterer<TRange> : IRangeFilterer<TRange>
    {
        private readonly IRangeCreator<TRange> _creator;
        private readonly IRangeFilteringPolicy<TRange> _policy;
        private readonly IRangeAccessor<TRange> _accessor;

        public RangeFilterer(IRangeCreator<TRange> creator, IRangeFilteringPolicy<TRange> policy, IRangeAccessor<TRange> accessor)
        {
            _creator = creator;
            _policy = policy;
            _accessor = accessor;
        }

        public IEnumerable<TRange> FilterDataRanges(IEnumerable<TRange> ranges, TimeRange timeRange)
        {
            return ranges?.Where(dataRange => RangeTouchesRange(_accessor.GetTimeRange(dataRange), timeRange)).Select(range => FilterRange(range, timeRange, _policy.FilterMode));
        }

        public TRange FilterDataRange(TRange range, TimeRange timeRange)
        {
            return FilterDataRanges(new List<TRange> { range }, timeRange).SingleOrDefault();
        }

        public IEnumerable<TRange> SubtractTimeRangeFromRanges(IEnumerable<TRange> ranges, TimeRange subtractRange)
        {
            if (subtractRange == null)
            {
                return ranges;
            }

            if (subtractRange.IsZeroLength())
            {
                return ranges;
            }

            IEnumerable<TRange> remainingRanges = new List<TRange>();
            foreach (var range in ranges)
            {
                remainingRanges = remainingRanges.Concat(SubtractTimeRangeFromRange(subtractRange, range));
            }
            return remainingRanges;
        }

        private TRange FilterRange(TRange range, TimeRange timeRange, FilterMode filterMode)
        {
            return _creator.Create(_accessor.GetTimeRange(range).Insersect(timeRange), FilterData(_accessor.GetData(range), timeRange, filterMode), range);
        }

        private List<double> FilterData(List<double> data, TimeRange timeRange, FilterMode filterMode)
        {
            if (data == null)
            {
                return null;
            }

            var startIndex = BinarySearcher.GetFirstIndexPastTimestampBinary(data, timeRange.Min);
            var endIndex = BinarySearcher.GetFirstIndexPastTimestampBinary(data, timeRange.Max);

            if (endIndex == null)
            {
                endIndex = data.Count;
            }
            else if (timeRange.Insersects((long)data[endIndex.Value]))
            {
                endIndex += 2;
            }

            if (startIndex == null)
            {
                startIndex = 0;
            }
            else if (filterMode == FilterMode.MaxInclusive && timeRange.EqualsMin(data[startIndex.Value]))
            {
                startIndex += 2;
            }

            return data.Skip(startIndex.Value).Take(endIndex.Value - startIndex.Value).ToList();
        }

        private bool RangeTouchesRange(TimeRange timeRangeA, TimeRange timeRangeB)
        {
            if (timeRangeA == null)
            {
                return false;
            }

            if (timeRangeB == null)
            {
                return false;
            }

            return RangeIntersectsRange(timeRangeA, timeRangeB) || timeRangeA.Max == timeRangeB.Min || timeRangeA.Min == timeRangeB.Max;
        }

        private bool RangeIntersectsRange(TimeRange timeRangeA, TimeRange timeRangeB)
        {
            if (timeRangeA == null || timeRangeB == null)
            {
                return false;
            }

            if (timeRangeA.Max <= timeRangeB.Min)
            {
                return false;
            }

            if (timeRangeB.Max <= timeRangeA.Min)
            {
                return false;
            }

            if (timeRangeA.Min <= timeRangeB.Min && timeRangeA.Max >= timeRangeB.Max)
            {
                return true;
            }

            return true;
        }

        private IEnumerable<TRange> SubtractTimeRangeFromRange(TimeRange subtractRange, TRange range)
        {
            var timeRange = _accessor.GetTimeRange(range);
            var data = _accessor.GetData(range);

            if (subtractRange.Max <= timeRange.Min || subtractRange.Min >= timeRange.Max)
            {
                return new[] { range };
            }

            if (subtractRange.Min <= timeRange.Min && subtractRange.Max >= timeRange.Max)
            {
                return new TRange[] { };
            }

            if (subtractRange.Max > timeRange.Min && subtractRange.Min <= timeRange.Min && subtractRange.Max < timeRange.Max)
            {
                var newTimeRange = new TimeRange(subtractRange.Max, timeRange.Max);
                return new[] { _creator.Create(newTimeRange, FilterData(data, newTimeRange, _policy.FilterMode), range) };
            }

            if (subtractRange.Max > timeRange.Min && subtractRange.Max < timeRange.Max && subtractRange.Min > timeRange.Min)
            {
                var timeRangeOne = new TimeRange(timeRange.Min, subtractRange.Min);
                var timeRangeTwo = new TimeRange(subtractRange.Max, timeRange.Max);

                return new[]
                {
                    _creator.Create(timeRangeOne, FilterData(data, timeRangeOne, _policy.FilterMode), range),
                    _creator.Create(timeRangeTwo, FilterData(data, timeRangeTwo, _policy.FilterMode), range)
                };
            }

            if (subtractRange.Max >= timeRange.Max && subtractRange.Min > timeRange.Min)
            {
                var newTimeRange = new TimeRange(timeRange.Min, subtractRange.Min);
                return new[] { _creator.Create(newTimeRange, FilterData(data, newTimeRange, _policy.FilterMode), range) };
            }

            return null;
        }

        
    }
}
