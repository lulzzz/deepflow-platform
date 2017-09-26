using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Deepflow.Platform.Abstractions.Series.Attribute;

namespace Deepflow.Platform.Abstractions.Series
{
    public interface ISeriesKnower
    {
        Task<Guid> GetAttributeSeriesGuid(Guid entity, Guid attribute, int aggregationSeconds);
        Task<Dictionary<int, Guid>> GetAttributeSeriesGuids(Guid entity, Guid attribute);
        Task<AttributeSeries> GetAttributeSeries(Guid series);
    }
}