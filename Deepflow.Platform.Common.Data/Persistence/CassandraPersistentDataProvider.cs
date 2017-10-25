using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Cassandra;
using Deepflow.Common.Model.Model;
using Deepflow.Platform.Abstractions.Series;
using Deepflow.Platform.Abstractions.Series.Extensions;
using Deepflow.Platform.Common.Data.Configuration;
using Deepflow.Platform.Core.Tools;
using Newtonsoft.Json;

namespace Deepflow.Platform.Common.Data.Persistence
{
    public class CassandraPersistentDataProvider : IPersistentDataProvider
    {
        private readonly IRangeFilterer<TimeRange> _filterer;
        private readonly IModelProvider _model;
        private readonly ISession _session;

        public CassandraPersistentDataProvider(CassandraConfiguration configuration, IRangeFilterer<TimeRange> filterer, IModelProvider model)
        {
            _filterer = filterer;
            _model = model;
            var cluster = Cluster.Builder()
                .AddContactPoints(configuration.Address)
                .WithCredentials(configuration.Username, configuration.Password)
                .WithDefaultKeyspace(configuration.Keyspace)
                .WithQueryTimeout(configuration.QueryTimeout)
                .Build();
            _session = cluster.Connect();
        }

        public async Task<IEnumerable<AggregatedDataRange>> GetData(Guid series, TimeRange timeRange)
        {
            var query = $"SELECT Time, Value FROM deepflowdata WHERE Guid = {series} AND Time >= {timeRange.Min} AND Time < {timeRange.Max};";
            var aggregationTask = _model.ResolveAggregationForSeries(series);
            var timeRangesTask = GetAllTimeRanges(series);
            var rowSet = await _session.ExecuteAsync(new SimpleStatement(query)).ConfigureAwait(false);
            var alTimeRanges = await timeRangesTask.ConfigureAwait(false);
            var timeRanges = _filterer.FilterRanges(alTimeRanges, timeRange);
            var aggregation = await aggregationTask.ConfigureAwait(false);
            return ToDataRanges(timeRanges, rowSet.Reverse().ToList(), timeRange, aggregation);
        }

        public async Task SaveData(IEnumerable<(Guid series, IEnumerable<AggregatedDataRange> dataRanges)> seriesData)
        {
            var preparedStatement = _session.Prepare($"INSERT INTO deepflowdata (Guid, Time, Value) VALUES (?, ?, ?)");
            await Task.WhenAll(Enumerate(seriesData).Batch(10000).Select(async batch =>
            {
                var batchStatement = new BatchStatement();
                foreach (var datum in batch)
                {
                    var statement = preparedStatement.Bind(datum.series, datum.time, datum.value);
                    batchStatement.Add(statement);
                }

                await _session.ExecuteAsync(batchStatement);
            }));
        }

        private IEnumerable<(Guid series, int time, double value)> Enumerate(IEnumerable<(Guid series, IEnumerable<AggregatedDataRange> dataRanges)> seriesData)
        {
            var datum = new ValueTuple<Guid, int, double>();
            foreach ((Guid series, IEnumerable<AggregatedDataRange> dataRanges) series in seriesData)
            {
                datum.Item1 = series.series;
                foreach (var inDatum in series.dataRanges.GetData())
                {
                    datum.Item2 = (int)inDatum.Time;
                    datum.Item3 = inDatum.Value;
                    yield return datum;
                }
            }
        }

        public async Task<IEnumerable<TimeRange>> GetTimeRanges(Guid series, TimeRange timeRange)
        {
            var all = await GetAllTimeRanges(series);
            return _filterer.FilterRanges(all, timeRange);
        }

        public async Task<IEnumerable<TimeRange>> GetAllTimeRanges(Guid series)
        {
            var query = $"SELECT Ranges FROM deepflowtimeranges WHERE Guid = {series};";
            var rowSet = await _session.ExecuteAsync(new SimpleStatement(query));
            var rows = rowSet.ToList();
            var row = rows.SingleOrDefault();
            if (row == null)
            {
                return new List<TimeRange>();
            }
            var ranges = row.GetValue<string>(0);
            return JsonConvert.DeserializeObject<IEnumerable<TimeRange>>(ranges);
        }

        public async Task SaveTimeRanges(Guid series, IEnumerable<TimeRange> timeRanges)
        {
            await _session.ExecuteAsync(new SimpleStatement("INSERT INTO deepflowtimeranges (Guid, Ranges) VALUES (?, ?)", series, JsonConvert.SerializeObject(timeRanges)));
        }

        private IEnumerable<AggregatedDataRange> ToDataRanges(IEnumerable<TimeRange> timeRanges, List<Row> data, TimeRange filterTimeRange, int aggregationSeconds)
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

                int time = 0;

                while (index < data.Count && data[index].GetValue<int>(0) <= timeRange.Min)
                {
                    index++;
                }

                while (index < data.Count && (time = data[index].GetValue<int>(0)) <= timeRange.Max)
                {
                    var record = data[index];
                    rangeData.Add(time);
                    rangeData.Add(record.GetValue<double>(1));
                    index += 1;
                }

                if (rangeData.Count > 0)
                {
                    yield return new AggregatedDataRange(thisTimeRange, rangeData, aggregationSeconds);
                }
            }
        }
    }
}
