﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Deepflow.Platform.Abstractions.Series;

namespace Deepflow.Platform.Series.Providers
{
    public class SimpleRandomDataProvider : InMemoryDataProvider
    {
        private readonly ISeriesKnower _knower;

        public SimpleRandomDataProvider(ISeriesKnower knower, IDataMerger merger, IDataFilterer dataFilterer, ITimeFilterer timeFilterer) : base(merger, dataFilterer, timeFilterer)
        {
            _knower = knower;
        }

        protected override async Task<IEnumerable<DataRange>> ProduceAttributeRanges(Guid series, IEnumerable<TimeRange> timeRanges)
        {
            return await Task.WhenAll(timeRanges.Select(timeRange => GetAttributeRange(series, timeRange)));
        }

        private async Task<DataRange> GetAttributeRange(Guid guid, TimeRange timeRange)
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
