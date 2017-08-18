using System;

namespace Deepflow.Platform.Abstractions.Series
{
    public class AttributeSeries
    {
        public Guid Guid { get; set; }
        public Guid Entity { get; set; }
        public Guid Attribute { get; set; }
        public int AggregationSeconds { get; set; }
    }
}
