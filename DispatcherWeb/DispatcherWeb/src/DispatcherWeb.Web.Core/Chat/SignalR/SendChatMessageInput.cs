namespace DispatcherWeb.Web.Chat.SignalR
{
    public class SendChatMessageInput
    {
        public long TargetUserId { get; set; }

        public string Message { get; set; }

        public int? SourceTruckId { get; set; }

        public int? SourceTrailerId { get; set; }

        public int? SourceDriverId { get; set; }
    }
}
