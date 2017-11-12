using System;
using System.Threading.Tasks;
using Deepflow.Platform.Abstractions.Series;

namespace Deepflow.Ingestion.Service.Processing
{
    public interface IIngestionProcessor
    {
        Task ReceiveRealtimeRawData(Guid entity, Guid attribute, RawDataRange rawDataRange);
        Task ReceiveRealtimeAggregatedData(Guid entity, Guid attribute, AggregatedDataRange dataRange);
        Task ReceiveHistoricalData(Guid entity, Guid attribute, AggregatedDataRange dataRange);
    }
}