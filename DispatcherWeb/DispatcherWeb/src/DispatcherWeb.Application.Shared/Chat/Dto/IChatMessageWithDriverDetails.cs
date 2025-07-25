namespace DispatcherWeb.Chat.Dto
{
    public interface IChatMessageWithDriverDetails
    {
        int? TargetDriverId { get; set; }

        string TargetDriverName { get; set; }

        int? TargetTruckId { get; set; }

        string TargetTruckCode { get; set; }

        int? TargetTruckLeaseHaulerId { get; set; }

        string TargetTruckLeaseHaulerName { get; set; }
    }
}
