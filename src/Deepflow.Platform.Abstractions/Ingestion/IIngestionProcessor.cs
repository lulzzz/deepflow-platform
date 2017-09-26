using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Deepflow.Platform.Abstractions.Series;

namespace Deepflow.Platform.Abstractions.Ingestion
{
    public interface IIngestionProcessor
    {
        Task AddData(Guid dataSource, string sourceName, AggregatedDataRange aggregatedRange);
    }
}