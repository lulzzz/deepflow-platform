using System;
using System.Threading.Tasks;
using Deepflow.Platform.Abstractions.Series;

namespace Deepflow.Ingestion.Service.Processing
{
    public interface IIngestionProcessor
    {
        Task ReceiveRealtimeData(Guid entity, Guid attribute, AggregatedDataRange dataRange, RawDataRange rawDataRange);
        Task ReceiveHistoricalData(Guid entity, Guid attribute, AggregatedDataRange dataRange);
    }
}