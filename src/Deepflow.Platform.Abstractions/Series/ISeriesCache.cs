using System.Collections.Generic;

namespace Deepflow.Platform.Abstractions.Series
{
    public interface ISeriesCache
    {
        IEnumerable<DataRange> GetData(TimeRange timeRange);
        void SetData(DataRange dataRange);
        void SetData(IEnumerable<DataRange> dataRanges);
    }
}
