using Deepflow.Platform.Abstractions.Series;

namespace Deepflow.Platform.Sources.FakeSource.Data
{
    public interface IDataGenerator
    {
        RawDataRange GenerateData(string sourceName, TimeRange timeRange, int aggregationSeconds);
    }
}