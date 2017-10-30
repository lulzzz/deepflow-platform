using System;
using System.Collections.Generic;
using Deepflow.Platform.Abstractions.Series;

namespace Deepflow.Platform.Sources.FakeSource.Data
{
    public class RandomDataGenerator : IDataGenerator
    {
        private readonly IRangeFilterer<AggregatedDataRange> _filterer;
        private static readonly Random Random = new Random();

        public RandomDataGenerator(IRangeFilterer<AggregatedDataRange> filterer)
        {
            _filterer = filterer;
        }

        public RawDataRange GenerateRawPoint(string sourceName, int time, int aggregationSeconds)
        {
            return new RawDataRange(time, time, new List<double> { time, Random.NextDouble() });
        }

        public RawDataRange GenerateRawRange(string sourceName, TimeRange timeRange, int aggregationSeconds)
        {
            var data = new List<double>();

            for (var time = timeRange.Min; time <= timeRange.Max; time += aggregationSeconds)
            {
                data.Add(time);
                data.Add(Random.NextDouble());
            }

            return new RawDataRange(timeRange, data);
        }

        public AggregatedDataRange GenerateRange(string sourceName, TimeRange timeRange, int aggregationSeconds)
        {
            var data = new List<double>();

            var quantised = timeRange.Quantise(aggregationSeconds);
            
            for (var time = quantised.Min + aggregationSeconds; time <= quantised.Max; time += aggregationSeconds)
            {
                data.Add(time);
                data.Add(Random.NextDouble());
            }

            return _filterer.FilterDataRange(new AggregatedDataRange(quantised, data, aggregationSeconds), timeRange);
        }
    }
}
