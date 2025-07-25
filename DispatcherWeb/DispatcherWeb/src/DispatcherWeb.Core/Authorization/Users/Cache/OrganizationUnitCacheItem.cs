namespace DispatcherWeb.Authorization.Users.Cache
{
    public class OrganizationUnitCacheItem
    {
        public const string CacheName = "OrganizationUnitCache";
        public long Id { get; set; }
        public string Name { get; set; }
        public int? OfficeId { get; set; }
    }
}
