using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Deepflow.Platform.Abstractions.Series;
using Deepflow.Platform.Abstractions.Series.Attribute;

namespace Deepflow.Platform.Series
{
    public class SeriesKnower : ISeriesKnower
    {
        private readonly SeriesSettings _seriesSettings;
        private readonly ConcurrentDictionary<Guid, ConcurrentDictionary<int, AttributeSeries>> _keyedAttributeSeries = new ConcurrentDictionary<Guid, ConcurrentDictionary<int, AttributeSeries>>();
        private readonly ConcurrentDictionary<Guid, AttributeSeries> _attributeSeries = new ConcurrentDictionary<Guid, AttributeSeries>();
        private readonly ConcurrentDictionary<Tuple<Guid, Guid, int>, Guid> _seriesGuidByEntityAttribute = new ConcurrentDictionary<Tuple<Guid, Guid, int>, Guid>();

        public SeriesKnower(SeriesSettings seriesSettings)
        {
            _seriesSettings = seriesSettings;
        }

        public Task<Guid> GetAttributeSeriesGuid(Guid entity, Guid attribute, int aggregationSeconds)
        {
            if (_seriesGuidByEntityAttribute.TryGetValue(new Tuple<Guid, Guid, int>(entity, attribute, aggregationSeconds), out Guid seriesGuid))
            {
                return Task.FromResult(seriesGuid);
            }

            var series = new AttributeSeries { Guid = Guid.NewGuid(), Entity = entity, Attribute = attribute, AggregationSeconds = aggregationSeconds };
            var seriesByAggregation = _keyedAttributeSeries.AddOrUpdate(series.Guid, new ConcurrentDictionary<int, AttributeSeries>(), (guid, old) => old);
            seriesByAggregation.AddOrUpdate(aggregationSeconds, series, (seconds, oldSeries) => series);
            _attributeSeries.AddOrUpdate(series.Guid, series, (guid, attributeSeries) => series);
            _seriesGuidByEntityAttribute.AddOrUpdate(new Tuple<Guid, Guid, int>(entity, attribute, aggregationSeconds), series.Guid, (tuple, guid) => series.Guid);
            return Task.FromResult(series.Guid);
        }

        public async Task<Dictionary<int, Guid>> GetAttributeSeriesGuids(Guid entity, Guid attribute)
        {
            var aggregations = _seriesSettings.Aggregations.Select(x => new { Aggregation = x, Guid = GetAttributeSeriesGuid(entity, attribute, x)});
            var tasks = aggregations.Select(x => x.Guid);
            await Task.WhenAll(tasks);
            return aggregations.ToDictionary(x => x.Aggregation, x => x.Guid.Result);
        }

        public Task<AttributeSeries> GetAttributeSeries(Guid guid)
        {
            if (!_attributeSeries.TryGetValue(guid, out AttributeSeries series))
            {
                throw new Exception($"Cannot find attribute series with GUID {guid}");
            }

            return Task.FromResult(series);
        }
    }
}
