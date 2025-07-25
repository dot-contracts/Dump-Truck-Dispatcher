namespace DispatcherWeb.SyncRequests.Dto
{
    public class UpdateDriverSyncRequestTimestampInput
    {
        public int? DriverId { get; set; }
        public int? TenantId { get; set; }
        public string EntityType { get; set; }
    }
}
