using System.Threading.Tasks;
using Orleans;

namespace Deepflow.Platform.Abstractions.Series
{
    public interface ISourceSeriesGrain : IGrainWithStringKey
    {
        Task AddData(AggregatedDataRange aggregatedRange);
    }
}