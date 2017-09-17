using System.Collections.Generic;
using System.Linq;
using Deepflow.Platform.Abstractions.Series;

namespace Deepflow.Platform.Series
{
    public class DataFilterer<TRange> : IDataFilterer<TRange> where TRange : IDataRange
    {
        private readonly IDataRangeCreator<TRange> _creator;

        public DataFilterer(IDataRangeCreator<TRange> creator)
        {
            _creator = creator;
        }

        public IEnumerable<TRange> FilterDataRanges(IEnumerable<TRange> ranges, TimeRange timeRange)
        {
            return ranges?.Where(dataRange => RangeTouchesRange(dataRange.TimeRange, timeRange)).Select(range => FilterRange(range, timeRange, FilterMode.MinAndMaxInclusive)).Where(x => x.Data.Count > 0);
        }

        public IEnumerable<TRange> FilterDataRangesEndTimeInclusive(IEnumerable<TRange> ranges, TimeRange timeRange)
        {
            return ranges?.Where(dataRange => RangeTouchesRange(dataRange.TimeRange, timeRange)).Select(range => FilterRange(range, timeRange, FilterMode.MaxInclusive)).Where(x => x.Data.Count > 0);
        }

        public TRange FilterDataRange(TRange range, TimeRange timeRange)
        {
            return FilterDataRanges(new List<TRange> { range }, timeRange).SingleOrDefault();
        }

        public TRange FilterDataRangeEndTimeInclusive(TRange range, TimeRange timeRange)
        {
            return FilterDataRangesEndTimeInclusive(new List<TRange> { range }, timeRange).SingleOrDefault();
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
            return _creator.Create(range.TimeRange.Insersect(timeRange), FilterData(range.Data, timeRange, filterMode), range);
        }

        private List<double> FilterData(List<double> data, TimeRange timeRange, FilterMode filterMode)
        {
            var startIndex = BinarySearcher.GetFirstIndexPastTimestampBinary(data, timeRange.MinSeconds);
            var endIndex = BinarySearcher.GetFirstIndexPastTimestampBinary(data, timeRange.MaxSeconds);

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

            return RangeIntersectsRange(timeRangeA, timeRangeB) || timeRangeA.MaxSeconds == timeRangeB.MinSeconds || timeRangeA.MinSeconds == timeRangeB.MaxSeconds;
        }

        private bool RangeIntersectsRange(TimeRange timeRangeA, TimeRange timeRangeB)
        {
            if (timeRangeA == null || timeRangeB == null)
            {
                return false;
            }

            if (timeRangeA.MaxSeconds <= timeRangeB.MinSeconds)
            {
                return false;
            }

            if (timeRangeB.MaxSeconds <= timeRangeA.MinSeconds)
            {
                return false;
            }

            if (timeRangeA.MinSeconds <= timeRangeB.MinSeconds && timeRangeA.MaxSeconds >= timeRangeB.MaxSeconds)
            {
                return true;
            }

            return true;
        }

        private IEnumerable<TRange> SubtractTimeRangeFromRange(TimeRange subtractRange, TRange range)
        {
            if (subtractRange.MaxSeconds <= range.TimeRange.MinSeconds || subtractRange.MinSeconds >= range.TimeRange.MaxSeconds)
            {
                return new[] { range };
            }

            if (subtractRange.MinSeconds <= range.TimeRange.MinSeconds && subtractRange.MaxSeconds >= range.TimeRange.MaxSeconds)
            {
                return new TRange[] { };
            }

            if (subtractRange.MaxSeconds > range.TimeRange.MinSeconds && subtractRange.MinSeconds <= range.TimeRange.MinSeconds && subtractRange.MaxSeconds < range.TimeRange.MaxSeconds)
            {
                var timeRange = new TimeRange(subtractRange.MaxSeconds, range.TimeRange.MaxSeconds);
                return new[] { _creator.Create(timeRange, FilterData(range.Data, timeRange, FilterMode.MinAndMaxInclusive), range) };
            }

            if (subtractRange.MaxSeconds > range.TimeRange.MinSeconds && subtractRange.MaxSeconds < range.TimeRange.MaxSeconds && subtractRange.MinSeconds > range.TimeRange.MinSeconds)
            {
                var timeRangeOne = new TimeRange(range.TimeRange.MinSeconds, subtractRange.MinSeconds);
                var timeRangeTwo = new TimeRange(subtractRange.MaxSeconds, range.TimeRange.MaxSeconds);

                return new[]
                {
                    _creator.Create(timeRangeOne, FilterData(range.Data, timeRangeOne, FilterMode.MinAndMaxInclusive), range),
                    _creator.Create(timeRangeTwo, FilterData(range.Data, timeRangeTwo, FilterMode.MinAndMaxInclusive), range)
                };
            }

            if (subtractRange.MaxSeconds >= range.TimeRange.MaxSeconds && subtractRange.MinSeconds > range.TimeRange.MinSeconds)
            {
                var timeRange = new TimeRange(range.TimeRange.MinSeconds, subtractRange.MinSeconds);
                return new[] { _creator.Create(timeRange, FilterData(range.Data, timeRange, FilterMode.MinAndMaxInclusive), range) };
            }

            return null;
        }

        private enum FilterMode
        {
            MaxInclusive,
            MinAndMaxInclusive
        }
    }
}
