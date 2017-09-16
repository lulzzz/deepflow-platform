using Amazon;

namespace Deepflow.Platform.Series.DynamoDB
{
    public class DynamoDbConfiguration
    {
        public string AccessKey { get; set; }
        public string SecretKey { get; set; }
        public string RegionSystemName { get; set; }
        public string TableName { get; set; }
        public int WriteBatchSize { get; set; }
        public int WriteBatchParallelism { get; set; }
    }
}
