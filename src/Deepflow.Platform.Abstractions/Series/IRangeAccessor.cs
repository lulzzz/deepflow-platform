using System.Collections.Generic;

namespace Deepflow.Platform.Abstractions.Series
{
    public interface IRangeAccessor<TRange>
    {
        TimeRange GetTimeRange(TRange range);
        List<double> GetData(TRange range);
    }
}