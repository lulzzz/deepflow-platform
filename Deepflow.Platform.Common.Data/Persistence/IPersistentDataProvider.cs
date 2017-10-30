using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Deepflow.Platform.Abstractions.Series;

namespace Deepflow.Platform.Common.Data.Persistence
{
    public interface IPersistentDataProvider
    {
        Task<IEnumerable<AggregatedDataRange>> GetData(Guid entity, Guid attribute, int aggregationSeconds, TimeRange timeRange);
        Task SaveData(IEnumerable<(Guid entity, Guid attribute, int aggregationSeconds, IEnumerable<AggregatedDataRange> dataRanges)> seriesData);

        Task<IEnumerable<TimeRange>> GetTimeRanges(Guid entity, Guid attribute, TimeRange timeRange);
        Task<IEnumerable<TimeRange>> GetAllTimeRanges(Guid entity, Guid attribute);
        Task SaveTimeRanges(Guid entity, Guid attribute, IEnumerable<TimeRange> timeRanges);
    }
}