using Deepflow.Platform.Abstractions.Series;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Amazon;
using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.DataModel;
using Amazon.DynamoDBv2.DocumentModel;
using Amazon.DynamoDBv2.Model;
using Amazon.Runtime;
using Deepflow.Platform.Abstractions.Series.Extensions;
using Deepflow.Platform.Core.Async;
using Deepflow.Platform.Core.Tools;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Polly;
using RetryPolicy = Polly.Retry.RetryPolicy;

namespace Deepflow.Platform.Series.DynamoDB
{
    public class DynamoDbDataStore : IDataStore
    {
        private readonly DynamoDbConfiguration _configuration;
        private readonly ILogger<DynamoDbDataStore> _logger;
        private readonly IRangeFilterer<TimeRange> _timeFilterer;
        private readonly TripCounterFactory _tripCounterFactory;
        private readonly AmazonDynamoDBClient _client;
        private readonly DynamoDBContext _context;
        private readonly RetryPolicy _retryPolicy;
        private readonly SemaphoreSlim _semaphore = new SemaphoreSlim(10000);
        private int _timeRangesSavedSinceLastCheck = 0;
        private int _dataPointsSavedSinceLastCheck = 0;

        public DynamoDbDataStore(DynamoDbConfiguration configuration, ILogger<DynamoDbDataStore> logger, IRangeFilterer<TimeRange> timeFilterer, TripCounterFactory tripCounterFactory)
        {
            _configuration = configuration;
            _logger = logger;
            _timeFilterer = timeFilterer;
            _tripCounterFactory = tripCounterFactory;

            var credentials = new BasicAWSCredentials(configuration.AccessKey, configuration.SecretKey);
            _client = new AmazonDynamoDBClient(credentials, RegionEndpoint.GetBySystemName(configuration.RegionSystemName));
            _context = new DynamoDBContext(_client);

            _retryPolicy = Policy
                .Handle<ProvisionedThroughputExceededException>()
                .WaitAndRetryForeverAsync(i => TimeSpan.Zero, (Action<Exception, TimeSpan>) OnRetry);

            Task.Run(async () =>
            {
                while (true)
                {
                    var timeRanges = Interlocked.Exchange(ref _timeRangesSavedSinceLastCheck, 0);
                    var dataPoints = Interlocked.Exchange(ref _dataPointsSavedSinceLastCheck, 0);

                    _logger.LogDebug($"{timeRanges} timeRanges {dataPoints} dataPoints");
                    await Task.Delay(1000);
                }
            });
        }

        private void OnRetry(Exception exception, TimeSpan timeSpan)
        {
            _logger.LogError("Encountered ProvisionedThroughputExceededException, retrying...");
        }
        
        public async Task<Dictionary<Guid, List<TimeRange>>> LoadTimeRanges(IEnumerable<Guid> series)
        {
            try
            {
                _logger.LogDebug($"Loading time ranges...");
                var records = await Task.WhenAll(series.Select(LoadTimeRanges));
                var result = records.GroupBy(x => x.series).ToDictionary(x => x.Key, x => x.SelectMany(y => y.records).ToList());
                _logger.LogDebug($"Loaded time ranges...");
                return result;
            }
            catch (Exception exception)
            {
                _logger.LogDebug("Failed loading time ranges" + exception.Message);
                _logger.LogError(new EventId(107), exception, "Failed loading time ranges");
                throw;
            }
        }

        private async Task<(Guid series, List<TimeRange> records)> LoadTimeRanges(Guid series)
        {
            try
            {
                await _semaphore.WaitAsync();
                var search = _context.QueryAsync<TimeRangeRecord>(series.ToString(), new DynamoDBOperationConfig { OverrideTableName = _configuration.RangeTableName });
                var records = await search.GetRemainingAsync();
                var timeRanges = records.SelectMany(x => JsonConvert.DeserializeObject<List<TimeRange>>(x.Ranges)).ToList();
                return (series, timeRanges);
            }
            finally
            {
                _semaphore.Release();
            }
        }

