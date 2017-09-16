using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Deepflow.Platform.Abstractions.Series;
using Deepflow.Platform.Core.Tools;
using Microsoft.Extensions.Logging;

namespace Deepflow.Platform.Series
{
    public class DataAggregator : IDataAggregator
    {
        private readonly ILogger<DataAggregator> _logger;

        public DataAggregator(ILogger<DataAggregator> logger)
        {
            _logger = logger;
        }

        public AggregatedDataRange Aggregate(DataRange dataRange, int aggregationSeconds)
        {
            var stopwatch = Stopwatch.StartNew();
            var quantisedRange = dataRange.TimeRange.Quantise(aggregationSeconds);
            var maxExpectedPoints = (int) Math.Ceiling((double)(quantisedRange.MaxSeconds - quantisedRange.MinSeconds) / aggregationSeconds);

            int totalCount = 0;
            List<double> aggregated = new List<double>(maxExpectedPoints);
            int index = 0;
            var data = dataRange.Data;
            var numPoints = data.Count / 2;
            for (var timeSeconds = quantisedRange.MinSeconds + aggregationSeconds; timeSeconds <= quantisedRange.MaxSeconds; timeSeconds += aggregationSeconds)
            {
                var minTimeExclusive = timeSeconds - aggregationSeconds;
                var maxTimeInclusive = timeSeconds;

                while (index < numPoints && data[index * 2] <= minTimeExclusive)
                {
                    index++;
                }

                double chunkSum = 0.0;
                int chunkCount = 0;
                while (index < numPoints && data[index * 2] <= maxTimeInclusive)
                {
                    chunkSum += data[index * 2 + 1];
                    chunkCount++;
                    index++;
                }

                if (chunkCount > 0)
                {
                    aggregated.Add(timeSeconds);
                    aggregated.Add(chunkSum / chunkCount);
                }
                
                totalCount += chunkCount;
            }
            
            _logger.LogDebug($"Aggregated {totalCount} points for {aggregationSeconds} second aggregation in {stopwatch.ElapsedMilliseconds} ms");
            return new AggregatedDataRange(quantisedRange, aggregated, aggregationSeconds);
        }

        public IEnumerable<AggregatedDataRange> Aggregate(DataRange dataRange, IEnumerable<int> aggregationsSeconds)
        {
            return aggregationsSeconds.Select(aggregationSeconds => Aggregate(dataRange, aggregationSeconds));
        }
    }
}
