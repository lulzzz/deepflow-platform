using System.Threading.Tasks;
using Deepflow.Platform.Abstractions.Series;

namespace Deepflow.Platform.Agent.Client
{
    public interface IIngestionClient
    {
        Task Start();
        Task SendRealtimeData(string name, AggregatedDataRange aggregatedDataRange, RawDataRange rawDataRange);
        Task SendHistoricalData(string name, AggregatedDataRange aggregatedDataRange);
    }
}