        public async Task SaveTimeRanges(IEnumerable<(Guid series, List<TimeRange> timeRanges)> timeRanges)
        {
            await _semaphore.WaitAsync();
            _logger.LogDebug($"Saving time ranges...");
            var writeRequests = timeRanges.Select(x => CreateTimeRangeWriteRequest(x.series, x.timeRanges)).ToList();
            await Save(writeRequests, _configuration.RangeTableName);
            Interlocked.Add(ref _timeRangesSavedSinceLastCheck, writeRequests.Count);
            _logger.LogDebug($"Saved {writeRequests.Count} time ranges for {timeRanges.Count()} series");
        }

        public async Task<List<double>> LoadData(Guid series, TimeRange timeRange)
        {
            try
            {
                await _semaphore.WaitAsync();
                _logger.LogDebug($"Loading data...");
                var search = _context.QueryAsync<DataRecord>(series.ToString(), QueryOperator.Between, new object[] { timeRange.Min, timeRange.Max }, new DynamoDBOperationConfig { OverrideTableName = _configuration.DataTableName });
                var records = await search.GetRemainingAsync();

                List<double> data = new List<double>(records.Count * 2);
                foreach (var record in records)
                {
                    data.Add(record.Time);
                    data.Add(record.Value);
                }

                _logger.LogDebug($"Loaded data...");
                return data;
            }
            finally
            {
                _semaphore.Release();
            }
        }

        public async Task SaveData(IEnumerable<(Guid series, List<double> data)> dataRangesBySeries)
        {
            using (_tripCounterFactory.Create("DynamoDbDataStore.SaveData"))
            {
                _logger.LogDebug($"Saving data...");
                var writeRequests = dataRangesBySeries.SelectMany(x => x.data.GetData().Select(y => CreateDataWriteRequest(x.series, y))).ToList();
                await Save(writeRequests, _configuration.DataTableName);
                Interlocked.Add(ref _dataPointsSavedSinceLastCheck, writeRequests.Count);
                _logger.LogDebug($"Saved data for {dataRangesBySeries.Count()} series");
            }
        }

        private async Task Save(List<WriteRequest> writeRequests, string tableName)
        {
            try
            {
                var batchRequests = writeRequests.Batch(_configuration.WriteBatchSize).Select(x => new BatchWriteItemRequest { RequestItems = new Dictionary<string, List<WriteRequest>> { { tableName, x.ToList() } } });

                foreach (var batchRequest in batchRequests)
                {
                    await _retryPolicy.ExecuteAsync(async () => await _client.BatchWriteItemAsync(batchRequest));
                }
            }
            finally
            {
                _semaphore.Release();
            }
        }

        private WriteRequest CreateDataWriteRequest(Guid series, Datum datum)
        {
            var items = new Dictionary<string, AttributeValue>
            {
                {"Guid", new AttributeValue(series.ToString())},
                {"Time", new AttributeValue {N = ((int)datum.Time).ToString()}},
                {"Value", new AttributeValue {N = datum.Value.ToString()}}
            };

            return new WriteRequest(new PutRequest(items));
        }

        private WriteRequest CreateTimeRangeWriteRequest(Guid series, IEnumerable<TimeRange> timeRanges)
        {
            var items = new Dictionary<string, AttributeValue>
            {
                {"Guid", new AttributeValue(series.ToString())},
                {"Time", new AttributeValue { N = ((int)timeRanges.LastOrDefault()?.Max).ToString() ?? "0" }},
                {"Ranges", new AttributeValue(JsonConvert.SerializeObject(timeRanges, JsonSettings.Setttings))}
            };

            return new WriteRequest(new PutRequest(items));
        }
    }

    public class DataRecord
    {
        [DynamoDBHashKey]
        public string Guid { get; set; }

        [DynamoDBRangeKey]
        public long Time { get; set; }

        public double Value { get; set; }
    }

    public class TimeRangeRecord
    {
        [DynamoDBHashKey]
        public string Guid { get; set; }

        [DynamoDBRangeKey]
        public long Time { get; set; }

        public string Ranges { get; set; }
    }
}
