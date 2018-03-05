using System.Collections.Generic;

namespace Deepflow.Platform.Abstractions.Series
{
    public interface IRangeMerger<TRange>
    {
        IEnumerable<TRange> MergeRangeWithRanges(IEnumerable<TRange> ranges, TRange incomingRange);
        IEnumerable<TRange> MergeRangeWithRange(TRange range, TRange incomingRange);

        IEnumerable<TRange> MergeRangesWithRange(TRange range, IEnumerable<TRange> incomingRanges);
        IEnumerable<TRange> MergeRangesWithRanges(IEnumerable<TRange> ranges, IEnumerable<TRange> incomingRanges);
    }
}