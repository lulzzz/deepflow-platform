using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Deepflow.Platform.Abstractions.Series;
using Deepflow.Platform.Abstractions.Series.Extensions;
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
            if (!timeRange.IsQuantisedTo(aggregationSeconds))
            {
                throw new Exception($"Cannot aggregate to time range {timeRange} because it is not quantised to {aggregationSeconds}");
            }

            ValidateDataRangesInOrder(dataRanges);

            var stopwatch = Stopwatch.StartNew();
            var quantisedRange = timeRange.Quantise(aggregationSeconds);

            var dataEnumerator = dataRanges.GetData().GetEnumerator();
            if (!dataEnumerator.MoveNext())
            {
                return new List<AggregatedDataRange>();
            }

            int inCount = 0;
            int outCount = 0;
            List<AggregatedDataRange> aggregatedRanges = new List<AggregatedDataRange>();
            List<double> aggregatedData = null;
            for (var timeSeconds = quantisedRange.Min + aggregationSeconds; timeSeconds <= quantisedRange.Max; timeSeconds += aggregationSeconds)
            {
                var minTimeExclusive = timeSeconds - aggregationSeconds;
                var maxTimeInclusive = timeSeconds;
                
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
                    inCount++;
                    if (!dataEnumerator.MoveNext())
                    {
                        moreData = false;
                        break;
                    }
                }

                if (chunkCount > 0)
                {
                    if (aggregatedData == null)
                    {
                        aggregatedData = new List<double>();
                    }
                    aggregatedData.Add(timeSeconds);
                    aggregatedData.Add(chunkSum / chunkCount);
                    outCount++;
                }
                else
                {
                    if (aggregatedData != null)
                    {
                        var minTime = (long)aggregatedData[0] - aggregationSeconds;
                        var maxTime = (long)aggregatedData[aggregatedData.Count - 2];
                        aggregatedRanges.Add(new AggregatedDataRange(minTime, maxTime, aggregatedData, aggregationSeconds));
                        aggregatedData = null;
                    }
                }

                if (!moreData)
                {
                    break;
                }
            }

            if (aggregatedData != null)
            {
                var minTime = (long)aggregatedData[0] - aggregationSeconds;
                var maxTime = (long)aggregatedData[aggregatedData.Count - 2];
                aggregatedRanges.Add(new AggregatedDataRange(minTime, maxTime, aggregatedData, aggregationSeconds));
            }

            _logger.LogDebug($"Aggregated {inCount} points to {outCount} points at {aggregationSeconds} second aggregation in {stopwatch.ElapsedMilliseconds} ms");
            return aggregatedRanges;
        }

        public Dictionary<int, IEnumerable<AggregatedDataRange>> Aggregate(IEnumerable<AggregatedDataRange> dataRanges, TimeRange timeRange, IEnumerable<int> aggregationSeconds)
        {
            return aggregationSeconds.ToDictionary(x => x, x => Aggregate(dataRanges, timeRange, x));
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

                if (dataRange.TimeRange.Min < previous.Min)
                {
                    throw new Exception($"Cannot aggregate out of order time ranges. {previous} should be before {dataRange.TimeRange}");
                }
            }
        }
    }
}

