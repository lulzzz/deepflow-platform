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
using Deepflow.Common.Model.Model;
using Deepflow.Platform.Abstractions.Series;
using Deepflow.Platform.Abstractions.Series.Extensions;
using Deepflow.Platform.Common.Data.Configuration;
using Deepflow.Platform.Core.Tools;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Polly;

namespace Deepflow.Platform.Common.Data.Persistence
{
    public class DynamoDbPersistentDataProvider : IPersistentDataProvider
    {
        private readonly DynamoDbConfiguration _configuration;
        private readonly ILogger<DynamoDbPersistentDataProvider> _logger;
        private readonly IRangeFilterer<TimeRange> _filterer;
        private readonly IModelProvider _model;
        private readonly AmazonDynamoDBClient _client;
        private readonly DynamoDBContext _context;
        private readonly Polly.Retry.RetryPolicy _retryPolicy;
        private SemaphoreSlim _semaphoreSlim = new SemaphoreSlim(1);
        private int _count = 0;
        private int _rampedUpParallelism = 10;
        private int _rampUpCount = 200;

        public DynamoDbPersistentDataProvider(DynamoDbConfiguration configuration, ILogger<DynamoDbPersistentDataProvider> logger, IRangeFilterer<TimeRange> filterer, IModelProvider model)
        {
            _configuration = configuration;
            _logger = logger;
            _filterer = filterer;
            _model = model;
            var credentials = new BasicAWSCredentials(configuration.AccessKey, configuration.SecretKey);
            _client = new AmazonDynamoDBClient(credentials, RegionEndpoint.GetBySystemName(configuration.RegionSystemName));
            _context = new DynamoDBContext(_client);

            _retryPolicy = Policy
                .Handle<ProvisionedThroughputExceededException>()
                .WaitAndRetryAsync(10, count => TimeSpan.FromSeconds(1), (Action<Exception, TimeSpan>)OnRetry);
        }

        private void OnRetry(Exception exception, TimeSpan timeSpan)
        {
            _logger.LogError("Encountered ProvisionedThroughputExceededException, retrying...");
        }

        public async Task SaveData(IEnumerable<(Guid series, IEnumerable<AggregatedDataRange> dataRanges)> seriesData)
        {
            await RunWithRampUp(async () =>
            {
                var writeRequests = seriesData.SelectMany(x => x.dataRanges.GetData().Select(y => CreateDataWriteRequest(x.series, y)));
                if (!writeRequests.Any())
                {
                    return;
                }
                var batchRequests = writeRequests.Batch(25).Select(x => new BatchWriteItemRequest { RequestItems = new Dictionary<string, List<WriteRequest>> { { _configuration.DataTableName, x.ToList() } } });
                foreach (var batchRequest in batchRequests)
                {
                    await _retryPolicy.ExecuteAsync(async () => await _client.BatchWriteItemAsync(batchRequest).ConfigureAwait(false));
                }
            }).ConfigureAwait(false);
        }

        public async Task SaveData(Guid series, IEnumerable<AggregatedDataRange> dataRanges)
        {
            await RunWithRampUp(async () =>
            { 
                var writeRequests = dataRanges.GetData().Select(y => CreateDataWriteRequest(series, y));
                if (!writeRequests.Any())
                {
                    return;
                }
                var batchRequests = writeRequests.Batch(25).Select(x => new BatchWriteItemRequest { RequestItems = new Dictionary<string, List<WriteRequest>> { { _configuration.DataTableName, x.ToList() } } });
                foreach (var batchRequest in batchRequests)
                {
                    await _retryPolicy.ExecuteAsync(async () => await _client.BatchWriteItemAsync(batchRequest).ConfigureAwait(false));
                }
            }).ConfigureAwait(false);
        }

        public async Task<IEnumerable<TimeRange>> GetTimeRanges(Guid series, TimeRange timeRange)
        {
            var all = await RunWithRampUp(() => GetAllTimeRangesInner(series)).ConfigureAwait(false);
            return _filterer.FilterRanges(all, timeRange);
        }

        public async Task<IEnumerable<TimeRange>> GetAllTimeRanges(Guid series)
        {
            return await RunWithRampUp(() => GetAllTimeRangesInner(series)).ConfigureAwait(false);
        }

        private async Task<IEnumerable<TimeRange>> GetAllTimeRangesInner(Guid series)
        {
            var records = await RunQuery<TimeRangeRecord>(series.ToString(), _configuration.RangeTableName).ConfigureAwait(false);
            return records.SelectMany(x => JsonConvert.DeserializeObject<IEnumerable<TimeRange>>(x.Ranges)).ToArray();
        }

