using System.Collections.Generic;
using System.Linq;
using Deepflow.Platform.Abstractions.Series;
using Deepflow.Platform.Core.Tools;

namespace Deepflow.Platform.Series
{
    public class DataAggregator : IDataAggregator
    {
        public AggregatedDataRange Aggregate(DataRange dataRange, int aggregationSeconds)
        {
            var quantisedRange = dataRange.TimeRange.Quantise(aggregationSeconds);

            List<double> aggregated = new List<double>();
            var dataEnumerator = dataRange.GetData();
            for (var timeSeconds = quantisedRange.MinSeconds + aggregationSeconds; timeSeconds <= quantisedRange.MaxSeconds; timeSeconds += aggregationSeconds)
            {
                var minTimeExclusive = timeSeconds - aggregationSeconds;
                var maxTimeInclusive = timeSeconds;
                var chunkData = dataEnumerator.SkipWhile(x => x.Time <= minTimeExclusive).TakeWhile(x => x.Time <= maxTimeInclusive);
                var average = chunkData.Average(x => x.Value);
                aggregated.Add(timeSeconds);
                aggregated.Add(average);
            }

            return new AggregatedDataRange(quantisedRange, aggregated, aggregationSeconds);
        }

        public IEnumerable<AggregatedDataRange> Aggregate(DataRange dataRange, IEnumerable<int> aggregationsSeconds)
        {
            return aggregationsSeconds.Select(aggregationSeconds => Aggregate(dataRange, aggregationSeconds));
        }
    }
}
