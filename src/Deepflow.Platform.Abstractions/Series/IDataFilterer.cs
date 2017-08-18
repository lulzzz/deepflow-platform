using System.Collections.Generic;

namespace Deepflow.Platform.Abstractions.Series
{
    public interface IDataFilterer
    {
        IEnumerable<DataRange> FilterDataRanges(IEnumerable<DataRange> ranges, TimeRange timeRange);

        IEnumerable<DataRange> SubtractTimeRangeFromRanges(IEnumerable<DataRange> ranges, TimeRange subtractRange);
    }
}