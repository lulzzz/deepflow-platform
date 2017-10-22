namespace Deepflow.Platform.Common.Data.Configuration
{
    public class DynamoDbConfiguration
    {
        public string AccessKey { get; set; }
        public string SecretKey { get; set; }
        public string RegionSystemName { get; set; }
        public string DataTableName { get; set; }
        public string RangeTableName { get; set; }
    }
}
