using System.Collections.Generic;

namespace Deepflow.Platform.Abstractions.Series
{
    public interface IDataFilterer<TRange> where TRange : IDataRange
    {
        IEnumerable<TRange> FilterDataRanges(IEnumerable<TRange> ranges, TimeRange timeRange);
        IEnumerable<TRange> FilterDataRangesEndTimeInclusive(IEnumerable<TRange> ranges, TimeRange timeRange);
        TRange FilterDataRange(TRange range, TimeRange timeRange);
        TRange FilterDataRangeEndTimeInclusive(TRange range, TimeRange timeRange);
        IEnumerable<TRange> SubtractTimeRangeFromRanges(IEnumerable<TRange> ranges, TimeRange subtractRange);
    }
}