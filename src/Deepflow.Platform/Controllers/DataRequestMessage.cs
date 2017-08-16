using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Deepflow.Platform.Controllers
{
    public class DataRequestMessage
    {
        public IEnumerable<DataSubscription> Subscriptions { get; set; }
    }
}
