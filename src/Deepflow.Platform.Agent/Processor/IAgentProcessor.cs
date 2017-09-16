using Deepflow.Platform.Abstractions.Sources;
using Deepflow.Platform.Agent.Client;

namespace Deepflow.Platform.Agent.Processor
{
    public interface IAgentProcessor
    {
        void SetClient(IIngestionClient client);
        void Start();
        void SetSourceSeriesList(SourceSeriesList sourceSeriesList);
    }
}