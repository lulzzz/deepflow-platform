using Deepflow.Platform.Abstractions.Series;

namespace Deepflow.Platform.Sources.FakeSource.Data
{
    public interface IDataGenerator
    {
        RawDataRange GenerateRawPoint(string sourceName, int time, int aggregationSeconds);
        RawDataRange GenerateRawRange(string sourceName, TimeRange timeRange, int aggregationSeconds);
        AggregatedDataRange GenerateRange(string sourceName, TimeRange timeRange, int aggregationSeconds);
    }
}