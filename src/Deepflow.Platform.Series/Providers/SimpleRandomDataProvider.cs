/*
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Deepflow.Platform.Abstractions.Series;

namespace Deepflow.Platform.Series.Providers
{
    public class SimpleRandomDataProvider : InMemoryDataProvider
    {
        private readonly ISeriesKnower _knower;

        public SimpleRandomDataProvider(ISeriesKnower knower, IRangeMerger merger, IRangeFilterer dataFilterer, ITimeFilterer timeFilterer) : base(merger, dataFilterer, timeFilterer)
        {
            _knower = knower;
        }

        protected override async Task<IEnumerable<RawDataRange>> ProduceAttributeRanges(Guid series, IEnumerable<TimeRange> timeRanges)
        {
            return await Task.WhenAll(timeRanges.Select(timeRange => GetAttributeRange(series, timeRange)));
        }

        private async Task<RawDataRange> GetAttributeRange(Guid guid, TimeRange timeRange)
        {
            var series = await _knower.GetAttributeSeries(guid);
            var random = new Random();

            var minTime = (int)Math.Ceiling((double)timeRange.Min / series.AggregationSeconds);
            var maxTime = (int)Math.Floor((double)timeRange.Max / series.AggregationSeconds);
            var data = Enumerable.Range(minTime, maxTime - minTime).Select(i => random.NextDouble()).ToList();
            return new RawDataRange(timeRange, data);
        }
    }
}
*/
