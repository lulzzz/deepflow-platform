using System.Collections.Generic;
using System.Threading.Tasks;
using Orleans;

namespace Deepflow.Platform.Abstractions.Series
{
    public interface ISourceSeriesGrain : IGrainWithStringKey
    {
        Task AddAggregatedData(DataRange dataRange, int aggregationSeconds);
        Task AddAggregatedData(IEnumerable<DataRange> dataRanges, int aggregationSeconds);
        Task NotifyRawData(IEnumerable<DataRange> dataRanges);
    }
}