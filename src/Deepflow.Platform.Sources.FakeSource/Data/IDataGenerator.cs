using Deepflow.Platform.Abstractions.Series;

namespace Deepflow.Platform.Sources.FakeSource.Data
{
    public interface IDataGenerator
    {
        DataRange GenerateData(string sourceName, TimeRange timeRange, int aggregationSeconds);
    }
}