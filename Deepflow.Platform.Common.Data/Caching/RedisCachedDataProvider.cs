using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Deepflow.Platform.Abstractions.Series;
using Deepflow.Platform.Abstractions.Series.Extensions;
using StackExchange.Redis;

namespace Deepflow.Platform.Common.Data.Caching
{
    public class RedisCachedDataProvider : ICachedDataProvider
    {
        private readonly IRangeFilterer<TimeRange> _filterer;
        private readonly IDatabase _db;

        public RedisCachedDataProvider(IRangeFilterer<TimeRange> filterer)
        {
            _filterer = filterer;

            ConnectionMultiplexer redis = ConnectionMultiplexer.Connect("13.210.70.245,password=305bdaabb4c1881f2af5fdf68323a33d72ce87811c1663d503ac459ed7bd52dd");
            _db = redis.GetDatabase();
        }

        public async Task<IEnumerable<AggregatedDataRange>> GetData(Guid series, TimeRange timeRange, int aggregationSeconds)
        {
            var key = GetDataKey(series, aggregationSeconds);
            var dataTask = _db.SortedSetRangeByScoreAsync(key, timeRange.Min, timeRange.Max);
            var timeRangesTask = GetTimeRanges(series, timeRange, aggregationSeconds);
            var data = await dataTask;
            var timeRanges = await timeRangesTask;
            return ToDataRanges(timeRanges, data, timeRange, aggregationSeconds);
        }

        public async Task<IEnumerable<TimeRange>> GetTimeRanges(Guid series, TimeRange timeRange, int aggregationSeconds)
        {
            var key = GetTimeRangeKey(series, aggregationSeconds);
            var bytes = (byte[]) await _db.StringGetAsync(key);
            if (bytes == null)
            {
                return new List<TimeRange>();
            }
            var timeRanges = DeserialiseTimeRanges(bytes);
            return _filterer.FilterRanges(timeRanges, timeRange);
        }

        public async Task SaveTimeRanges(Guid series, IEnumerable<TimeRange> timeRanges, int aggregationSeconds)
        {
            var key = GetTimeRangeKey(series, aggregationSeconds);
            var bytes = SerialiseTimeRanges(timeRanges);
            await _db.StringSetAsync(key, bytes);
        }

        public async Task SaveData(Guid series, IEnumerable<AggregatedDataRange> dataRanges)
        {
            await Task.WhenAll(dataRanges.Select(x => SaveRange(series, x)));
        }

        private async Task SaveRange(Guid series, AggregatedDataRange dataRange)
        {
            var key = GetDataKey(series, dataRange.AggregationSeconds);
            var entries = dataRange.Data.GetData().Select(x => new SortedSetEntry($"{x.Time}:{x.Value}", x.Time)).ToArray();
            await _db.SortedSetAddAsync(key, entries);
        }

        private string GetDataKey(Guid series, int aggregationSeconds)
        {
            return $"{series}_Aggregated_{aggregationSeconds}_Data";
        }

        private string GetTimeRangeKey(Guid series, int aggregationSeconds)
        {
            return $"{series}_Aggregated_{aggregationSeconds}_TimeRange";
        }

        private static IEnumerable<TimeRange> DeserialiseTimeRanges(byte[] bytes)
        {
            using (var stream = new MemoryStream(bytes))
            {
                using (var reader = new BinaryReader(stream))
                {
                    var bytesLength = reader.ReadInt32();
                    for (int i = 0; i < bytesLength / 16; i++)
                    {
                        yield return new TimeRange(reader.ReadInt64(), reader.ReadInt64());
                    }
                }
            }
        }

        private static byte[] SerialiseTimeRanges(IEnumerable<TimeRange> timeRanges)
        {
            using (var stream = new MemoryStream())
            {
                using (var writer = new BinaryWriter(stream))
                {
                    var bytesLength = timeRanges.Count() * sizeof(long) * 2;
                    writer.Write(bytesLength);
                    foreach (var timeRange in timeRanges)
                    {
                        writer.Write(timeRange.Min);
                        writer.Write(timeRange.Max);
                    }
                }
                return stream.ToArray();
            }
        }

        private IEnumerable<AggregatedDataRange> ToDataRanges(IEnumerable<TimeRange> timeRanges, RedisValue[] data, TimeRange filterTimeRange, int aggregationSeconds)
        {
            var index = 0;
            foreach (var timeRange in timeRanges)
            {
                if (timeRange.Max <= filterTimeRange.Min || timeRange.Min >= filterTimeRange.Max)
                {
                    continue;
                }

                var thisTimeRange = new TimeRange(timeRange.Min, timeRange.Max);

                var rangeData = new List<double>();

                double time;
                double value;
                while (index < data.Length && Parse(data[index], out time, out value) && time <= timeRange.Min)
                {
                    index++;
                }

                while (index < data.Length && Parse(data[index], out time, out value) && time <= timeRange.Max)
                {
                    rangeData.Add(time);
                    rangeData.Add(value);
                    index += 2;
                }

                if (rangeData.Count > 0)
                {
                    yield return new AggregatedDataRange(thisTimeRange, rangeData, aggregationSeconds);
                }
            }
        }

        private bool Parse(RedisValue serialised, out double time, out double value)
        {
            value = 0;
            var parts = ((string) serialised).Split(':');
            return double.TryParse(parts[0], out time) && double.TryParse(parts[1], out value);
        }
    }
}
