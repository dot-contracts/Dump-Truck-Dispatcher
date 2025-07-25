namespace DispatcherWeb.Authorization.Cache.Dto
{
    public class UserClaimsCacheItem
    {
        public string OfficeName { get; set; }
        public bool? OfficeCopyChargeTo { get; set; }
        public int? CustomerId { get; set; }
        public string CustomerName { get; set; }
        public bool? CustomerPortalAccessEnabled { get; set; }
        public int? LeaseHaulerId { get; set; }
    }
}
