using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Deepflow.Platform.Abstractions.Series;

namespace Deepflow.Ingestion.Service.Realtime
{
    public interface IDataMessenger
    {
        Task Notify(Guid entity, Guid attribute, Dictionary<int, AggregatedDataRange> aggregatedDataRanges, RawDataRange rawDataRange);
    }
}