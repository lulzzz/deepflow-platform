using System.Collections.Generic;

namespace Deepflow.Platform.Abstractions.Series
{
    public interface IDataJoiner<TRange> where TRange : IDataRange
    {
        IEnumerable<TRange> JoinDataRangesToDataRanges(IEnumerable<TRange> dataRanges, IEnumerable<TRange> newDataRanges);
        IEnumerable<TRange> JoinDataRangeToDataRanges(IEnumerable<TRange> ranges, TRange newRange);
    }
}