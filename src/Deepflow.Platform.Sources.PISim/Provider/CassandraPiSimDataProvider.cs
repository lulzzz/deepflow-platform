using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Amazon.Runtime.Internal.Util;
using Cassandra;
using Deepflow.Platform.Abstractions.Series;
using Deepflow.Platform.Agent.Provider;
using Deepflow.Platform.Common.Data.Configuration;
using Deepflow.Platform.Core.Tools;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace Deepflow.Platform.Sources.PISim.Provider
{
    public class CassandraPiSimDataProvider : IPiSimDataProvider, ISourceDataProvider
    {
        private readonly CassandraConfiguration _configuration;
        private readonly IDataAggregator _aggregator;
        private readonly ILogger<CassandraPiSimDataProvider> _logger;
        private readonly ISession _session;

        public CassandraPiSimDataProvider(CassandraConfiguration configuration, IDataAggregator aggregator, ILogger<CassandraPiSimDataProvider> logger)
        {
            _configuration = configuration;
            _aggregator = aggregator;
            _logger = logger;
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
            var raw = await GetRaw(sourceName, timeRange);
            if (raw.Data.Count == 0)
            {
                return new AggregatedDataRange(timeRange, new List<double>(), aggregationSeconds);
            }
            var range = new AggregatedDataRange(timeRange, raw.Data, 5, true);
            return _aggregator.Aggregate(new[] { range }, timeRange, new[] { aggregationSeconds })[300].FirstOrDefault();
        }
        
        public async Task<RawDataRange> GetRawDataRange(string sourceName, [FromQuery] DateTime minTimeUtc, [FromQuery] DateTime maxTimeUtc)
        {
            var timeRange = new TimeRange(minTimeUtc, maxTimeUtc);
            return await GetRaw(sourceName, timeRange);
        }

        private async Task<RawDataRange> GetRaw(string sourceName, TimeRange timeRange)
        {
            var query = $"SELECT timestamp, value FROM {_configuration.TableName} WHERE tag = '{sourceName}' AND timestamp >= {timeRange.Min} AND timestamp < {timeRange.Max};";
            var stopwatch = Stopwatch.StartNew();
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
            _logger.LogDebug($"Query finished in {stopwatch.ElapsedMilliseconds}ms with {data?.Count / 2} points");
            return new RawDataRange(timeRange, data, true);
        }

        public Task<AggregatedDataRange> FetchAggregatedData(string sourceName, TimeRange timeRange, int aggregationSeconds)
        {
            var minTimeUtc = timeRange.Min.ToDateTime();
            var maxTimeUtc = timeRange.Max.ToDateTime();
            return GetAggregatedDataRange(sourceName, aggregationSeconds, minTimeUtc, maxTimeUtc);
        }

        public Task<RawDataRange> FetchRawData(string sourceName, TimeRange timeRange)
        {
            var minTimeUtc = timeRange.Min.ToDateTime();
            var maxTimeUtc = timeRange.Max.ToDateTime();
            return GetRawDataRange(sourceName, minTimeUtc, maxTimeUtc);
        }

        public Task SubscribeForRawData(string sourceName, CancellationToken cancellationToken, Func<RawDataRange, Task> onReceive)
        {
            // This will get pushed at us from Steve's Python PI Sim regardless
            return Task.CompletedTask;
        }
    }
}
