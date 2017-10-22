using System.Collections.Generic;

namespace Deepflow.Platform.Abstractions.Series
{
    public interface IRangeFilterer<TRange>
    {
        IEnumerable<TRange> FilterRanges(IEnumerable<TRange> ranges, TimeRange timeRange);
        TRange FilterDataRange(TRange range, TimeRange timeRange);
        IEnumerable<TRange> SubtractTimeRangeFromRanges(IEnumerable<TRange> ranges, TimeRange subtractRange);
        IEnumerable<TRange> SubtractTimeRangesFromRange(TRange range, IEnumerable<TimeRange> subtractRanges);
    }
}