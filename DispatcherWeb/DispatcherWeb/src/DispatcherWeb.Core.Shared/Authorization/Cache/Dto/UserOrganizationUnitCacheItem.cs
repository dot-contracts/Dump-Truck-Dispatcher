using DispatcherWeb.Caching;

namespace DispatcherWeb.Authorization.Cache.Dto
{
    public class UserOrganizationUnitCacheItem : AuditableCacheItem<long>
    {
        public long OrganizationUnitId { get; set; }
        public long UserId { get; set; }
    }
}
