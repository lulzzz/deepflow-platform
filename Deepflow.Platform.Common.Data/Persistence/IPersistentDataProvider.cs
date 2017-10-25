using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Deepflow.Platform.Abstractions.Series;

namespace Deepflow.Platform.Common.Data.Persistence
{
    public interface IPersistentDataProvider
    {
        Task<IEnumerable<AggregatedDataRange>> GetData(Guid series, TimeRange timeRange);
        Task SaveData(IEnumerable<(Guid series, IEnumerable<AggregatedDataRange> dataRanges)> seriesData);

        Task<IEnumerable<TimeRange>> GetTimeRanges(Guid series, TimeRange timeRange);
        Task<IEnumerable<TimeRange>> GetAllTimeRanges(Guid series);
        Task SaveTimeRanges(Guid series, IEnumerable<TimeRange> timeRanges);
    }
}