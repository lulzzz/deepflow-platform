using System;
using System.Collections.Generic;
using Deepflow.Platform.Abstractions.Series;

namespace Deepflow.Platform.Abstractions.Realtime.Messages.Data
{
    public class ReceiveAttributeDataNotification : NotificationMessage
    {
        public Guid EntityGuid { get; set; }
        public Guid AttributeGuid { get; set; }
        public AttributeDataNotification Notification { get; set; }
    }

    public class AttributeDataNotification
    {
        public Dictionary<int, AggregatedDataRange> AggregatedRanges { get; set; }
    }
}
