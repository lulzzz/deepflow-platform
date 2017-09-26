namespace Deepflow.Platform.Abstractions.Realtime.Messages
{
    public class RequestMessage : IncomingMessage
    {
        public int ActionId { get; set; }
        public RequestType RequestType { get; set; }
    }
}
