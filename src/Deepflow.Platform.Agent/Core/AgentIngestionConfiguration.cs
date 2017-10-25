using System;

namespace Deepflow.Platform.Agent.Core
{
    public class AgentIngestionConfiguration
    {
        public bool Disabled { get;set }
        public Uri ApiBaseUrl { get; set; }
        public Uri RealtimeBaseUrl { get; set; }
        public Guid DataSource { get; set; }
        public string DataSourceFriendlyName { get; set; }
        public int ClientRetrySeconds { get; set; }
        public int FetchFailedPauseSeconds { get; set; }
        public int PushFailedPauseSeconds { get; set; }
        public int PushFailedRetryCount { get; set; }
        public int AggregationSeconds { get; set; }
        public int MaxFetchSpanSeconds { get; set; }
        public int BetweenFetchPauseSeconds { get; set; }
        public int SendParallelism { get; set; }
    }
}
