using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Deepflow.Platform.Abstractions.Series;

namespace Deepflow.Platform.Common.Data.Caching
{
    public interface ICachedDataProvider
    {
        Task<IEnumerable<AggregatedDataRange>> GetData(Guid series, TimeRange timeRange, int aggregationSeconds);
        Task SaveData(Guid series, IEnumerable<AggregatedDataRange> dataRanges);

        Task<IEnumerable<TimeRange>> GetTimeRanges(Guid series, TimeRange timeRange, int aggregationSeconds);
        Task SaveTimeRanges(Guid series, IEnumerable<TimeRange> timeRanges, int aggregationSeconds);
    }
}