using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Deepflow.Platform.Abstractions.Series
{
    public interface IDataProvider
    {
        Task<IEnumerable<DataRange>> GetRanges(Guid series, IEnumerable<TimeRange> timeRanges);
    }
}