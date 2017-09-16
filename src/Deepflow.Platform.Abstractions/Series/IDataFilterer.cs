using System.Collections.Generic;

namespace Deepflow.Platform.Abstractions.Series
{
    public interface IDataFilterer
    {
        IEnumerable<DataRange> FilterDataRanges(IEnumerable<DataRange> ranges, TimeRange timeRange);
        IEnumerable<DataRange> FilterDataRangesEndTimeInclusive(IEnumerable<DataRange> ranges, TimeRange timeRange);
        DataRange FilterDataRange(DataRange range, TimeRange timeRange);
        DataRange FilterDataRangeEndTimeInclusive(DataRange range, TimeRange timeRange);
        IEnumerable<DataRange> SubtractTimeRangeFromRanges(IEnumerable<DataRange> ranges, TimeRange subtractRange);
    }
}