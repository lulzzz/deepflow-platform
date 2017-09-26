using System.Collections.Generic;
using Deepflow.Platform.Abstractions.Series;

namespace Deepflow.Platform.Abstractions.Realtime.Messages.Data
{
    public class FetchAggregatedAttributeDataResponse : ResponseMessage
    {
        public IEnumerable<AggregatedDataRange> Ranges { get; set; }
    }
}
