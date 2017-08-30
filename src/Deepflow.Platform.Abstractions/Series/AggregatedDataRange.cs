using System.Collections.Generic;

namespace Deepflow.Platform.Abstractions.Series
{
    public class AggregatedDataRange
    {
        public AggregatedDataRange(DataRange dataRange, int aggregationSeconds)
        {
            DataRange = dataRange;
            AggregationSeconds = aggregationSeconds;
        }

        public AggregatedDataRange(TimeRange timeRange, List<double> data, int aggregationSeconds)
        {
            DataRange = new DataRange(timeRange, data);
            AggregationSeconds = aggregationSeconds;
        }

        public int AggregationSeconds { get; set; }
        public DataRange DataRange { get; set; }
    }
}
