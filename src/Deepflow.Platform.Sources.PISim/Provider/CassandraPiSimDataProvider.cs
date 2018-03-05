using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Cassandra;
using Deepflow.Platform.Abstractions.Series;
using Deepflow.Platform.Agent.Provider;
using Deepflow.Platform.Common.Data.Configuration;
using Deepflow.Platform.Core.Tools;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace Deepflow.Platform.Sources.PISim.Provider
{
    public class CassandraPiSimDataProvider : IPiSimDataProvider, ISourceDataProvider
    {
        private readonly CassandraConfiguration _configuration;
        private readonly IDataAggregator _aggregator;
        private readonly ILogger<CassandraPiSimDataProvider> _logger;
        private readonly IRangeMerger<RawDataRange> _rawMerger;
        private readonly ISession _session;

        public CassandraPiSimDataProvider(CassandraConfiguration configuration, IDataAggregator aggregator, ILogger<CassandraPiSimDataProvider> logger, IRangeMerger<RawDataRange> rawMerger)
        {
            _configuration = configuration;
            _aggregator = aggregator;
            _logger = logger;
            _rawMerger = rawMerger;
            var cluster = Cluster.Builder()
                .AddContactPoints(configuration.Address)
                .WithDefaultKeyspace(configuration.Keyspace)
                .WithQueryTimeout(configuration.QueryTimeout)
                .Build();
            _session = cluster.Connect();
        }
        
        public async Task<AggregatedDataRange> GetAggregatedDataRange(string sourceName, int aggregationSeconds, [FromQuery] DateTime minTimeUtc, [FromQuery] DateTime maxTimeUtc)
        {
            var timeRange = new TimeRange(minTimeUtc, maxTimeUtc).Quantise(aggregationSeconds);
            var raw = await GetRawRange(sourceName, timeRange);
            if (raw.Data.Count == 0)
            {
                return new AggregatedDataRange(timeRange, new List<double>(), aggregationSeconds);
            }
            var range = new AggregatedDataRange(timeRange, raw.Data, 5, true);
            return _aggregator.Aggregate(new[] { range }, timeRange, new[] { aggregationSeconds })[300].FirstOrDefault();
        }
        
        public async Task<RawDataRange> GetRawDataRangeWithEdges(string sourceName, [FromQuery] DateTime minTimeUtc, [FromQuery] DateTime maxTimeUtc)
        {
            var timeRange = new TimeRange(minTimeUtc, maxTimeUtc);
            var rangeTask = GetRawRange(sourceName, timeRange);
            var earlierEdgeTask = GetEarlierEdge(sourceName, timeRange.Min);
            var laterEdgeTask = GetLaterEdge(sourceName, timeRange.Max);
            var range = await rangeTask;
            var earlierEdge = await earlierEdgeTask;
            var laterEdge = await laterEdgeTask;
            var ranges = _rawMerger.MergeRangesWithRange(range, new [] { earlierEdge, laterEdge });
            if (ranges.Count() > 1)
            {
                throw new Exception("Merged raw ranges and got more than one. " + JsonConvert.SerializeObject(earlierEdge) + JsonConvert.SerializeObject(range) + JsonConvert.SerializeObject(laterEdge) + JsonConvert.SerializeObject(ranges));
            }
            if (!ranges.Any())
            {
                throw new Exception("Merged raw ranges and got none");
            }
            return ranges.Single();
        }

        private async Task<RawDataRange> GetRawRange(string sourceName, TimeRange timeRange)
        {
            var query = $"SELECT timestamp, value FROM {_configuration.TableName} WHERE tag = '{sourceName}' AND timestamp >= {timeRange.Min} AND timestamp < {timeRange.Max};";
            var stopwatch = Stopwatch.StartNew();
            var data = await FetchRaw(query);
            _logger.LogDebug($"Query finished in {stopwatch.ElapsedMilliseconds}ms with {data?.Count / 2} points");
            return new RawDataRange(timeRange, data);
        }

        private async Task<RawDataRange> GetEarlierEdge(string sourceName, long earlierEdgeTime)
        {
            var query = $"SELECT timestamp, value FROM {_configuration.TableName} WHERE tag = '{sourceName}' AND timestamp <= {earlierEdgeTime} ORDER BY timestamp DESC LIMIT 1";
            var stopwatch = Stopwatch.StartNew();
            var data = await FetchRaw(query);
            _logger.LogDebug($"Earlier Edge Query finished in {stopwatch.ElapsedMilliseconds}ms with {data?.Count / 2} points");
            var earliestTime = data.Any() ? (long) data[0] : earlierEdgeTime;
            return new RawDataRange(new TimeRange(earliestTime, earlierEdgeTime), data);
        }

        private async Task<RawDataRange> GetLaterEdge(string sourceName, long laterEdgeTime)
        {
            var query = $"SELECT timestamp, value FROM {_configuration.TableName} WHERE tag = '{sourceName}' AND timestamp >= {laterEdgeTime} ORDER BY timestamp ASC LIMIT 1";
            var stopwatch = Stopwatch.StartNew();
            var data = await FetchRaw(query);
            _logger.LogDebug($"Later Edge Query finished in {stopwatch.ElapsedMilliseconds}ms with {data?.Count / 2} points");
            var latestTime = data.Any() ? (long)data[data.Count - 2] : laterEdgeTime;
            return new RawDataRange(new TimeRange(laterEdgeTime, latestTime), data);
        }

        private async Task<List<double>> FetchRaw(string query)
        {
            var rowSet = await _session.ExecuteAsync(new SimpleStatement(query)).ConfigureAwait(false);
            if (rowSet == null)
            {
                throw new Exception("rowSet is null");
            }
            var data = new List<double>();
            foreach (var row in rowSet)
            {
                data.Add(row.GetValue<int>(0));
                data.Add(row.GetValue<double>(1));
            }
            return data;
        }

        public Task<AggregatedDataRange> FetchAggregatedData(string sourceName, TimeRange timeRange, int aggregationSeconds)
        {
            var minTimeUtc = timeRange.Min.ToDateTime();
            var maxTimeUtc = timeRange.Max.ToDateTime();
            return GetAggregatedDataRange(sourceName, aggregationSeconds, minTimeUtc, maxTimeUtc);
        }

        public Task<RawDataRange> FetchRawDataWithEdges(string sourceName, TimeRange timeRange)
        {
            var minTimeUtc = timeRange.Min.ToDateTime();
            var maxTimeUtc = timeRange.Max.ToDateTime();
            return GetRawDataRangeWithEdges(sourceName, minTimeUtc, maxTimeUtc);
        }

        public Task SubscribeForRawData(string sourceName, CancellationToken cancellationToken, Func<RawDataRange, Task> onReceive)
        {
            // This will get pushed at us from Steve's Python PI Sim regardless
            return Task.CompletedTask;
        }
    }
}
