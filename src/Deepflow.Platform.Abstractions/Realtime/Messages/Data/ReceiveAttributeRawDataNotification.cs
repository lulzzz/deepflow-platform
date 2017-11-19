using System;
using Deepflow.Platform.Abstractions.Series;

namespace Deepflow.Platform.Abstractions.Realtime.Messages.Data
{
    public class ReceiveAttributeRawDataNotification : NotificationMessage
    {
        public Guid EntityGuid { get; set; }
        public Guid AttributeGuid { get; set; }
        public RawDataRange DataRange { get; set; }
    }
}
