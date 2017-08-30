using System.Collections.Generic;
using System.Linq;
using Deepflow.Platform.Abstractions.Series;

namespace Deepflow.Platform.Series
{
    public class TimeFilterer : ITimeFilterer
    {
        public IEnumerable<TimeRange> SubtractTimeRangesFromRange(TimeRange timeRange, IEnumerable<TimeRange> subtractRanges)
        {
            return SubtractTimeRangesFromRanges(new[] { timeRange }, subtractRanges);
        }

        public IEnumerable<TimeRange> SubtractTimeRangesFromRanges(IEnumerable<TimeRange> timeRanges, IEnumerable<TimeRange> subtractRanges) {
            var remainingRanges = timeRanges;
            foreach (var subtractRange in subtractRanges)
            {
                remainingRanges = SubtractTimeRangeFromRanges(subtractRange, remainingRanges);
            }
            return remainingRanges;
        }

        public IEnumerable<TimeRange> SubtractTimeRangeFromRanges(TimeRange subtractRange, IEnumerable<TimeRange> ranges) {
            IEnumerable<TimeRange> remainingRanges = new List<TimeRange>();
            foreach (var timeRange in ranges)
            {
                remainingRanges = remainingRanges.Concat(SubtractTimeRangeFromRange(subtractRange, timeRange));
            }
            return remainingRanges;
        }

        private IEnumerable<TimeRange> SubtractTimeRangeFromRange(TimeRange subtractRange, TimeRange range)
        {
            if (subtractRange.MaxSeconds <= range.MinSeconds || subtractRange.MinSeconds >= range.MaxSeconds) {
                return new [] { range };
            }

            if (subtractRange.MinSeconds <= range.MinSeconds && subtractRange.MaxSeconds >= range.MaxSeconds) {
                return new TimeRange[] { };
            }

            if (subtractRange.MaxSeconds > range.MinSeconds && subtractRange.MinSeconds <= range.MinSeconds && subtractRange.MaxSeconds < range.MaxSeconds) {
                return new[] { new TimeRange(subtractRange.MaxSeconds, range.MaxSeconds) };
            }

            if (subtractRange.MaxSeconds > range.MinSeconds && subtractRange.MaxSeconds < range.MaxSeconds && subtractRange.MinSeconds > range.MinSeconds)
            {
                return new[]
                {
                    new TimeRange(range.MinSeconds, subtractRange.MinSeconds),
                    new TimeRange(subtractRange.MaxSeconds, range.MaxSeconds)
                };
            }

            if (subtractRange.MaxSeconds >= range.MaxSeconds && subtractRange.MinSeconds > range.MinSeconds) {
                return new[] { new TimeRange(range.MinSeconds, subtractRange.MinSeconds) };
            }

            return null;
        }
    }
}
