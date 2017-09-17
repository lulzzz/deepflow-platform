using System.Collections.Generic;
using System.Threading.Tasks;
using Deepflow.Platform.Abstractions.Series;

namespace Deepflow.Platform.Agent.Client
{
    public interface IIngestionClient
    {
        Task Start();
        Task SendAggregatedRange(string name, RawDataRange dataRange, int aggregationSeconds);
        Task SendRawRange(string name, RawDataRange dataRange);
    }
}