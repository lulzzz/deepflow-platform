using System;
using System.Collections.Generic;
using Orleans;

namespace Deepflow.Platform.Abstractions.Series
{
    public interface ISeriesObserver : IGrainObserver
    {
        void ReceiveAggregatedData(Guid entity, Guid attribute, IEnumerable<AggregatedDataRange> dataRanges);
        void ReceiveRawData(Guid entity, Guid attribute, IEnumerable<RawDataRange> dataRanges);
    }
}