namespace DispatcherWeb.SyncRequests.FcmPushMessages.ChangeDetails
{
    public class FcmDispatchDetailsDto : FcmEntityChangeDetailsDto<int>
    {
        public string DeliveryDate { get; set; }
    }
}
