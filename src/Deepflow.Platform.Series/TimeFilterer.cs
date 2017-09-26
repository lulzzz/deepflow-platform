/*
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

        /*public IEnumerable<TimeRange> FilterTimeRangesToRange(IEnumerable<TimeRange> timeRanges, TimeRange filterRange)
        {
            throw new System.NotImplementedException();
            asdas
        }#1#

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
            if (subtractRange.Max <= range.Min || subtractRange.Min >= range.Max) {
                return new [] { range };
            }

            if (subtractRange.Min <= range.Min && subtractRange.Max >= range.Max) {
                return new TimeRange[] { };
            }

            if (subtractRange.Max > range.Min && subtractRange.Min <= range.Min && subtractRange.Max < range.Max) {
                return new[] { new TimeRange(subtractRange.Max, range.Max) };
            }

            if (subtractRange.Max > range.Min && subtractRange.Max < range.Max && subtractRange.Min > range.Min)
            {
                return new[]
                {
                    new TimeRange(range.Min, subtractRange.Min),
                    new TimeRange(subtractRange.Max, range.Max)
                };
            }

            if (subtractRange.Max >= range.Max && subtractRange.Min > range.Min) {
                return new[] { new TimeRange(range.Min, subtractRange.Min) };
            }

            return null;
        }
    }
}
*/
