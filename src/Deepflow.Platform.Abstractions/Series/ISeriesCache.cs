using System.Collections.Generic;

namespace Deepflow.Platform.Abstractions.Series
{
    public interface ISeriesCache
    {
        IEnumerable<AggregatedDataRange> GetData(TimeRange timeRange);
        void SetData(AggregatedDataRange dataRange);
        void SetData(IEnumerable<AggregatedDataRange> dataRanges);
    }
}
