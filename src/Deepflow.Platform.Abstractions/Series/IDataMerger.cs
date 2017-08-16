using System.Collections.Generic;

namespace Deepflow.Platform.Abstractions.Series
{
    public interface IDataMerger
    {
        IEnumerable<DataRange> MergeDataRangeWithRanges(IEnumerable<DataRange> ranges, DataRange dataRange);

        IEnumerable<DataRange> MergeDataRangesWithRanges(IEnumerable<DataRange> ranges, IEnumerable<DataRange> dataRanges);
    }
}