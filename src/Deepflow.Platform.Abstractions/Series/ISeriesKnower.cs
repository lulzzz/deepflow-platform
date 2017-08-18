using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Deepflow.Platform.Abstractions.Series
{
    public interface ISeriesKnower
    {
        Task<Guid> GetAttributeSeriesGuid(Guid entity, Guid attribute, int aggregationSeconds);
        Task<Guid> GetCalculationSeriesGuid(Guid entity, Guid calculation, int aggregationSeconds);

        Task<Dictionary<int, Guid>> GetAttributeSeriesGuids(Guid entity, Guid attribute);
        Task<Dictionary<int, Guid>> GetCalculationSeriesGuids(Guid entity, Guid calculation);

        Task<AttributeSeries> GetAttributeSeries(Guid series);
        Task<CalculationSeries> GetCalculationSeries(Guid series);
    }
}