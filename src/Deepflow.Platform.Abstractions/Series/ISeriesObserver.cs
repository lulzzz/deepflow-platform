using System;
using System.Collections.Generic;
using Orleans;

namespace Deepflow.Platform.Abstractions.Series
{
    public interface ISeriesObserver : IGrainObserver
    {
        void ReceiveData(Guid entity, Guid attribute, Dictionary<int, AggregatedDataRange> aggregatedRanges);
    }
}