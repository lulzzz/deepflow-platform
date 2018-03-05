using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Deepflow.Platform.Abstractions.Series;

namespace Deepflow.Platform.Common.Data.Persistence
{
    public class NoopPersistenceProvider : IPersistentDataProvider
    {
        public Task<IEnumerable<AggregatedDataRange>> GetAggregatedDataWithEdges(Guid entity, Guid attribute, int aggregationSeconds, TimeRange timeRange)
        {
            return Task.FromResult((IEnumerable<AggregatedDataRange>)new List<AggregatedDataRange>());
        }

        public Task<IEnumerable<AggregatedDataRange>> GetAggregatedData(Guid entity, Guid attribute, int aggregationSeconds, TimeRange timeRange)
        {
            return Task.FromResult((IEnumerable<AggregatedDataRange>)new List<AggregatedDataRange>());
        }

        public Task SaveData(IEnumerable<(Guid entity, Guid attribute, int aggregationSeconds, IEnumerable<AggregatedDataRange> dataRanges)> seriesData)
        {
            return Task.CompletedTask;
        }

        public Task<IEnumerable<TimeRange>> GetTimeRanges(Guid entity, Guid attribute, TimeRange timeRange)
        {
            return Task.FromResult((IEnumerable<TimeRange>)new List<TimeRange>());
        }

        public Task<IEnumerable<TimeRange>> GetAllTimeRanges(Guid entity, Guid attribute)
        {
            return Task.FromResult((IEnumerable<TimeRange>)new List<TimeRange>());
        }

        public Task SaveTimeRanges(Guid entity, Guid attribute, IEnumerable<TimeRange> timeRanges)
        {
            return Task.CompletedTask;
        }
    }
}
