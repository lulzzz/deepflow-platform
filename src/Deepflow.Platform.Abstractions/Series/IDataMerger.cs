using System.Collections.Generic;

namespace Deepflow.Platform.Abstractions.Series
{
    public interface IDataMerger<TRange> where TRange : IDataRange
    {
        IEnumerable<TRange> MergeDataRangeWithRanges(IEnumerable<TRange> ranges, TRange incomingRange);
        IEnumerable<TRange> MergeDataRangeWithRange(TRange range, TRange incomingRange);
        IEnumerable<TRange> MergeDataRangesWithRanges(IEnumerable<TRange> ranges, IEnumerable<TRange> incomingRanges);
    }
}