using Deepflow.Platform.Abstractions.Series;

namespace Deepflow.Platform.Abstractions.Realtime.Messages.Data
{
    public class FetchRawSourceDataRequest : RequestMessage
    {
        public string SourceName { get; set; }
        public TimeRange TimeRange { get; set; }
    }
}
