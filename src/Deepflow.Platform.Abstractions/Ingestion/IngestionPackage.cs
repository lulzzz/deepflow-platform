using Deepflow.Platform.Abstractions.Series;

namespace Deepflow.Platform.Abstractions.Ingestion
{
    public class IngestionPackage
    {
        public AggregatedDataRange AggregatedDataRange { get; set; }
        public RawDataRange RawDataRange { get; set; }
    }
}
