namespace Deepflow.Ingestion.Service.Configuration
{
    public class IngestionConfiguration
    {
        public PersistencePlugin PersistencePlugin { get; set; }
        public int IngestionParallelism { get; set; }
        public long MinHistoryUtcSeconds { get; set; }
    }

    public enum PersistencePlugin
    {
        Cassandra,
        DynamoDb,
        Noop
    }
}
