using System.Collections.Generic;

namespace Deepflow.Platform.Abstractions.Series
{
    public interface IDataRangeCreator<TDataRange>
    {
        TDataRange Create(TimeRange timeRange, List<double> data, TDataRange previousRange);
    }
}