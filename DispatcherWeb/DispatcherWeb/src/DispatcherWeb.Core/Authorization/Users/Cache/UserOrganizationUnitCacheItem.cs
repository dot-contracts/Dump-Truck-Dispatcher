namespace DispatcherWeb.Authorization.Users.Cache
{
    public class UserOrganizationUnitCacheItem
    {
        public const string CacheName = "UserOrganizationUnitCache";
        public long UserId { get; set; }
        public long OrganizationUnitId { get; set; }
        public int? OfficeId { get; set; }
    }
}
