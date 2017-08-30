using System.Collections.Generic;
using System.Threading.Tasks;
using Orleans;

namespace Deepflow.Platform.Abstractions.Series
{
    public interface ICalculationSeriesGrain : IGrainWithStringKey
    {
        Task<IEnumerable<DataRange>> GetData(TimeRange timeRange, int aggregationSeconds);
    }
}