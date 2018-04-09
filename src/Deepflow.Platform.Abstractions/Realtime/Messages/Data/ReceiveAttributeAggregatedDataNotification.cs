using System;
using System.Collections.Generic;
using Deepflow.Platform.Abstractions.Series;

namespace Deepflow.Platform.Abstractions.Realtime.Messages.Data
{
    public class ReceiveAttributeAggregatedDataNotification : NotificationMessage
    {
        public Guid EntityGuid { get; set; }
        public Guid AttributeGuid { get; set; }
        public Dictionary<int, IEnumerable<AggregatedDataRange>> AggregatedRanges { get; set; }
    }
}
