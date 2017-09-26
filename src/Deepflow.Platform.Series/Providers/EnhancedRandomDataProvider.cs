/*
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
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
        private readonly IRangeFilterer _dataFilterer;
        private readonly IRangeMerger _merger;
        private readonly ConcurrentDictionary<Guid, ConcurrentDictionary<int, IEnumerable<RawDataRange>>> _rangesBySeries = new ConcurrentDictionary<Guid, ConcurrentDictionary<int, IEnumerable<RawDataRange>>>();
        private readonly ConcurrentDictionary<Guid, ReaderWriterLockSlim> _seriesLocks = new ConcurrentDictionary<Guid, ReaderWriterLockSlim>();
        private readonly EnhancedRandomDataGenerator _generator = new EnhancedRandomDataGenerator();
        private readonly Aggregator _aggregator = new Aggregator();

        public EnhancedRandomDataProvider(ISeriesKnower knower, ITimeFilterer timeFilterer, IRangeFilterer dataFilterer, IRangeMerger merger) : base(merger, dataFilterer, timeFilterer)
        {
            _knower = knower;
            _timeFilterer = timeFilterer;
            _dataFilterer = dataFilterer;
            _merger = merger;
        }

        protected override async Task<IEnumerable<RawDataRange>> ProduceAttributeRanges(Guid series, IEnumerable<TimeRange> timeRanges)
        {
            return await Task.WhenAll(timeRanges.Select(timeRange => GetAttributeRange(series, timeRange)));
        }

        private async Task<RawDataRange> GetAttributeRange(Guid guid, TimeRange timeRange)
        {
            var series = await _knower.GetAttributeSeries(guid);

            var rangesByAggregation = _rangesBySeries.GetOrAdd(guid, g => new ConcurrentDictionary<int, IEnumerable<RawDataRange>>());
            var ranges = rangesByAggregation.GetOrAdd(series.AggregationSeconds, new List<RawDataRange>());
            var missingRanges = _timeFilterer.SubtractTimeRangesFromRange(timeRange, ranges.Select(range => range.TimeRange));
            
            if (missingRanges.Any())
            {
                var seriesLock = _seriesLocks.GetOrAdd(guid, g => new ReaderWriterLockSlim());
                var newRawRanges = missingRanges.Select(x => _generator.GenerateData($"{series.Entity}:{series.Attribute}", timeRange.Min, timeRange.Max, 500, 1500, 300, TimeSpan.FromMinutes(5)));
                seriesLock.EnterWriteLock();
                try
                {
                    var rawRanges = rangesByAggregation.GetOrAdd(0, new List<RawDataRange>());
                    rawRanges = _merger.MergeRangesWithRanges(rawRanges, newRawRanges);
                    rangesByAggregation.AddOrUpdate(0, rawRanges, (i, old) => rawRanges);
                    var aggregatedRange = _aggregator.Aggregate(rawRanges, timeRange.Min, timeRange.Max, series.AggregationSeconds);

                    var existingRanges = rangesByAggregation.GetOrAdd(series.AggregationSeconds, new List<RawDataRange>());
                    var aggregatedRanges = _merger.MergeRangeWithRanges(existingRanges, aggregatedRange);
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

    public class Aggregator
    {
        public RawDataRange Aggregate(IEnumerable<RawDataRange> inData, long startTimeSeconds, long endTimeSeconds, float sampleIntervalSeconds)
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

            return new RawDataRange(startTimeSeconds, endTimeSeconds, data);
        }
    }
}
*/
