using System.Collections.Generic;

namespace Deepflow.Platform.Abstractions.Series
{
    public interface IDataAggregator
    {
        IEnumerable<AggregatedDataRange> Aggregate(IEnumerable<AggregatedDataRange> dataRanges, TimeRange timeRange, IEnumerable<int> aggregationSeconds);
    }
}