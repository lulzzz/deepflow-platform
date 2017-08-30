using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Deepflow.Platform.Abstractions.Series;
using Deepflow.Platform.Core.Tools;
using Dse;

namespace Deepflow.Platform.Series.Cassandra
{
    public class CassandraDataProvider : IDataProvider
    {
        private static readonly ISession Session;

        static CassandraDataProvider()
        {
            /*var cluster = DseCluster.Builder()
                .AddContactPoints("52.63.159.36")
                .WithCredentials("cassandra", "PdvGCGnumSM9")
                .WithDefaultKeyspace("quantiseddataseries")
                .WithQueryTimeout(300000)
                .Build();
            Session = cluster.Connect();*/
        }

        public async Task SaveAttributeRanges(Guid series, IEnumerable<DataRange> ranges)
        {
            var preparedStatement = Session.Prepare($"INSERT INTO AverageByTimeDesc (guid, timestamp, value) VALUES (?, ?, ?)");

            var data = ranges.SelectMany(x => x.Data).Batch(10000);

            foreach (var batch in data)
            {
                var batchStatement = new BatchStatement();

                var i = 0;
                double timestamp = 0;
                foreach (var value in batch)
                {
                    if (i % 2 == 0)
                    {
                        timestamp = value;
                    }
                    else
                    {
                        var statement = preparedStatement.Bind(series, timestamp, value);
                        batchStatement.Add(statement);
                    }

                    i++;
                }

                await Session.ExecuteAsync(batchStatement);
            }
        }

        public async Task<IEnumerable<DataRange>> GetAttributeRanges(Guid series, IEnumerable<TimeRange> timeRanges)
        {
            var tasks = timeRanges.Select(timeRange => GetAttributeRanges(series, timeRange));
            var ranges = await Task.WhenAll(tasks);
            return ranges.SelectMany(x => x);
        }

        public async Task<IEnumerable<DataRange>> GetAttributeRanges(Guid series, TimeRange timeRange)
        {
            var query = $"SELECT timestamp, value FROM AverageByTimeDesc WHERE guid = {series} AND timestamp >= '{FromUnixTimestampMinutes((int) timeRange.MinSeconds / 60):s}' AND timestamp < '{FromUnixTimestampMinutes((int)timeRange.MaxSeconds / 60):s}';";

            var rowSet = await Session.ExecuteAsync(new SimpleStatement(query)).ConfigureAwait(false);
            var rows = rowSet.ToList();

            var data = new List<double>(rows.Count * 2);
            var i = 0;
            foreach (var row in rows)
            {
                data[i++] = row.GetValue<int>(0) * 60;
                data[i++] = row.GetValue<float>(1);
            }
            return new List<DataRange> { new DataRange(timeRange, data) };
        }

        public Task SaveAttributeRange(Guid series, DataRange incomingDataRange)
        {
            return SaveAttributeRanges(series, new List<DataRange> { incomingDataRange });
        }

        private static DateTime FromUnixTimestampMinutes(int minutes)
        {
            return new DateTime(1970, 1, 1) + TimeSpan.FromMinutes(minutes);
        }
    }
}
