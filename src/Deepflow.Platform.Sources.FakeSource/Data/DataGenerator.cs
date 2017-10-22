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

        private readonly EnhancedRandomDataGenerator _generator = new EnhancedRandomDataGenerator();
        private readonly RangeFilterer<RawDataRange> _filterer = new RangeFilterer<RawDataRange>(new RawDataRangeCreator(), new RawDataRangeFilteringPolicy(), new RawDataRangeAccessor());
        private readonly RangeJoiner<RawDataRange> _joiner;

        public DataGenerator(ILogger<RangeJoiner<RawDataRange>> logger)
        {
            _logger = logger;
            _joiner = new RangeJoiner<RawDataRange>(new RawDataRangeCreator(), new RawDataRangeAccessor(), _logger);
        }

        public RawDataRange GenerateData(string sourceName, TimeRange timeRange, int aggregationSeconds)
        {
            var chunkSeconds = aggregationSeconds * 8;
            var quantisedRange = timeRange.Quantise(chunkSeconds);
            var chunks = quantisedRange.Chop(chunkSeconds);

            var generated = chunks.Select(chunk => _generator.GenerateData(sourceName, chunk.Min, chunk.Max, 500, 1500, 300, TimeSpan.FromSeconds(aggregationSeconds)));
            var joined = _joiner.JoinDataRangesToDataRanges(new List<RawDataRange>(), generated).ToList();
            /*if (joined.Any(x => x.Data.GetData().Any(y => x.TimeRange.Min == y.Time)))
            {
                throw new Exception();
            }*/
            return _filterer.FilterDataRange(joined.Single(), timeRange); 
        }
    }
}