/*
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

        public IEnumerable<AggregatedDataRange> Aggregate(IEnumerable<AggregatedDataRange> dataRanges, TimeRange timeRange, int sourceAggregationSeconds, int aggregationSeconds)
        {
            if (!timeRange.IsQuantisedTo(aggregationSeconds))
            {
                throw new Exception($"Cannot aggregate to time range {timeRange} because it is not quantised to {aggregationSeconds}");
            }

            ValidateDataRanges(dataRanges, sourceAggregationSeconds);

            var stopwatch = Stopwatch.StartNew();
            var quantisedRange = timeRange.Quantise(aggregationSeconds);

            var rangeEnumerator = dataRanges.GetEnumerator();

            //var dataEnumerator = dataRanges.GetData().GetEnumerator();
            if (!rangeEnumerator.MoveNext())
            {
                return new List<AggregatedDataRange>();
            }

            int totalCount = 0;
            List<AggregatedDataRange> aggregatedRanges = new List<AggregatedDataRange>();
            List<double> aggregatedData = null;
            int maxPointsPerChunk = aggregationSeconds / sourceAggregationSeconds;
            double[] recentValues = new double[maxPointsPerChunk];
            int recentValueIndex = 0;
            int recentTotal = 0;
            int firstPointIndex = 0;
            bool foundFirstPoint = false;
            double previousValue = 0;
            double rollingValue = 0;
            double fraction = 1.0 / maxPointsPerChunk;
            IEnumerator<Datum> dataEnumerator = rangeEnumerator.Current.GetData().GetEnumerator();
            AggregatedDataRange currentRange = rangeEnumerator.Current;

            //var currentRange = rangeEnumerator.Current;

            //long time;
            long pointTime;
            var moreData = true;
            int chunkCount = 0;

            var index = 0;
            for (var time = quantisedRange.Min + sourceAggregationSeconds; time <= quantisedRange.Max; time += sourceAggregationSeconds)
            {
                /*while (rangeEnumerator.Current.TimeRange.Max > time)
                {
                    if (!rangeEnumerator.MoveNext())
                    {
                        moreData = false;
                        break;
                    }
                    else
                    {
                        dataEnumerator.Dispose();
                        dataEnumerator = rangeEnumerator.Current.GetData().GetEnumerator();
                    }
                }#1#

                recentValueIndex = index % maxPointsPerChunk;
                while ((pointTime = (long)dataEnumerator.Current.Time) < time)
                {
                    if (!dataEnumerator.MoveNext())
                    {
                        if (!rangeEnumerator.MoveNext())
                        {
                            moreData = false;
                            break;
                        }
                        dataEnumerator = rangeEnumerator.Current.GetData().GetEnumerator();
                        if (!dataEnumerator.MoveNext())
                        {
                            moreData = false;
                            break;
                        }
                    }
                }

                rollingValue -= recentValues[(maxPointsPerChunk + recentValueIndex) % maxPointsPerChunk];

                if (moreData && pointTime == time)
                {
                    var value = dataEnumerator.Current.Value;
                    recentValues[recentValueIndex] = value;
                    previousValue = value;
                    chunkCount++;
                    totalCount++;

                    if (!foundFirstPoint)
                    {
                        foundFirstPoint = true;
                        firstPointIndex = (int)index;
                    }
                }
                else
                {
                    recentValues[recentValueIndex] = previousValue;
                }
                
                rollingValue += previousValue;

                var isAggregationInterval = index > 0 && recentValueIndex == maxPointsPerChunk - 1;
                if (isAggregationInterval)
                {
                    double numPointsThisInterval = Math.Min(index - firstPointIndex + 1, maxPointsPerChunk);
                    if (chunkCount > 0)
                    {
                        var value = rollingValue / numPointsThisInterval;
                        if (chunkCount > 0)
                        {
                            if (aggregatedData == null)
                            {
                                aggregatedData = new List<double>();
                            }
                            aggregatedData.Add(time);
                            aggregatedData.Add(value);
                        }
                        else
                        {
                            if (aggregatedData != null)
                            {
                                var minTime = (long)aggregatedData[0];
                                var maxTime = (long)aggregatedData[aggregatedData.Count - 2];
                                aggregatedRanges.Add(new AggregatedDataRange(minTime - aggregationSeconds, maxTime, aggregatedData, aggregationSeconds));
                                aggregatedData = null;
                            }
                        }
                        chunkCount = 0;
                    }
                }

                index++;
            }

            if (aggregatedData != null)
            {
                var minTime = (long)aggregatedData[0];
                var maxTime = (long)aggregatedData[aggregatedData.Count - 2];
                aggregatedRanges.Add(new AggregatedDataRange(minTime - aggregationSeconds, maxTime, aggregatedData, aggregationSeconds));
            }
            
            dataEnumerator.Dispose();

            /*for (var chunkIndex = 0; chunkIndex < maxPointsPerChunk; chunkIndex++)
        {#1#
            //time = minTimeExclusive + chunkIndex * sourceAggregationSeconds + sourceAggregationSeconds;
            /*while ((pointTime = (long)dataEnumerator.Current.Time) <= time)
            {
                if (!dataEnumerator.MoveNext())
                {
                    moreData = false;
                    break;
                }
            }

            if (pointTime == time)
            {
                var value = dataEnumerator.Current.Value / maxPointsPerChunk;
                recentValues[recentValueIndex] = previousValue;
                previousValue = value;
                chunkCount++;
            }
            else
            {
                recentValues[recentValueIndex] = previousValue;
            }#1#
            /*while ((time = (long) dataEnumerator.Current.Time) <= maxTimeInclusive)
            {



                if (!dataEnumerator.MoveNext())
                {
                    moreData = false;
                    break;
                }
            }#1#
            /*if (!moreData)
            {
                break;
            }#1#
            //}

            /*for (var timeSeconds = quantisedRange.Min + aggregationSeconds; timeSeconds <= quantisedRange.Max; timeSeconds += aggregationSeconds)
            {
                var minTimeExclusive = timeSeconds - aggregationSeconds;
                var maxTimeInclusive = timeSeconds;

                if (!dataEnumerator.MoveNext())
                {
                    break;
                }#1#



            /*if (!moreData)
            {
                break;
            }#1#



            /*while ((time = (long) dataEnumerator.Current.Time) <= maxTimeInclusive)
                {
                    var index = time % maxPointsPerChunk;
                    while (recentValueIndex != index)
                    {
                        recentValues[recentValueIndex] = previousValue;
                        recentValueIndex = (recentValueIndex + 1) % maxPointsPerChunk;
                        if (recentTotal > maxPointsPerChunk)
                        {
                            rollingValue -= (maxPointsPerChunk + recentValueIndex) % maxPointsPerChunk;
                        }
                        rollingValue += previousValue;
                        recentTotal++;
                    }

                    var value = dataEnumerator.Current.Value / maxPointsPerChunk;
                    recentValues[recentValueIndex] = previousValue;
                    previousValue = value;
                    chunkCount++;

                    if (!dataEnumerator.MoveNext())
                    {
                        moreData = false;
                        break;
                    }
                }

                /*var startIndex = Math.Min(recentTotal, recentValueIndex - maxPointsPerChunk);
                double aggregatedValue = 0.0;
                for (int i = startIndex; i != recentValueIndex; i = (i + 1) % maxPointsPerChunk)
                {
                    aggregatedValue += recentValues[i] * fraction;
                }#2#
                
               
                
                if (!moreData)
                {
                    break;
                }

                totalCount += chunkCount;
            //}

            if (aggregatedData != null)
            {
                var minTime = (long) aggregatedData[0];
                var maxTime = (long) aggregatedData[aggregatedData.Count - 2];
                aggregatedRanges.Add(new AggregatedDataRange(minTime - aggregationSeconds, maxTime, aggregatedData, aggregationSeconds));
            }#1#

            _logger.LogDebug($"Aggregated {totalCount} points for {aggregationSeconds} second aggregation in {stopwatch.ElapsedMilliseconds} ms");
            return aggregatedRanges;
        }

        public IEnumerable<AggregatedDataRange> Aggregate(IEnumerable<AggregatedDataRange> dataRanges, TimeRange timeRange, int sourceAggregationSeconds, IEnumerable<int> aggregationsSeconds)
        {
            return aggregationsSeconds.SelectMany(aggregation => Aggregate(dataRanges, timeRange, sourceAggregationSeconds, aggregation));
        }

        private void ValidateDataRanges(IEnumerable<AggregatedDataRange> dataRanges, int sourceAggregationSeconds)
        {
            if (!dataRanges.Any())
            {
                return;
            }

            TimeRange previous = null;

            foreach (var dataRange in dataRanges)
            {
                if (dataRange.AggregationSeconds != sourceAggregationSeconds)
                {
                    throw new Exception($"Cannot aggregate data range with aggregation of {dataRange.AggregationSeconds} that should match {sourceAggregationSeconds}");
                }

                if (previous == null)
                {
                    previous = dataRange.TimeRange;
                    continue;
                }

                if (dataRange.TimeRange.Min < previous.Min)
                {
                    throw new Exception($"Cannot aggregate out of order time ranges. {previous} should be before {dataRange.TimeRange}");
                }
            }
        }
    }
}
*/
