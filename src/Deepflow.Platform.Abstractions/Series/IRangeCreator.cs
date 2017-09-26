using System.Collections.Generic;

namespace Deepflow.Platform.Abstractions.Series
{
    public interface IRangeCreator<TDataRange>
    {
        TDataRange Create(TimeRange timeRange, List<double> data, TDataRange previousRange);
    }
}