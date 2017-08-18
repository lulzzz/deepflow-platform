using System.Threading.Tasks;
using Orleans;

namespace Deepflow.Platform.Abstractions.Series
{
    public interface IAttributeSeriesGrain : IGrainWithStringKey
    {
        Task<DataRange> GetData(TimeRange timeRange, int aggregationSeconds);
    }
}