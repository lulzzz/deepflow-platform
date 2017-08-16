using System;
using System.Collections.Generic;
using Deepflow.Platform.Abstractions.Series;

namespace Deepflow.Platform.Controllers
{
    public class DataSubscriptionMessage
    {
        public Guid Entity { get; set; }
        public Guid Attribute { get; set; }
        public IEnumerable<AggregatedDataRange> DataRanges { get; set; }
    }
}
