using System;

namespace Deepflow.Platform.Abstractions.Series
{
    public class CalculationSeries
    {
        public Guid Guid { get; set; }
        public Guid Entity { get; set; }
        public Guid Calculation { get; set; }
        public int AggregationSeconds { get; set; }
    }
}
