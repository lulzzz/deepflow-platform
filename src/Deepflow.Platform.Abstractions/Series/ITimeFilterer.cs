using System.Collections.Generic;

namespace Deepflow.Platform.Abstractions.Series
{
    public interface ITimeFilterer
    {
        IEnumerable<TimeRange> SubtractTimeRangesFromRange(TimeRange timeRange, IEnumerable<TimeRange> subtractRanges);
    }
}