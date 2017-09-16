using System;
using System.Collections.Generic;
using System.Linq;
using Deepflow.Platform.Abstractions.Series;
using Deepflow.Platform.Series;
using Deepflow.Platform.Series.Providers;

namespace Deepflow.Platform.Sources.FakeSource.Data
{
    public class DataGenerator : IDataGenerator
    {
        private readonly EnhancedRandomDataGenerator _generator = new EnhancedRandomDataGenerator();
        private readonly DataFilterer _filterer = new DataFilterer();
        private readonly DataJoiner _joiner = new DataJoiner();

        public DataRange GenerateData(string sourceName, TimeRange timeRange, int aggregationSeconds)
        {
            var chunkSeconds = aggregationSeconds * 1024;
            var quantisedRange = timeRange.Quantise(chunkSeconds);
            var chunks = quantisedRange.Chop(chunkSeconds);

            var generated = chunks.Select(chunk => _generator.GenerateData(sourceName, chunk.MinSeconds, chunk.MaxSeconds, 500, 1500, 300, TimeSpan.FromSeconds(aggregationSeconds)));
            var joined = _joiner.JoinDataRangesToDataRanges(new List<DataRange>(), generated);
            return _filterer.FilterDataRange(joined.Single(), timeRange); 
        }
    }
}
