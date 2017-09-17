using System.Collections.Generic;
using System.Threading.Tasks;
using Orleans;

namespace Deepflow.Platform.Abstractions.Series
{
    public interface IAttributeSeriesGrain : IGrainWithStringKey
    {
        Task<IEnumerable<AggregatedDataRange>> GetAggregatedData(TimeRange timeRange, int aggregationSeconds);
        Task AddAggregatedData(IEnumerable<AggregatedDataRange> dataRanges, int aggregationSeconds);
        Task NotifyRawData(IEnumerable<RawDataRange> dataRanges);
        Task Subscribe(ISeriesObserver observer);
        Task Unsubscribe(ISeriesObserver observer);
    }
}