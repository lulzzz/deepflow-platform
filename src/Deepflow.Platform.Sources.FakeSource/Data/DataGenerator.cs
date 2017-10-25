using System;
using System.Collections.Generic;
using System.Linq;
using Deepflow.Platform.Abstractions.Series;
using Deepflow.Platform.Abstractions.Series.Extensions;
using Deepflow.Platform.Series;
using Deepflow.Platform.Series.Providers;
using Microsoft.Extensions.Logging;
using Orleans.Runtime;

namespace Deepflow.Platform.Sources.FakeSource.Data
{
    public class DataGenerator : IDataGenerator
    {
        private readonly ILogger<RangeJoiner<RawDataRange>> _logger;

        private readonly QuantisedRandomDataGenerator _generator = new QuantisedRandomDataGenerator();
        private readonly RangeFilterer<RawDataRange> _filterer = new RangeFilterer<RawDataRange>(new RawDataRangeCreator(), new RawDataRangeFilteringPolicy(), new RawDataRangeAccessor());
        private readonly RangeJoiner<RawDataRange> _joiner;
        private const int _interval = 3000000;

        public DataGenerator(ILogger<RangeJoiner<RawDataRange>> logger)
        {
            _logger = logger;
            _joiner = new RangeJoiner<RawDataRange>(new RawDataRangeCreator(), new RawDataRangeAccessor(), _logger);
        }

        public RawDataRange GenerateRawPoint(string sourceName, int time, int aggregationSeconds)
        {
            if (_interval % aggregationSeconds != 0)
            {
                throw new Exception("Aggregation seconds doesnt match interval");
            }

            var range = _generator.GenerateData(sourceName, new TimeRange(time - aggregationSeconds, time), 500, 1500, 300, aggregationSeconds, _interval);
            var timeIndex = range.Data.IndexOf(time);
            return new RawDataRange(time, time, new List<double> { time, range.Data[timeIndex + 1] });
        }

        public RawDataRange GenerateRawRange(string sourceName, TimeRange timeRange, int aggregationSeconds)
        {
            if (_interval % aggregationSeconds != 0)
            {
                throw new Exception("Aggregation seconds doesnt match interval");
            }

            var range = _generator.GenerateData(sourceName, timeRange, 500, 1500, 300, aggregationSeconds, _interval);
            return new RawDataRange(timeRange, range.Data);
        }

        public AggregatedDataRange GenerateRange(string sourceName, TimeRange timeRange, int aggregationSeconds)
        {
            if (_interval % aggregationSeconds != 0)
            {
                throw new Exception("Aggregation seconds doesnt match interval");
            }

            return _generator.GenerateData(sourceName, timeRange, 500, 1500, 300, aggregationSeconds, _interval);
        }

        /*public RawDataRange GenerateRange(string sourceName, TimeRange timeRange, int aggregationSeconds)
        {
            //var chunkSeconds = aggregationSeconds * 8;
            //var quantisedRange = timeRange.Quantise(chunkSeconds);
            //var chunks = quantisedRange.Chop(chunkSeconds);

            _generator.GenerateData(sourceName, chunk.Min, chunk.Max, 500, 1500, 300, TimeSpan.FromSeconds(aggregationSeconds))

            //var generated = chunks.Select(chunk => _generator.GenerateData(sourceName, chunk.Min, chunk.Max, 500, 1500, 300, TimeSpan.FromSeconds(aggregationSeconds)));
            //var joined = _joiner.JoinDataRangesToDataRanges(new List<RawDataRange>(), generated).ToList();
            /*if (joined.Any(x => x.Data.GetData().Any(y => x.TimeRange.Min == y.Time)))
            {
                throw new Exception();
            }#1#
            //return _filterer.FilterDataRange(joined.Single(), timeRange); 
        }*/
    }
}
