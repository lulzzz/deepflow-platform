using System;
using System.Collections.Generic;
using Deepflow.Platform.Abstractions.Series;

namespace Deepflow.Platform.Controllers
{
    public class DataRequestMessage
    {
        public DataRequest Request { get; set; }
        public IEnumerable<DataSubscription> Subscriptions { get; set; }
    }

    public class DataRequest
    {
        public int Id { get; set; }
        public DataRequestType Type { get; set; }
        public Guid Entity { get; set; }
        public Guid Attribute { get; set; }
        public int AggregationSeconds { get; set; }
        public int MinSeconds { get; set; }
        public int MaxSeconds { get; set; }
    }

    public enum DataRequestType
    {
        Attribute,
        Calculation
    }
}
