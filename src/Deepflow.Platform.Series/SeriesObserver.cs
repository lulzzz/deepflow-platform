using System;
using System.Collections.Generic;
using Deepflow.Platform.Abstractions.Realtime;
using Deepflow.Platform.Abstractions.Series;

namespace Deepflow.Platform.Series
{
    public class SeriesObserver : ISeriesObserver
    {
        private readonly string _socketId;
        private readonly IDataSender _sender;

        public SeriesObserver(string socketId, IDataSender sender)
        {
            _socketId = socketId;
            _sender = sender;
        }

        public void ReceiveData(Guid entity, Guid attribute, Dictionary<int, AggregatedDataRange> aggregatedRanges)
        {
            _sender.SendData(_socketId, entity, attribute, aggregatedRanges);
        }
    }
}
