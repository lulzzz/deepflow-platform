using System.Collections.Generic;

namespace Deepflow.Platform.Abstractions.Series
{
    public interface IDataFilterer
    {
        IEnumerable<DataRange> FilterDataRanges(IEnumerable<DataRange> ranges, TimeRange timeRange);
        DataRange FilterDataRange(DataRange range, TimeRange timeRange);
        List<double> FilterData(List<double> data, TimeRange timeRange);
        IEnumerable<DataRange> SubtractTimeRangeFromRanges(IEnumerable<DataRange> ranges, TimeRange subtractRange);
    }
}