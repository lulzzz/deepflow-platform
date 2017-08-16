using System.Collections.Generic;

namespace Deepflow.Platform.Abstractions.Series
{
    public interface IDataAggregator
    {
        IDictionary<int, IEnumerable<DataRange>> AddToAggregations(IDictionary<int, IEnumerable<DataRange>> aggregations, IEnumerable<DataRange> rawDataRanges, HashSet<int> levels);
    }
}