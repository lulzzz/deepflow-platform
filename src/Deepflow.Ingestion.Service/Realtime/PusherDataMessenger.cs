using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Deepflow.Ingestion.Service.Configuration;
using Deepflow.Platform.Abstractions.Series;
using Deepflow.Platform.Core.Tools;
using Newtonsoft.Json;
using PusherServer;

namespace Deepflow.Ingestion.Service.Realtime
{
    public class PusherDataMessenger : IDataMessenger
    {
        private readonly RealtimeConfiguration _configuration;
        private readonly Pusher _pusher;

        public PusherDataMessenger(RealtimeConfiguration configuration)
        {
            _configuration = configuration;
            var options = new PusherOptions { Cluster = "ap1", Encrypted = true };
            _pusher = new Pusher(_configuration.AppId, _configuration.AppKey, _configuration.AppSecret, options);
        }

        public async Task NotifyRaw(Guid entity, Guid attribute, RawDataRange dataRange)
        {
            await _pusher.TriggerAsync(_configuration.ChannelName, $"{entity}_{attribute}_raw", JsonConvert.SerializeObject(dataRange, JsonSettings.Setttings));
        }

        public async Task NotifyAggregated(Guid entity, Guid attribute, Dictionary<int, AggregatedDataRange> dataRanges)
        {
            await _pusher.TriggerAsync(_configuration.ChannelName, $"{entity}_{attribute}_aggregated", JsonConvert.SerializeObject(dataRanges, JsonSettings.Setttings));
        }
    }
}
