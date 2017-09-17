using System;
using System.Collections.Generic;
using Deepflow.Platform.Abstractions.Series;

namespace Deepflow.Platform.Controllers
{
    public class RawDataSubscriptionMessage
    {
        public Guid Entity { get; set; }
        public Guid Attribute { get; set; }
        public IEnumerable<RawDataRange> DataRanges { get; set; }
    }
}
