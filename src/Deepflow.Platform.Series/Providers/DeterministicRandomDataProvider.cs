/*
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Deepflow.Platform.Abstractions.Series;

namespace Deepflow.Platform.Series.Providers
{
    public class DeterministicRandomDataProvider : InMemoryDataProvider
    {
        private readonly ISeriesKnower _knower;
        private readonly IRangeFilterer _dataFilterer;
        private readonly IRangeMerger _merger;
        private readonly SeriesSettings _seriesSettings;
        private readonly ReverseDataGenerator _generator;

        public DeterministicRandomDataProvider(ISeriesKnower knower, IRangeFilterer dataFilterer, ITimeFilterer timeFilterer, IRangeMerger merger, SeriesSettings seriesSettings) : base(merger, dataFilterer, timeFilterer)
        {
            _knower = knower;
            _dataFilterer = dataFilterer;
            _merger = merger;
            _seriesSettings = seriesSettings;
            _generator = new ReverseDataGenerator(_dataFilterer, new HashValueGenerator(), _merger);
        }

        protected override async Task<IEnumerable<AggregatedDataRange>> ProduceAttributeRanges(Guid series, IEnumerable<TimeRange> timeRanges)
        {
            return await Task.WhenAll(timeRanges.Select(timeRange => GetAttributeRange(series, timeRange)));
        }

        private async Task<AggregatedDataRange> GetAttributeRange(Guid guid, TimeRange timeRange)
        {
            var series = await _knower.GetAttributeSeries(guid);
            var name = $"{series.Entity}:{series.Attribute}";
            var dataRange = _generator.GenerateReverseAverage(name, timeRange, _seriesSettings.Aggregations.ToArray(), series.AggregationSeconds, 500, 1000);
            return dataRange;
        }
    }
}
*/
