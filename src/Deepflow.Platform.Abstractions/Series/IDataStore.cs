using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Deepflow.Platform.Abstractions.Series
{
    public interface IDataStore
    {
        Task<Dictionary<Guid, List<TimeRange>>> LoadTimeRanges(IEnumerable<Guid> series);
        Task SaveTimeRanges(IEnumerable<(Guid series, List<TimeRange> timeRanges)> timeRanges);

        Task<List<double>> LoadData(Guid series, TimeRange timeRange);
        Task SaveData(IEnumerable<(Guid series, List<double> data)> dataRangesBySeries);
    }
}