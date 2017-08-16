using System.Collections.Generic;

namespace Deepflow.Platform.Abstractions.Series
{
    public interface IDataJoiner
    {
        IEnumerable<DataRange> JoinDataRangesToDataRanges(IEnumerable<DataRange> dataRanges, IEnumerable<DataRange> newDataRanges);

        IEnumerable<DataRange> JoinDataRangeToDataRanges(IEnumerable<DataRange> ranges, DataRange newRange);
    }
}