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

        public IEnumerable<AggregatedDataRange> Aggregate(IEnumerable<AggregatedDataRange> dataRanges, TimeRange timeRange, int aggregationSeconds)
        {
            throw new Exception("Implement time weighted averages");

            if (!timeRange.IsQuantisedTo(aggregationSeconds))
            {
                throw new Exception($"Cannot aggregate to time range {timeRange} because it is not quantised to {aggregationSeconds}");
            }

            ValidateDataRangesInOrder(dataRanges);

            var stopwatch = Stopwatch.StartNew();
            var quantisedRange = timeRange.Quantise(aggregationSeconds);

            var dataEnumerator = dataRanges.GetData().GetEnumerator();

            int totalCount = 0;
            List<AggregatedDataRange> aggregatedRanges = new List<AggregatedDataRange>();
            List<double> aggregatedData = null;
            for (var timeSeconds = quantisedRange.MinSeconds + aggregationSeconds; timeSeconds <= quantisedRange.MaxSeconds; timeSeconds += aggregationSeconds)
            {
                var minTimeExclusive = timeSeconds - aggregationSeconds;
                var maxTimeInclusive = timeSeconds;

                if (!dataEnumerator.MoveNext())
                {
                    break;
                }

                var moreData = true;
                while (dataEnumerator.Current.Time <= minTimeExclusive)
                {
                    if (!dataEnumerator.MoveNext())
                    {
                        moreData = false;
                        break;
                    }
                }

                if (!moreData)
                {
                    break;
                }

                double chunkSum = 0.0;
                int chunkCount = 0;
                while (dataEnumerator.Current.Time <= maxTimeInclusive)
                {
                    chunkSum += dataEnumerator.Current.Value;
                    chunkCount++;
                    if (!dataEnumerator.MoveNext())
                    {
                        moreData = false;
                        break;
                    }
                }

                if (!moreData)
                {
                    break;
                }

                if (chunkCount > 0)
                {
                    if (aggregatedData == null)
                    {
                        aggregatedData = new List<double>();
                    }
                    aggregatedData.Add(timeSeconds);
                    aggregatedData.Add(chunkSum / chunkCount);
                }
                else
                {
                    if (aggregatedData != null)
                    {
                        var minTime = (long) aggregatedData[0];
                        var maxTime = (long) aggregatedData[aggregatedData.Count - 2];
                        aggregatedRanges.Add(new AggregatedDataRange(minTime, maxTime, aggregationSeconds));
                        aggregatedData = null;
                    }
                }
                
                totalCount += chunkCount;
            }

            if (aggregatedData != null)
            {
                var minTime = (long) aggregatedData[0];
                var maxTime = (long) aggregatedData[aggregatedData.Count - 2];
                aggregatedRanges.Add(new AggregatedDataRange(minTime, maxTime, aggregationSeconds));
            }

            _logger.LogDebug($"Aggregated {totalCount} points for {aggregationSeconds} second aggregation in {stopwatch.ElapsedMilliseconds} ms");
            return aggregatedRanges;
        }

        public IEnumerable<AggregatedDataRange> Aggregate(IEnumerable<AggregatedDataRange> dataRanges, TimeRange timeRange, IEnumerable<int> aggregationsSeconds)
        {
            return aggregationsSeconds.SelectMany(aggregation => Aggregate(dataRanges, timeRange, aggregation));
        }

        private void ValidateDataRangesInOrder(IEnumerable<AggregatedDataRange> dataRanges)
        {
            if (!dataRanges.Any())
            {
                return;
            }

            TimeRange previous = null;

            foreach (var dataRange in dataRanges)
            {
                if (previous == null)
                {
                    previous = dataRange.TimeRange;
                    continue;
                }

                if (dataRange.TimeRange.MinSeconds < previous.MinSeconds)
                {
                    throw new Exception($"Cannot aggregate out of order time ranges. {previous} should be before {dataRange.TimeRange}");
                }
            }
        }
    }
}
