using System.Collections.Generic;
using Deepflow.Platform.Abstractions.Series;

namespace Deepflow.Platform.Agent.Model
{
    public class RawDataRange
    {
        public string Name { get; set; }
        public TimeRange TimeRange { get; set; }
        public List<Datum> Data { get; set; }
    }
}