        private async Task<List<T>> RunQuery<T>(object hashKeyValue, string tableName)
        {
            return await _retryPolicy.ExecuteAsync(async () =>
            {
                var search = _context.QueryAsync<T>(hashKeyValue, new DynamoDBOperationConfig { OverrideTableName = tableName });
                return await search.GetRemainingAsync().ConfigureAwait(false);
            }).ConfigureAwait(false);
        }

        private async Task<List<T>> RunQuery<T>(object hashKeyValue, QueryOperator queryOperator, IEnumerable<object> values, string tableName)
        {
            return await _retryPolicy.ExecuteAsync(async () =>
            {
                var search = _context.QueryAsync<T>(hashKeyValue, queryOperator, values, new DynamoDBOperationConfig { OverrideTableName = tableName, ConsistentRead = true });
                return await search.GetRemainingAsync().ConfigureAwait(false);
            }).ConfigureAwait(false);
        }

        public async Task SaveTimeRanges(Guid series, IEnumerable<TimeRange> timeRanges)
        {
            await RunWithRampUp(async () =>
            {
                var request = CreateTimeRangeWriteRequest(series, timeRanges);
                await _retryPolicy.ExecuteAsync(async () => await _client.PutItemAsync(request).ConfigureAwait(false)).ConfigureAwait(false);
            }).ConfigureAwait(false);
        }

        public async Task<IEnumerable<AggregatedDataRange>> GetData(Guid series, TimeRange timeRange)
        {
            return await RunWithRampUp(async () =>
            {
                var aggregationTask = _model.ResolveAggregationForSeries(series);
                var timeRangesTask = GetAllTimeRangesInner(series);
                var records = await RunQuery<DataRecord>(series.ToString(), QueryOperator.Between, new object[] { timeRange.Min, timeRange.Max }, _configuration.DataTableName).ConfigureAwait(false);
                var alTimeRanges = await timeRangesTask.ConfigureAwait(false);
                var timeRanges = _filterer.FilterRanges(alTimeRanges, timeRange);
                var aggregation = await aggregationTask.ConfigureAwait(false);
                return ToDataRanges(timeRanges, records, timeRange, aggregation);
            }).ConfigureAwait(false);
        }

        private IEnumerable<AggregatedDataRange> ToDataRanges(IEnumerable<TimeRange> timeRanges, List<DataRecord> data, TimeRange filterTimeRange, int aggregationSeconds)
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
                
                while (index < data.Count && data[index].Time <= timeRange.Min)
                {
                    index++;
                }

                while (index < data.Count && data[index].Time <= timeRange.Max)
                {
                    var record = data[index];
                    rangeData.Add(record.Time);
                    rangeData.Add(record.Value);
                    index += 2;
                }

                if (rangeData.Count > 0)
                {
                    yield return new AggregatedDataRange(thisTimeRange, rangeData, aggregationSeconds);
                }
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

        private PutItemRequest CreateTimeRangeWriteRequest(Guid series, IEnumerable<TimeRange> timeRanges)
        {
            var items = new Dictionary<string, AttributeValue>
            {
                {"Guid", new AttributeValue(series.ToString())},
                {"Null", new AttributeValue { N = "0" }},
                {"Ranges", new AttributeValue(JsonConvert.SerializeObject(timeRanges, JsonSettings.Setttings))}
            };

            return new PutItemRequest(_configuration.RangeTableName, items);
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
            public int Null { get; set; }

            public string Ranges { get; set; }
        }

        private async Task RunWithRampUp(Func<Task> task)
        {
            await _semaphoreSlim.WaitAsync().ConfigureAwait(false);
            try
            {
                await task().ConfigureAwait(false);
            }
            finally
            {
                _semaphoreSlim.Release();
                /*if (Interlocked.Increment(ref _count) == _rampUpCount)
                {
                    _semaphoreSlim = new SemaphoreSlim(_rampedUpParallelism);
                }*/
            }
        }

        private async Task<T> RunWithRampUp<T>(Func<Task<T>> task)
        {
            await _semaphoreSlim.WaitAsync().ConfigureAwait(false);
            try
            {
                return await task().ConfigureAwait(false);
            }
            finally
            {
                _semaphoreSlim.Release();
                /*if (Interlocked.Increment(ref _count) == _rampUpCount)
                {
                    _semaphoreSlim = new SemaphoreSlim(_rampedUpParallelism);
                }*/
            }
        }
    }
}
