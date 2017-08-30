using System.Collections.Generic;
using System.Threading.Tasks;
using Orleans;

namespace Deepflow.Platform.Abstractions.Series
{
    public interface IAttributeSeriesGrain : IGrainWithStringKey
    {
        Task<IEnumerable<DataRange>> GetData(TimeRange timeRange, int aggregationSeconds);
        Task AddData(IEnumerable<DataRange> dataRanges, int aggregationSeconds);
        Task Subscribe(ISeriesObserver observer);
        Task Unsubscribe(ISeriesObserver observer);
    }
}