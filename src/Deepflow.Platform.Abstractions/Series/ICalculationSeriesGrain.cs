using System.Threading.Tasks;
using Orleans;

namespace Deepflow.Platform.Abstractions.Series
{
    public interface ICalculationSeriesGrain : IGrainWithStringKey
    {
        Task<DataRange> GetData(TimeRange timeRange, int aggregationSeconds);
    }
}