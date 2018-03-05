using System.Collections.Generic;
using Deepflow.Platform.Abstractions.Series;

namespace Deepflow.Platform.Abstractions.Realtime.Messages.Data
{
    public class FetchAggregatedAttributeDataWithEdgesResponse : ResponseMessage
    {
        public IEnumerable<AggregatedDataRange> Ranges { get; set; }
    }
}
