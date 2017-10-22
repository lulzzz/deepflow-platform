using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Deepflow.Platform.Abstractions.Series;

namespace Deepflow.Data.Service.Services
{
    public interface IDataService
    {
        Task<IEnumerable<AggregatedDataRange>> GetData(Guid entity, Guid attribute, TimeRange timeRange, int aggregationSeconds);
    }
}