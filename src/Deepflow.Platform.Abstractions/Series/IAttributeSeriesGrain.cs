using System.Collections.Generic;
using System.Threading.Tasks;
using Orleans;

namespace Deepflow.Platform.Abstractions.Series
{
    public interface IAttributeSeriesGrain : IGrainWithStringKey
    {
        Task<DataRange> GetData(TimeRange timeRange, int aggregationSeconds);
        Task AddRawData(IEnumerable<DataRange> dataRanges);
        Task Subscribe(ISeriesObserver observer);
        Task Unsubscribe(ISeriesObserver observer);
    }
}