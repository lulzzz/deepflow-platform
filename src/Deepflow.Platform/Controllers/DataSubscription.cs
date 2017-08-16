using System;
using Deepflow.Platform.Abstractions.Series;

namespace Deepflow.Platform.Controllers
{
    public class DataSubscription
    {
        public Guid Entity { get; set; }
        public Guid Attribute { get; set; }
        public ISeriesObserver Observer { get; set; }

        public override bool Equals(object obj)
        {
            DataSubscription subscription = obj as DataSubscription;
            return subscription != null && subscription.Entity == Entity && subscription.Attribute == Attribute;
        }

        public override int GetHashCode()
        {
            return Entity.GetHashCode() ^ Attribute.GetHashCode();
        }
    }
}
