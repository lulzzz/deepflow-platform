using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Deepflow.Platform.Abstractions.Series;
using Deepflow.Platform.Core.Tools;

namespace Deepflow.Platform.Series.Providers
{
    public class EnhancedRandomDataProvider : InMemoryDataProvider
    {
        private readonly ISeriesKnower _knower;
        private readonly ITimeFilterer _timeFilterer;
        private readonly IDataFilterer _dataFilterer;
        private readonly IDataMerger _merger;
        private readonly ConcurrentDictionary<Guid, ConcurrentDictionary<int, IEnumerable<DataRange>>> _rangesBySeries = new ConcurrentDictionary<Guid, ConcurrentDictionary<int, IEnumerable<DataRange>>>();
        private readonly ConcurrentDictionary<Guid, ReaderWriterLockSlim> _seriesLocks = new ConcurrentDictionary<Guid, ReaderWriterLockSlim>();
        private readonly RandomDataGenerator _generator = new RandomDataGenerator();
        private readonly Aggregator _aggregator = new Aggregator();

        public EnhancedRandomDataProvider(ISeriesKnower knower, ITimeFilterer timeFilterer, IDataFilterer dataFilterer, IDataMerger merger) : base(merger, dataFilterer, timeFilterer)
        {
            _knower = knower;
            _timeFilterer = timeFilterer;
            _dataFilterer = dataFilterer;
            _merger = merger;
        }

        protected override async Task<IEnumerable<DataRange>> ProduceAttributeRanges(Guid series, IEnumerable<TimeRange> timeRanges)
        {
            return await Task.WhenAll(timeRanges.Select(timeRange => GetAttributeRange(series, timeRange)));
        }

        private async Task<DataRange> GetAttributeRange(Guid guid, TimeRange timeRange)
        {
            var series = await _knower.GetAttributeSeries(guid);

            var rangesByAggregation = _rangesBySeries.GetOrAdd(guid, g => new ConcurrentDictionary<int, IEnumerable<DataRange>>());
            var ranges = rangesByAggregation.GetOrAdd(series.AggregationSeconds, new List<DataRange>());
            var missingRanges = _timeFilterer.SubtractTimeRangesFromRange(timeRange, ranges.Select(range => range.TimeRange));
            
            if (missingRanges.Any())
            {
                var seriesLock = _seriesLocks.GetOrAdd(guid, g => new ReaderWriterLockSlim());
                var newRawRanges = missingRanges.Select(x => _generator.GenerateData($"{series.Entity}:{series.Attribute}", timeRange.MinSeconds, timeRange.MaxSeconds, 500, 1500, 300, TimeSpan.FromMinutes(5)));
                seriesLock.EnterWriteLock();
                try
                {
                    var rawRanges = rangesByAggregation.GetOrAdd(0, new List<DataRange>());
                    rawRanges = _merger.MergeDataRangesWithRanges(rawRanges, newRawRanges);
                    rangesByAggregation.AddOrUpdate(0, rawRanges, (i, old) => rawRanges);
                    var aggregatedRange = _aggregator.Aggregate(rawRanges, timeRange.MinSeconds, timeRange.MaxSeconds, series.AggregationSeconds);

                    var existingRanges = rangesByAggregation.GetOrAdd(series.AggregationSeconds, new List<DataRange>());
                    var aggregatedRanges = _merger.MergeDataRangeWithRanges(existingRanges, aggregatedRange);
                    rangesByAggregation.AddOrUpdate(series.AggregationSeconds, aggregatedRanges, (i, old) => aggregatedRanges);
                    ranges = aggregatedRanges;
                }
                finally
                {
                    seriesLock.ExitWriteLock();
                }
            }

            return _dataFilterer.FilterDataRanges(ranges, timeRange).Single();
        }
    }

    public class RandomDataGenerator
    {
        private static readonly SHA1 Sha1 = SHA1.Create();

        public DataRange GenerateData(string name, long startTimeSeconds, long endTimeSeconds, double minValue, double maxValue, double maxVariance, TimeSpan maxTimespan)
        {
            var startTimeHash = Hash($"${name}_${startTimeSeconds}_${minValue}_${maxValue}");
            var endTimeHash = Hash($"${name}_${endTimeSeconds}_${minValue}_${maxValue}");
            var fullHash = Hash($"{name}_${startTimeSeconds}_${endTimeSeconds}_${maxVariance}_${maxTimespan}");

            Random random = new Random(fullHash);
            var valueRange = maxValue - minValue;
            var startValue = new Random(startTimeHash).NextDouble() * valueRange + minValue;
            var endValue = new Random(endTimeHash).NextDouble() * valueRange + minValue;
            var startDatum = new Datum { Time = startTimeSeconds, Value = startValue };
            var endDatum = new Datum { Time = endTimeSeconds, Value = endValue };

            List<Datum> data = new List<Datum>();
            data.Add(startDatum);
            SplitValueRecursive(startDatum, endDatum, maxVariance, data, maxTimespan, random);
            data.Add(endDatum);

            return new DataRange(startTimeSeconds, endTimeSeconds, data);
        }

        private void SplitValueRecursive(Datum start, Datum end, double maxVariance, List<Datum> data, TimeSpan maxTimespan, Random random)
        {
            var range = end.Time - start.Time;
            var totalTimespans = range / maxTimespan.TotalSeconds;
            var midTime = start.Time + Math.Floor(totalTimespans / 2) * maxTimespan.TotalSeconds;

            if (totalTimespans <= 1)
            {
                return;
            }

            var midValue = start.Value + (end.Value - start.Value) / 2;
            var randomisedMidValue = midValue + random.NextDouble() * maxVariance * 2 - maxVariance;

            var midDatum = new Datum { Time = midTime, Value = randomisedMidValue };

            SplitValueRecursive(start, midDatum, maxVariance / 1.5, data, maxTimespan, random);
            data.Add(midDatum);
            SplitValueRecursive(midDatum, end, maxVariance / 1.5, data, maxTimespan, random);
        }

        public static int Hash(string src)
        {
            byte[] stringbytes = Encoding.UTF8.GetBytes(src);
            byte[] hashedBytes = Sha1.ComputeHash(stringbytes);
            Array.Resize(ref hashedBytes, 4);
            return BitConverter.ToInt32(hashedBytes, 0);
        }
    }

    public class Aggregator
    {
        public DataRange Aggregate(IEnumerable<DataRange> inData, long startTimeSeconds, long endTimeSeconds, float sampleIntervalSeconds)
        {
            var tickEnumerator = inData.GetData().GetEnumerator();
            tickEnumerator.MoveNext();

            List<Datum> data = new List<Datum>();
            Datum tick = null;
            Datum nextTick = tickEnumerator.Current;
            var end = false;
            for (double time = startTimeSeconds; time < endTimeSeconds; time += sampleIntervalSeconds)
            {
                Datum newDatum = new Datum { Time = time };

                while (nextTick != null && nextTick.Time < time)
                {
                    tick = nextTick;

                    if (!tickEnumerator.MoveNext())
                    {
                        end = true;
                        break;
                    }

                    nextTick = tickEnumerator.Current;
                }

                if (tick != null)
                {
                    newDatum.Value = tick.Value;
                    data.Add(newDatum);
                }

                if (end)
                {
                    break;
                }
            }

            return new DataRange(startTimeSeconds, endTimeSeconds, data);
        }
    }
}
