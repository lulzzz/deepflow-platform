using System.Collections.Generic;
using System.Threading.Tasks;
using Orleans;

namespace Deepflow.Platform.Abstractions.Series.Attribute
{
    public interface IAttributeSeriesGrain : IGrainWithStringKey
    {
        Task<IEnumerable<AggregatedDataRange>> GetAggregatedData(TimeRange timeRange, int aggregationSeconds);
        Task ReceiveData(AggregatedDataRange aggregatedRange);
        Task Subscribe(ISeriesObserver observer);
        Task Unsubscribe(ISeriesObserver observer);
    }
}