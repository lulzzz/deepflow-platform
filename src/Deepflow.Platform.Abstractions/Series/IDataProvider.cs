using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Deepflow.Platform.Abstractions.Series
{
    public interface IDataProvider
    {
        Task<IEnumerable<AggregatedDataRange>> GetAggregatedRanges(Guid series, TimeRange timeRange, int aggregationSeconds);
        Task SaveAggregatedRanges(Guid series, IEnumerable<AggregatedDataRange> dataRanges);
        Task SaveAggregatedRange(Guid series, AggregatedDataRange dataRange);
        Task<IEnumerable<TimeRange>> GetSavedAggregatedTimeRanges(Guid series);
    }
}