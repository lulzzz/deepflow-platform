using System.Threading.Tasks;
using Deepflow.Platform.Abstractions.Series;
using Deepflow.Platform.Abstractions.Sources;
using Deepflow.Platform.Agent.Client;

namespace Deepflow.Platform.Agent.Processor
{
    public interface IAgentProcessor
    {
        void SetClient(IIngestionClient client);
        void Start();
        void SetSourceSeriesList(SourceSeriesList sourceSeriesList);
        Task ReceiveRealtimeRaw(string sourceName, RawDataRange rawDataRange);
    }
}