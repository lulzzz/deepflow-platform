using System;
using System.Collections.Concurrent;
using System.Threading.Tasks;
using Deepflow.Platform.Abstractions.Series;

namespace Deepflow.Platform.Series
{
    public class SeriesKnower : ISeriesKnower
    {
        private readonly ConcurrentDictionary<Guid, Abstractions.Series.Series> _series = new ConcurrentDictionary<Guid, Abstractions.Series.Series>();

        public Task<Guid> GetSeriesGuid(Guid entity, Guid attribute, int aggregationSeconds)
        {
            var series = new Abstractions.Series.Series { Guid = Guid.NewGuid(), Entity = entity, Attribute = attribute };
            _series.AddOrUpdate(series.Guid, series, (guid, series1) => series);
            return Task.FromResult(series.Guid);
        }

        public Task<Abstractions.Series.Series> GetSeries(Guid series)
        {
            return Task.FromResult(_series[series]);
        }
    }
}
