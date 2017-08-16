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

        public async Task<IEnumerable<DataRange>> GetRanges(Guid guid, IEnumerable<TimeRange> timeRanges)
        {
            var series = await _knower.GetSeries(guid);
            var random = new Random();

            return timeRanges.Select(timeRange =>
            {
                var minTime = (int) Math.Ceiling((double)timeRange.MinSeconds / series.AggregationSeconds);
                var maxTime = (int) Math.Floor((double)timeRange.MaxSeconds / series.AggregationSeconds);
                var data = Enumerable.Range(minTime, maxTime - minTime).Select(i => random.NextDouble()).ToList();
                return new DataRange(timeRange, data);
            });
        }
    }
}
