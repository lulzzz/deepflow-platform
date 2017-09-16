using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Deepflow.Platform.Abstractions.Series
{
    public interface IDataProvider
    {
        Task<IEnumerable<DataRange>> GetAttributeRanges(Guid series, TimeRange timeRange);
        Task SaveAttributeRange(Guid series, DataRange incomingDataRange);
    }
}