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
using Deepflow.Platform.Core.Tools;
using Microsoft.Extensions.Logging;
using Polly;
using RetryPolicy = Polly.Retry.RetryPolicy;

namespace Deepflow.Platform.Series.DynamoDB
{
    public class DynamoDbDataProvider : IDataProvider
    {
        private readonly DynamoDbConfiguration _configuration;
        private readonly ILogger<DynamoDbDataProvider> _logger;
        private readonly AmazonDynamoDBClient _client;
        private readonly RetryPolicy _retryPolicy;

        public DynamoDbDataProvider(DynamoDbConfiguration configuration, ILogger<DynamoDbDataProvider> logger)
        {
            _configuration = configuration;
            _logger = logger;

            var credentials = new BasicAWSCredentials(configuration.AccessKey, configuration.SecretKey);
            _client = new AmazonDynamoDBClient(credentials, RegionEndpoint.GetBySystemName(configuration.RegionSystemName));

            _retryPolicy = Policy
                .Handle<ProvisionedThroughputExceededException>()
                .WaitAndRetryForeverAsync(i => TimeSpan.Zero, (Action<Exception, TimeSpan>) OnRetry);
        }

        private void OnRetry(Exception exception, TimeSpan timeSpan)
        {
            _logger.LogError("Encountered ProvisionedThroughputExceededException, retrying...");
        }

        public async Task<IEnumerable<DataRange>> GetAttributeRanges(Guid series, TimeRange timeRange)
        {
            try
            {
                var context = new DynamoDBContext(_client);
                var search = context.QueryAsync<ReadRecord>(series, QueryOperator.Between, new object[] { timeRange.MinSeconds, timeRange.MaxSeconds }, new DynamoDBOperationConfig { OverrideTableName = _configuration.TableName });
                var records = await search.GetRemainingAsync();
                var data = new double[records.Count * 2];
                var index = 0;
                foreach (var record in records)
                {
                    data[index] = record.TimeUtcSeconds;
                    data[index + 1] = record.Value;
                    index += 2;
                }
                return new List<DataRange> { new DataRange(timeRange, data.ToList()) };
            }
            catch (Exception exception)
            {
                _logger.LogError(null, exception, $"Could not get data for {series} between {timeRange.MinSeconds} and {timeRange.MaxSeconds}");
                throw;
            }
        }

        public async Task SaveAttributeRange(Guid series, DataRange dataRange)
        {
            try
            {
                var writeRequests = dataRange.GetData().Select(datum => CreateWriteRequest(series, datum)).ToList();
                var batchRequests = writeRequests.Batch(_configuration.WriteBatchSize).Select(x => new BatchWriteItemRequest { RequestItems = new Dictionary<string, List<WriteRequest>> { { _configuration.TableName, x.ToList() } } });
                
                var total = writeRequests.Count;
                _logger.LogInformation($"Preparing to save {total} points for {series}");

                var count = 0;
                await batchRequests.ForEachAsync(_configuration.WriteBatchParallelism, async request =>
                {
                    _logger.LogInformation($"Preapring to save batch");
                    await _retryPolicy.ExecuteAsync(async () => await _client.BatchWriteItemAsync(request));
                    Interlocked.Add(ref count, request.RequestItems.First().Value.Count);
                    _logger.LogInformation($"Saved {count} out of {total} records...");
                });

                _logger.LogInformation($"Saved {total} points for {series}");
            }
            catch (Exception exception)
            {
                _logger.LogError(null, exception, $"Could not save {dataRange.Data.Count / 2} points for {series}");
                throw;
            }
        }

        private WriteRequest CreateWriteRequest(Guid series, Datum datum)
        {
            var items = new Dictionary<string, AttributeValue>
            {
                {"Guid", new AttributeValue(series.ToString())},
                {"TimeUtcSeconds", new AttributeValue {N = ((int)datum.Time).ToString()}},
                {"Value", new AttributeValue {N = datum.Value.ToString()}}
            };

            return new WriteRequest(new PutRequest(items));
        }
        
        private class ReadRecord
        {
            [DynamoDBHashKey]
            public Guid Guid { get; set; }

            [DynamoDBRangeKey]
            public long TimeUtcSeconds { get; set; }

            public double Value { get; set; }
        }
    }
}
