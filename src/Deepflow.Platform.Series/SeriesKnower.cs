using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Deepflow.Platform.Abstractions.Series;
using Microsoft.Extensions.Options;

namespace Deepflow.Platform.Series
{
    public class SeriesKnower : ISeriesKnower
    {
        private readonly IOptions<SeriesConfiguration> _seriesConfiguration;
        private readonly ConcurrentDictionary<Guid, ConcurrentDictionary<int, AttributeSeries>> _keyedAttributeSeries = new ConcurrentDictionary<Guid, ConcurrentDictionary<int, AttributeSeries>>();
        private readonly ConcurrentDictionary<Guid, ConcurrentDictionary<int, CalculationSeries>> _keyedCalculationSeries = new ConcurrentDictionary<Guid, ConcurrentDictionary<int, CalculationSeries>>();
        private readonly ConcurrentDictionary<Guid, AttributeSeries> _attributeSeries = new ConcurrentDictionary<Guid, AttributeSeries>();
        private readonly ConcurrentDictionary<Guid, CalculationSeries> _calculationSeries = new ConcurrentDictionary<Guid, CalculationSeries>();

        public SeriesKnower(IOptions<SeriesConfiguration> seriesConfiguration)
        {
            _seriesConfiguration = seriesConfiguration;
        }

        public Task<Guid> GetAttributeSeriesGuid(Guid entity, Guid attribute, int aggregationSeconds)
        {
            var series = new AttributeSeries { Guid = Guid.NewGuid(), Entity = entity, Attribute = attribute, AggregationSeconds = aggregationSeconds };
            var seriesByAggregation = _keyedAttributeSeries.AddOrUpdate(series.Guid, new ConcurrentDictionary<int, AttributeSeries>(), (guid, old) => old);
            seriesByAggregation.AddOrUpdate(aggregationSeconds, series, (seconds, oldSeries) => series);
            _attributeSeries.AddOrUpdate(series.Guid, series, (guid, attributeSeries) => series);
            return Task.FromResult(series.Guid);
        }

        public Task<Guid> GetCalculationSeriesGuid(Guid entity, Guid calculation, int aggregationSeconds)
        {
            var series = new CalculationSeries { Guid = Guid.NewGuid(), Entity = entity, Calculation = calculation, AggregationSeconds = aggregationSeconds };
            var seriesByAggregation = _keyedCalculationSeries.AddOrUpdate(series.Guid, new ConcurrentDictionary<int, CalculationSeries>(), (guid, old) => old);
            seriesByAggregation.AddOrUpdate(aggregationSeconds, series, (seconds, oldSeries) => series);
            _calculationSeries.AddOrUpdate(series.Guid, series, (guid, calculationSeries) => series);
            return Task.FromResult(series.Guid);
        }

        public async Task<Dictionary<int, Guid>> GetAttributeSeriesGuids(Guid entity, Guid attribute)
        {
            var aggregations = _seriesConfiguration.Value.Aggregations.Select(x => new { Aggregation = x, Guid = GetAttributeSeriesGuid(entity, attribute, x)});
            var tasks = aggregations.Select(x => x.Guid);
            await Task.WhenAll(tasks);
            return aggregations.ToDictionary(x => x.Aggregation, x => x.Guid.Result);
        }

        public async Task<Dictionary<int, Guid>> GetCalculationSeriesGuids(Guid entity, Guid calculation)
        {
            var aggregations = _seriesConfiguration.Value.Aggregations.Select(x => new { Aggregation = x, Guid = GetCalculationSeriesGuid(entity, calculation, x) });
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

        public Task<CalculationSeries> GetCalculationSeries(Guid guid)
        {
            if (!_calculationSeries.TryGetValue(guid, out CalculationSeries series))
            {
                throw new Exception($"Cannot find calculation series with GUID {guid}");
            }

            return Task.FromResult(series);
        }
    }
}
