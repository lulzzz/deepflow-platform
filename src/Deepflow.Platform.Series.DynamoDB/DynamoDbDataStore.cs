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
using Polly;
using RetryPolicy = Polly.Retry.RetryPolicy;

namespace Deepflow.Platform.Series.DynamoDB
{
    public class DynamoDbDataStore : IDataStore
    {
        private readonly DynamoDbConfiguration _configuration;
        private readonly ILogger<DynamoDbDataStore> _logger;
        private readonly IRangeFilterer<TimeRange> _timeFilterer;
        private readonly AmazonDynamoDBClient _client;
        private readonly DynamoDBContext _context;
        private readonly RetryPolicy _retryPolicy;
        private readonly SemaphoreSlim _semaphore = new SemaphoreSlim(10000);

        public DynamoDbDataStore(DynamoDbConfiguration configuration, ILogger<DynamoDbDataStore> logger, IRangeFilterer<TimeRange> timeFilterer)
        {
            _configuration = configuration;
            _logger = logger;
            _timeFilterer = timeFilterer;

            var credentials = new BasicAWSCredentials(configuration.AccessKey, configuration.SecretKey);
            _client = new AmazonDynamoDBClient(credentials, RegionEndpoint.GetBySystemName(configuration.RegionSystemName));
            _context = new DynamoDBContext(_client);

            _retryPolicy = Policy
                .Handle<ProvisionedThroughputExceededException>()
                .WaitAndRetryForeverAsync(i => TimeSpan.Zero, (Action<Exception, TimeSpan>) OnRetry);
        }

        private void OnRetry(Exception exception, TimeSpan timeSpan)
        {
            _logger.LogError("Encountered ProvisionedThroughputExceededException, retrying...");
        }
        
        public async Task<Dictionary<Guid, List<TimeRange>>> LoadTimeRanges(IEnumerable<Guid> series)
        {
            _logger.LogInformation($"Loading time ranges...");
            var records = await Task.WhenAll(series.Select(LoadTimeRanges));
            var result = records.GroupBy(x => x.series).ToDictionary(x => x.Key, x => x.SelectMany(y => y.records).Select(y => new TimeRange(y.Min, y.Max)).ToList());
            _logger.LogInformation($"Loaded time ranges...");
            return result;
        }

        private async Task<(Guid series, List<TimeRangeRecord> records)> LoadTimeRanges(Guid series)
        {
            try
            {
                await _semaphore.WaitAsync();
                var search = _context.QueryAsync<TimeRangeRecord>(series.ToString(), new DynamoDBOperationConfig { OverrideTableName = _configuration.RangeTableName });
                var records = await search.GetRemainingAsync();
                return (series, records);
            }
            finally
            {
                _semaphore.Release();
            }
        }

        public async Task SaveTimeRanges(IEnumerable<(Guid series, List<TimeRange> timeRanges)> timeRanges)
        {
            await _semaphore.WaitAsync();
            _logger.LogInformation($"Saving time ranges...");
            var writeRequests = timeRanges.SelectMany(x => x.timeRanges.Select(y => CreateTimeRangeWriteRequest(x.series, y))).ToList();
            await Save(writeRequests, _configuration.RangeTableName);
            _logger.LogInformation($"Saved {writeRequests.Count} time ranges for {timeRanges.Count()} series");
        }

        public async Task<List<double>> LoadData(Guid series, TimeRange timeRange)
        {
            try
            {
                await _semaphore.WaitAsync();
                _logger.LogInformation($"Loading data...");
                var search = _context.QueryAsync<DataRecord>(series.ToString(), QueryOperator.Between, new object[] { timeRange.Min, timeRange.Max }, new DynamoDBOperationConfig { OverrideTableName = _configuration.DataTableName });
                var records = await search.GetRemainingAsync();

                List<double> data = new List<double>(records.Count * 2);
                foreach (var record in records)
                {
                    data.Add(record.Time);
                    data.Add(record.Value);
                }

                _logger.LogInformation($"Loaded data...");
                return data;
            }
            finally
            {
                _semaphore.Release();
            }
            }

        public async Task SaveData(IEnumerable<(Guid series, List<double> data)> dataRangesBySeries)
        {
            _logger.LogInformation($"Saving data...");
            var writeRequests = dataRangesBySeries.SelectMany(x => x.data.GetData().Select(y => CreateDataWriteRequest(x.series, y))).ToList();
            await Save(writeRequests, _configuration.DataTableName);
            _logger.LogInformation($"Saved data for {dataRangesBySeries.Count()} series");
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

        private WriteRequest CreateTimeRangeWriteRequest(Guid series, TimeRange timeRange)
        {
            var items = new Dictionary<string, AttributeValue>
            {
                {"Guid", new AttributeValue(series.ToString())},
                {"Min", new AttributeValue {N = ((int)timeRange.Min).ToString()}},
                {"Max", new AttributeValue {N = ((int)timeRange.Max).ToString()}}
            };

            return new WriteRequest(new PutRequest(items));
        }

        private class DataRecord
        {
            [DynamoDBHashKey]
            public string Guid { get; set; }

            [DynamoDBRangeKey]
            public long Time { get; set; }

            public double Value { get; set; }
        }

        private class TimeRangeRecord
        {
            [DynamoDBHashKey]
            public string Guid { get; set; } 

            [DynamoDBRangeKey]
            public long Max { get; set; }
            
            public long Min { get; set; }
        }
    }
}
