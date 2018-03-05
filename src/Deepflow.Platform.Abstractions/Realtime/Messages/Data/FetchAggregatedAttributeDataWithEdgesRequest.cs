using System;
using Deepflow.Platform.Abstractions.Series;

namespace Deepflow.Platform.Abstractions.Realtime.Messages.Data
{
    public class FetchAggregatedAttributeDataWithEdgesRequest : RequestMessage
    {
        public Guid EntityGuid { get; set; }
        public Guid AttributeGuid { get; set; }
        public int AggregationSeconds { get; set; }
        public TimeRange TimeRange { get; set; }
    }
}
