using System;
using Deepflow.Platform.Abstractions.Series;

namespace Deepflow.Platform.Abstractions.Realtime.Messages.Data
{
    public class AddAggregatedAttributeDataRequest : RequestMessage
    {
        public Guid DataSource { get; set; }
        public string SourceName { get; set; }
        public AggregatedDataRange AggregatedDataRange { get; set; }
    }
}
