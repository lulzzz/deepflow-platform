using System.Collections.Generic;

namespace Deepflow.Platform.Abstractions.Series
{
    public interface IDataMerger
    {
        IEnumerable<DataRange> MergeDataRangeWithRanges(IEnumerable<DataRange> ranges, DataRange incomingRange);
        IEnumerable<DataRange> MergeDataRangeWithRange(DataRange range, DataRange incomingRange);

        IEnumerable<DataRange> MergeDataRangesWithRanges(IEnumerable<DataRange> ranges, IEnumerable<DataRange> incomingRanges);
    }
}