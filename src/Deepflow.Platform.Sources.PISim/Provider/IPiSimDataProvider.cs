using System;
using System.Threading.Tasks;
using Deepflow.Platform.Abstractions.Series;
using Microsoft.AspNetCore.Mvc;

namespace Deepflow.Platform.Sources.PISim.Provider
{
    public interface IPiSimDataProvider
    {
        Task<AggregatedDataRange> GetAggregatedDataRange(string sourceName, int aggregationSeconds, DateTime minTimeUtc, DateTime maxTimeUtc);

        Task<RawDataRange> GetRawDataRangeWithEdges(string sourceName, [FromQuery] DateTime minTimeUtc, [FromQuery] DateTime maxTimeUtc);
    }
}