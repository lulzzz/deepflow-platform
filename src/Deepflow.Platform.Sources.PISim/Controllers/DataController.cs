using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Cassandra;
using Deepflow.Platform.Abstractions.Series;
using Deepflow.Platform.Common.Data.Configuration;
using Microsoft.AspNetCore.Mvc;

namespace Deepflow.Platform.Sources.PISim.Controllers
{
    [Route("api/v1/[controller]")]
    public class DataController : Controller
    {
        private readonly IDataAggregator _aggregator;
        private readonly ISession _session;

        public DataController(CassandraConfiguration configuration, IDataAggregator aggregator)
        {
            _aggregator = aggregator;
            var cluster = Cluster.Builder()
                .AddContactPoints(configuration.Address)
                .WithDefaultKeyspace(configuration.Keyspace)
                .WithQueryTimeout(configuration.QueryTimeout)
                .Build();
            _session = cluster.Connect();
        }

        [HttpGet("{sourceName}/Aggregations/{aggregationSeconds}")]
        public async Task<AggregatedDataRange> GetAggregatedDataRange(string sourceName, int aggregationSeconds, [FromQuery] DateTime minTimeUtc, [FromQuery] DateTime maxTimeUtc)
        {
            var timeRange = new TimeRange(minTimeUtc, maxTimeUtc).Quantise(aggregationSeconds);
            var raw = await GetRaw(sourceName, timeRange);
            var range = new AggregatedDataRange(timeRange, raw.Data, 5, true);
            return _aggregator.Aggregate(new [] { range }, timeRange, new []{ aggregationSeconds })[300].FirstOrDefault();
        }

        [HttpGet("{sourceName}/Raw")]
        public async Task<RawDataRange> GetRawDataRange(string sourceName, [FromQuery] DateTime minTimeUtc, [FromQuery] DateTime maxTimeUtc)
        {
            var timeRange = new TimeRange(minTimeUtc, maxTimeUtc);
            return await GetRaw(sourceName, timeRange);
        }

        private async Task<RawDataRange> GetRaw(string sourceName, TimeRange timeRange)
        {
            var query = $"SELECT timestamp, value FROM historian_simulator_int WHERE tag = '{sourceName}' AND timestamp >= {timeRange.Min} AND timestamp < {timeRange.Max};";
            var rowSet = await _session.ExecuteAsync(new SimpleStatement(query)).ConfigureAwait(false);
            var data = new List<double>();
            foreach (var row in rowSet)
            {
                data.Add(row.GetValue<int>(0));
                data.Add(row.GetValue<double>(1));
            }
            if (data.Count == 0)
            {
                return null;
            }
            return new RawDataRange(new TimeRange((long)data[0], (long)data[data.Count - 2]), data, true);
        }
    }
}
