using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using Deepflow.Ingestion.Service.Realtime;
using Microsoft.AspNetCore.Mvc;

namespace Deepflow.Ingestion.Service.Controllers
{
    [Route("api/v1/[controller]")]
    public class MonitoringController : Controller
    {
        private readonly IRealtimeSubscriptions _subscriptions;

        public MonitoringController(IRealtimeSubscriptions subscriptions)
        {
            _subscriptions = subscriptions;
        }

        [HttpGet]
        public ConcurrentDictionary<string, Dictionary<Guid, HashSet<Guid>>> GetSubscriptions()
        {
            return _subscriptions.GetSubscriptions();
        }
    }
}
