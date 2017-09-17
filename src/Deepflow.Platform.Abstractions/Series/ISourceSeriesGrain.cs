using System.Collections.Generic;
using System.Threading.Tasks;
using Orleans;

namespace Deepflow.Platform.Abstractions.Series
{
    public interface ISourceSeriesGrain : IGrainWithStringKey
    {
        Task AddAggregatedData(AggregatedDataRange dataRange, int aggregationSeconds);
        Task AddAggregatedData(IEnumerable<AggregatedDataRange> dataRanges, int aggregationSeconds);
        Task NotifyRawData(IEnumerable<RawDataRange> dataRanges);
    }
}