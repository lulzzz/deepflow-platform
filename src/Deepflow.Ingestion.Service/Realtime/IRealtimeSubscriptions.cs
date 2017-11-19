using System;
using System.Collections.Concurrent;
using System.Collections.Generic;

namespace Deepflow.Ingestion.Service.Realtime
{
    public interface IRealtimeSubscriptions
    {
        ConcurrentDictionary<string, Dictionary<Guid, HashSet<Guid>>> GetSubscriptions();
    }
}
