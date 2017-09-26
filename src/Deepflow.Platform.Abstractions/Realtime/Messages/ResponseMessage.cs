namespace Deepflow.Platform.Abstractions.Realtime.Messages
{
    public class ResponseMessage : OutgoingMessage
    {
        public int ActionId { get; set; }
        public ResponseType ResponseType { get; set; }
        public bool Succeeded { get; set; }
        public string FriendlyError { get; set; }
    }
}
