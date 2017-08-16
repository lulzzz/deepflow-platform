using System.Collections.Generic;
using System.Threading.Tasks;
using Orleans;

namespace Deepflow.Platform.Abstractions.Series
{
    public interface ISeriesGrain : IGrainWithStringKey
    {
        Task AddRawData(IEnumerable<DataRange> dataRanges);
        Task<IEnumerable<DataRange>> GetAggregatedData(TimeRange timeRange, int aggregationSeconds);

        Task Subscribe(ISeriesObserver observer);
        Task Unsubscribe(ISeriesObserver observer);
    }
}
