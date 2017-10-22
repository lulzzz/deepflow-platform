using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Deepflow.Common.Model.Model
{
    public interface IModelProvider
    {
        Task<(Guid entity, Guid attribute)> ResolveEntityAndAttribute(Guid dataSource, string sourceName);
        Task<Guid> ResolveSeries(Guid entity, Guid attribute, int aggregationSeconds);
        Task<int> ResolveAggregationForSeries(Guid series);
        Task<IEnumerable<string>> ResolveSourceNamesForDataSource(Guid dataSource);
    }
}