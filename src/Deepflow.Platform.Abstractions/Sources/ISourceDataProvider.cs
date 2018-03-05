using System;
using System.Threading;
using System.Threading.Tasks;
using Deepflow.Platform.Abstractions.Series;

namespace Deepflow.Platform.Agent.Provider
{
    public interface ISourceDataProvider
    {
        Task<AggregatedDataRange> FetchAggregatedData(string sourceName, TimeRange timeRange, int aggregationSeconds);
        Task<RawDataRange> FetchRawDataWithEdges(string sourceName, TimeRange timeRange);
        Task SubscribeForRawData(string sourceName, CancellationToken cancellationToken, Func<RawDataRange, Task> onReceive);
    }
}