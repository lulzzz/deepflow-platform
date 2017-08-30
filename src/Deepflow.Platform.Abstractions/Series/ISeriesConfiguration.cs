using System.Collections.Generic;

namespace Deepflow.Platform.Abstractions.Series
{
    public interface ISeriesConfiguration
    {
        int HighestAggregationSeconds { get; }
        int LowestAggregationSeconds { get; }

        IEnumerable<int> AggregationsSecondsDescending { get; }
    }
}