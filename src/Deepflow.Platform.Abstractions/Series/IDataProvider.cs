using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Deepflow.Platform.Abstractions.Series
{
    public interface IDataProvider
    {
        Task<IEnumerable<DataRange>> GetAttributeRanges(Guid series, IEnumerable<TimeRange> timeRanges);
        Task<DataRange> GetAttributeRange(Guid series, TimeRange timeRange);
    }
}