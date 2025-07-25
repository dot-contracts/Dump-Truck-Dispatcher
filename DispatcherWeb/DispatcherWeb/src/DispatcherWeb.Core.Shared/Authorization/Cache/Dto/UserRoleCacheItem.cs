using DispatcherWeb.Caching;

namespace DispatcherWeb.Authorization.Cache.Dto
{
    public class UserRoleCacheItem : AuditableCacheItem<long>
    {
        public long UserId { get; set; }
        public int RoleId { get; set; }
    }
}
