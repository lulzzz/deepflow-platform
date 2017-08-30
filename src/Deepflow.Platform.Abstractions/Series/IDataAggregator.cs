using System.Collections.Generic;

namespace Deepflow.Platform.Abstractions.Series
{
    public interface IDataAggregator
    {
        AggregatedDataRange Aggregate(DataRange dataRange, int aggregationSeconds);
        IEnumerable<AggregatedDataRange> Aggregate(DataRange dataRange, IEnumerable<int> aggregationsSeconds);
    }
}