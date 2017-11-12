using System.Threading.Tasks;
using Deepflow.Platform.Abstractions.Series;

namespace Deepflow.Platform.Agent.Client
{
    public interface IIngestionClient
    {
        Task Start();
        Task SendRealtimeAggregatedData(string name, AggregatedDataRange dataRange);
        Task SendRealtimeRawData(string name, RawDataRange dataRange);
        Task SendHistoricalData(string name, AggregatedDataRange aggregatedDataRange);
    }
}