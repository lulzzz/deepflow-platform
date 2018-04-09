using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Deepflow.Platform.Abstractions.Series;

namespace Deepflow.Ingestion.Service.Realtime
{
    public interface IDataMessenger
    {
        Task NotifyRaw(Guid entity, Guid attribute, RawDataRange dataRange);
        Task NotifyAggregated(Guid entity, Guid attribute, Dictionary<int, IEnumerable<AggregatedDataRange>> dataRanges);
    }
}