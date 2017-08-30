using System.Collections.Generic;
using System.Linq;
using Deepflow.Platform.Abstractions.Series;

namespace Deepflow.Platform.Series
{
    public class DataFilterer : IDataFilterer
    {
        public IEnumerable<DataRange> FilterDataRanges(IEnumerable<DataRange> ranges, TimeRange timeRange)
        {
            return ranges?.Where(dataRange => RangeTouchesRange(dataRange.TimeRange, timeRange)).Select(range => FilterRange(range, timeRange)).Where(x => x.Data.Count > 0);
        }

        public DataRange FilterDataRange(DataRange range, TimeRange timeRange)
        {
            return FilterDataRanges(new List<DataRange> { range }, timeRange).SingleOrDefault();
        }

        public IEnumerable<DataRange> SubtractTimeRangeFromRanges(IEnumerable<DataRange> ranges, TimeRange subtractRange)
        {
            if (subtractRange == null)
            {
                return ranges;
            }

            if (subtractRange.IsZeroLength())
            {
                return ranges;
            }

            IEnumerable<DataRange> remainingRanges = new List<DataRange>();
            foreach (var timeRange in ranges)
            {
                remainingRanges = remainingRanges.Concat(SubtractTimeRangeFromRange(subtractRange, timeRange));
            }
            return remainingRanges;
        }

        private DataRange FilterRange(DataRange range, TimeRange timeRange)
        {
            return new DataRange(range.TimeRange.Insersect(timeRange), FilterData(range.Data, timeRange));
        }

        public List<double> FilterData(List<double> data, TimeRange timeRange)
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
            if (timeRangeA == null || timeRangeB == null) {
                return false;
            }

            if (timeRangeA.MaxSeconds <= timeRangeB.MinSeconds) {
                return false;
            }

            if (timeRangeB.MaxSeconds <= timeRangeA.MinSeconds) {
                return false;
            }

            if (timeRangeA.MinSeconds <= timeRangeB.MinSeconds && timeRangeA.MaxSeconds >= timeRangeB.MaxSeconds) {
                return true;
            }
    
            return true;
        }

        private IEnumerable<DataRange> SubtractTimeRangeFromRange(TimeRange subtractRange, DataRange range)
        {
            if (subtractRange.MaxSeconds <= range.TimeRange.MinSeconds || subtractRange.MinSeconds >= range.TimeRange.MaxSeconds)
            {
                return new[] { range };
            }

            if (subtractRange.MinSeconds <= range.TimeRange.MinSeconds && subtractRange.MaxSeconds >= range.TimeRange.MaxSeconds)
            {
                return new DataRange[] { };
            }

            if (subtractRange.MaxSeconds > range.TimeRange.MinSeconds && subtractRange.MinSeconds <= range.TimeRange.MinSeconds && subtractRange.MaxSeconds < range.TimeRange.MaxSeconds)
            {
                var timeRange = new TimeRange(subtractRange.MaxSeconds, range.TimeRange.MaxSeconds);
                return new [] { new DataRange(timeRange, FilterData(range.Data, timeRange)) };
            }

            if (subtractRange.MaxSeconds > range.TimeRange.MinSeconds && subtractRange.MaxSeconds < range.TimeRange.MaxSeconds && subtractRange.MinSeconds > range.TimeRange.MinSeconds)
            {
                var timeRangeOne = new TimeRange(range.TimeRange.MinSeconds, subtractRange.MinSeconds);
                var timeRangeTwo = new TimeRange(subtractRange.MaxSeconds, range.TimeRange.MaxSeconds);

                return new []
                {
                    new DataRange(timeRangeOne, FilterData(range.Data, timeRangeOne)),
                    new DataRange(timeRangeTwo, FilterData(range.Data, timeRangeTwo))
                };
            }

            if (subtractRange.MaxSeconds >= range.TimeRange.MaxSeconds && subtractRange.MinSeconds > range.TimeRange.MinSeconds)
            {
                var timeRange = new TimeRange(range.TimeRange.MinSeconds, subtractRange.MinSeconds);
                return new[] { new DataRange(timeRange, FilterData(range.Data, timeRange)) };
            }

            return null;
        }
    }
}
