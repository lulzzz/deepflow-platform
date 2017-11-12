using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Deepflow.Common.Model;
using Deepflow.Common.Model.Model;
using Deepflow.Ingestion.Service.Processing;
using Deepflow.Ingestion.Service.Realtime;
using Deepflow.Platform.Abstractions.Series;
using Deepflow.Platform.Common.Data.Configuration;
using Deepflow.Platform.Common.Data.Persistence;
using Deepflow.Platform.Core.Tools;
using Deepflow.Platform.Series;
using Microsoft.Extensions.Logging;
using SeriesConfiguration = Deepflow.Common.Model.SeriesConfiguration;

namespace Deepflow.Sandbox.IngestionStress
{
    class Program
    {
        static void Main(string[] args)
        {
            Do().Wait();
        }

        private static async Task Do()
        {
            /*var dynamodbConfiguration = new DynamoDbConfiguration { AccessKey = "AKIAJ6RFWYERKM4SI6LA", SecretKey = "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", RegionSystemName = "ap-southeast-2", RangeTableName = "DeepflowTimeRanges", DataTableName = "DeepflowData" };
            var timeCreator = new TimeRangeCreator();
            var timePolicy = new TimeRangeFilteringPolicy();
            var timeAccessor = new TimeRangeAccessor();
            var timeFilterer = new RangeFilterer<TimeRange>(timeCreator, timePolicy, timeAccessor);
            var timeJoiner = new RangeJoiner<TimeRange>(timeCreator, timeAccessor, new Logger<RangeJoiner<TimeRange>>());
            var timeMerger = new RangeMerger<TimeRange>(timeFilterer, timeJoiner, timeAccessor);
            var seriesConfiguration = new SeriesConfiguration { AggregationsSeconds = new[] { 300, 900, 1800, 3600, 7200, 14400, 28800, 43200, 86400, 172800, 345600 } };
            var modelConfiguration = new ModelConfiguration
            {
                Entities = Enumerable.Range(0, 1).Select(x => Guid.NewGuid()).ToArray(),
                Attributes = Enumerable.Range(0, 1).Select(x => Guid.NewGuid()).ToArray()
            };
            var modelProvider = new ModelProvider(seriesConfiguration, modelConfiguration);
            //var persistence = new DynamoDbPersistentDataProvider(dynamodbConfiguration, new Logger<DynamoDbPersistentDataProvider>(), timeFilterer, modelProvider);
            var cassandraConfiguration = new CassandraConfiguration { Address = "54.206.106.29", Username = "cassandra", Password = "bitnami", Keyspace = "deepflowtimeseries", QueryTimeout = 300000 };
            var persistence = new CassandraPersistentDataProvider(cassandraConfiguration, timeFilterer, modelProvider);
            var aggregator = new DataAggregator(new Logger<DataAggregator>());
            var creator = new AggregatedRangeCreator();
            var policy = new AggregateRangeFilteringPolicy();
            var accessor = new AggregatedRangeAccessor();
            var filterer = new RangeFilterer<AggregatedDataRange>(creator, policy, accessor);
            var joiner = new RangeJoiner<AggregatedDataRange>(creator, accessor, new Logger<RangeJoiner<AggregatedDataRange>>());
            var merger = new RangeMerger<AggregatedDataRange>(filterer, joiner, accessor);
            var tripCounter = new TripCounterFactory(new Logger<TripCounter>());
            var ingestionProcessor = new IngestionProcessor(persistence, aggregator, modelProvider, new Messenger(), merger, timeMerger, filterer, seriesConfiguration, new Logger<IngestionProcessor>(), tripCounter);

            var points = 1000;
            var interval = 300;
            var min = DateTime.UtcNow.SecondsSince1970Utc();
            min = min - min % interval;
            var max = min + points * interval;
            var random = new Random();
            var range = new AggregatedDataRange(min, max, Enumerable.Range(0, points).SelectMany(x => new[] { min + x * interval + interval, random.NextDouble() }).ToList(), interval);

            Console.WriteLine("Saving");

            /*modelConfiguration.Entities.Select(entity =>
            {
                return 
            });#1#

            while (true)
            {
                await tripCounter.Run("Outer", async () =>
                {
                    await ingestionProcessor.ReceiveHistoricalData(modelConfiguration.Entities[0], modelConfiguration.Attributes[0], range);
                });
                
                Console.WriteLine("Done");
            }

            Console.WriteLine("Done");
            Console.ReadKey();*/
        }
    }

    class Logger<T> : ILogger<T>
    {
        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception, Func<TState, Exception, string> formatter)
        {
            Console.WriteLine(formatter(state, exception));
        }

        public bool IsEnabled(LogLevel logLevel)
        {
            return true;
        }

        public IDisposable BeginScope<TState>(TState state)
        {
            throw new NotImplementedException();
        }
    }

    class Messenger : IDataMessenger
    {
        public Task NotifyRaw(Guid entity, Guid attribute, RawDataRange dataRange)
        {
            return Task.CompletedTask;
        }

        public Task NotifyAggregated(Guid entity, Guid attribute, Dictionary<int, AggregatedDataRange> dataRanges)
        {
            return Task.CompletedTask;
        }
    }
}
