using DispatcherWeb.Caching;

namespace DispatcherWeb.Authorization.Cache.Dto
{
    public class OrganizationUnitRoleCacheItem : AuditableCacheItem<long>
    {
        public long OrganizationUnitId { get; set; }
        public int RoleId { get; set; }
    }
}
