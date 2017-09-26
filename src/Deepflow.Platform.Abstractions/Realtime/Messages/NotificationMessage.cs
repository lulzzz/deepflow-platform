namespace Deepflow.Platform.Abstractions.Realtime.Messages
{
    public class NotificationMessage : OutgoingMessage
    {
        public NotificationType NotificationType { get; set; }
    }
}
