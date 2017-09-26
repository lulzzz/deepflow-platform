using System.Collections.Generic;
using System.Threading.Tasks;
using Deepflow.Platform.Abstractions.Series;

namespace Deepflow.Platform.Agent.Client
{
    public interface IIngestionClient
    {
        Task Start();
        Task SendData(string name, AggregatedDataRange aggregatedDataRange);
    }
}