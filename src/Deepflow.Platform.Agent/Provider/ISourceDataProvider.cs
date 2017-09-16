using System;
using System.Threading;
using System.Threading.Tasks;
using Deepflow.Platform.Abstractions.Series;

namespace Deepflow.Platform.Agent.Provider
{
    public interface ISourceDataProvider
    {
        Task<DataRange> FetchAggregatedData(string sourceName, TimeRange timeRange, int aggregationSeconds);
        Task<DataRange> FetchRawData(string sourceName, TimeRange timeRange);
        Task SubscribeForRawData(string sourceName, CancellationToken cancellationToken, Func<DataRange, Task> onReceive);
    }
}