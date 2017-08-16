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

        public async Task SaveRanges(Guid series, IEnumerable<DataRange> ranges)
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

        public async Task<IEnumerable<DataRange>> GetRanges(Guid series, IEnumerable<TimeRange> timeRanges)
        {
            IEnumerable<Task<DataRange>> tasks = timeRanges.Select(timeRange => GetRange(series, timeRange));
            return await Task.WhenAll(tasks);
        }

        private async Task<DataRange> GetRange(Guid series, TimeRange timeRange)
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
            return new DataRange(timeRange, data);
        }

        private static DateTime FromUnixTimestampMinutes(int minutes)
        {
            return new DateTime(1970, 1, 1) + TimeSpan.FromMinutes(minutes);
        }
    }
}
