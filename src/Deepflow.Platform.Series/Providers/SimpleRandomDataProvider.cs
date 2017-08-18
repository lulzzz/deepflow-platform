using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Deepflow.Platform.Abstractions.Series;

namespace Deepflow.Platform.Series.Providers
{
    public class SimpleRandomDataProvider : IDataProvider
    {
        private readonly ISeriesKnower _knower;

        public SimpleRandomDataProvider(ISeriesKnower knower)
        {
            _knower = knower;
        }

        public async Task<IEnumerable<DataRange>> GetAttributeRanges(Guid guid, IEnumerable<TimeRange> timeRanges)
        {
            return await Task.WhenAll(timeRanges.Select(timeRange => GetAttributeRange(guid, timeRange)));
        }

        public async Task<DataRange> GetAttributeRange(Guid guid, TimeRange timeRange)
        {
            var series = await _knower.GetAttributeSeries(guid);
            var random = new Random();

            var minTime = (int)Math.Ceiling((double)timeRange.MinSeconds / series.AggregationSeconds);
            var maxTime = (int)Math.Floor((double)timeRange.MaxSeconds / series.AggregationSeconds);
            var data = Enumerable.Range(minTime, maxTime - minTime).Select(i => random.NextDouble()).ToList();
            return new DataRange(timeRange, data);
        }
    }
}
