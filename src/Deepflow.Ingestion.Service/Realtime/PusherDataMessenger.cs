using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Deepflow.Platform.Abstractions.Series;
using Deepflow.Platform.Core.Tools;
using Newtonsoft.Json;
using PusherServer;

namespace Deepflow.Ingestion.Service.Realtime
{
    public class PusherDataMessenger : IDataMessenger
    {
        private readonly Pusher _pusher;

        public PusherDataMessenger()
        {
            var options = new PusherOptions { Cluster = "ap1", Encrypted = true };
            _pusher = new Pusher("408288", "22ff7105446ab14b8512", "bf56749c3712246ac403", options);
        }

        public async Task Notify(Guid entity, Guid attribute, Dictionary<int, AggregatedDataRange> dataRanges, RawDataRange rawDataRange)
        {
            await _pusher.TriggerAsync($"deepflow-demo", $"{entity}_{attribute}", JsonConvert.SerializeObject(new NotificationPackage { AggregatedDataRanges = dataRanges, RawDataRange = rawDataRange }, JsonSettings.Setttings));
        }

        private class NotificationPackage
        {
            public Dictionary<int, AggregatedDataRange> AggregatedDataRanges { get; set; }
            public RawDataRange RawDataRange { get; set; }
        }
    }
}
