namespace Deepflow.Ingestion.Service.Configuration
{
    public class IngestionConfiguration
    {
        public int IngestionParallelism { get; set; }
        public long MinHistoryUtcSeconds { get; set; }
    }
}
