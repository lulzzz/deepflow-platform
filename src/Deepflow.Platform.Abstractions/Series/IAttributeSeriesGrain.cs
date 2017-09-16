using System.Collections.Generic;
using System.Threading.Tasks;
using Orleans;

namespace Deepflow.Platform.Abstractions.Series
{
    public interface IAttributeSeriesGrain : IGrainWithStringKey
    {
        Task<IEnumerable<DataRange>> GetData(TimeRange timeRange, int aggregationSeconds);
        Task AddAggregatedData(IEnumerable<DataRange> dataRanges, int aggregationSeconds);
        Task NotifyRawData(IEnumerable<DataRange> dataRanges);
        Task Subscribe(ISeriesObserver observer);
        Task Unsubscribe(ISeriesObserver observer);
    }
}