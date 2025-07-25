namespace DispatcherWeb.SyncRequests.FcmPushMessages.ChangeDetails
{

    public abstract class FcmEntityChangeDetailsDto
    {
        public ChangeType ChangeType { get; set; }
    }

    public class FcmEntityChangeDetailsDto<TKey> : FcmEntityChangeDetailsDto
    {
        public TKey Id { get; set; }
    }
}
