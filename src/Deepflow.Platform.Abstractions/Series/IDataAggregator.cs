using System.Collections.Generic;

namespace Deepflow.Platform.Abstractions.Series
{
    public interface IDataAggregator
    {
        Dictionary<int, IEnumerable<AggregatedDataRange>> Aggregate(IEnumerable<AggregatedDataRange> dataRanges, TimeRange timeRange, IEnumerable<int> aggregationSeconds);
    }
}