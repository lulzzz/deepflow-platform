using System.Collections.Generic;

namespace Deepflow.Platform.Controllers
{
    public class DataRequestMessage
    {
        public IEnumerable<DataSubscription> Subscriptions { get; set; }
    }
}
