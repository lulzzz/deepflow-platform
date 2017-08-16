using System;
using System.Threading.Tasks;

namespace Deepflow.Platform.Abstractions.Series
{
    public interface ISeriesKnower
    {
        Task<Guid> GetSeriesGuid(Guid entity, Guid attribute, int aggregationSeconds);
        Task<Series> GetSeries(Guid series);
    }
}