using Deepflow.Platform.Abstractions.Series;

namespace Deepflow.Platform.Abstractions.Realtime.Messages.Data
{
    public class FetchRawSourceDataResponse : ResponseMessage
    {
        public RawDataRange Range { get; set; }
    }
}
