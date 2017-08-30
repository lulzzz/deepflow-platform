using System;
using System.Collections.Generic;
using System.Linq;
using Deepflow.Platform.Abstractions.Series;

namespace Deepflow.Platform.Series
{
    public class SeriesConfiguration : ISeriesConfiguration
    {
        public SeriesConfiguration(SeriesSettings settings)
        {
            if (settings.Aggregations == null || !settings.Aggregations.Any())
            {
                throw new Exception("No aggregations set in series settings.");
            }

            AggregationsSecondsDescending = settings.Aggregations.OrderByDescending(x => x);
            HighestAggregationSeconds = AggregationsSecondsDescending.First();
            LowestAggregationSeconds = AggregationsSecondsDescending.Last();
        }

        public int HighestAggregationSeconds { get; }
        public int LowestAggregationSeconds { get; }
        public IEnumerable<int> AggregationsSecondsDescending { get; }
    }
}
