using System;
using System.Collections.Generic;

namespace Deepflow.Platform.Abstractions.Realtime.Messages.Subscriptions
{
    public class UpdateAttributeSubscriptionsRequest : RequestMessage
    {
        public IEnumerable<EntitySubscriptions> EntitySubscriptions { get; set; }
    }

    public class EntitySubscriptions
    {
        public Guid EntityGuid { get; set; }
        public IEnumerable<Guid> AttributeGuids { get; set; }
    }
}
