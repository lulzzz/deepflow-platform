using System.Collections.Generic;
using Deepflow.Platform.Abstractions.Series;

namespace Deepflow.Platform.Abstractions.Sources
{
    public class SourceSeriesList
    {
        public int AggregationSeconds { get; set; }
        public IEnumerable<SourceSeriesFetchRequests> Series { get; set; }
    }

    public class SourceSeriesFetchRequests
    {
        public string SourceName { get; set; }
        public bool Realtime { get; set; }
        public IEnumerable<TimeRange> TimeRanges { get; set; }
    }
}
