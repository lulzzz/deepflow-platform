using System;
using System.Collections.Generic;
using Deepflow.Platform.Abstractions.Series;

namespace Deepflow.Platform.Abstractions.Realtime
{
    public interface IDataSender
    {
        void SendAggregatedData(string socketId, Guid entity, Guid attribute, IEnumerable<AggregatedDataRange> dataRanges);
        void SendRawData(string socketId, Guid entity, Guid attribute, IEnumerable<DataRange> dataRanges);
    }
}
