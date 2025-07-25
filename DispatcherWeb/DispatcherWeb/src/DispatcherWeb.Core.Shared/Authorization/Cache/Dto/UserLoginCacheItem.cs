using DispatcherWeb.Caching;

namespace DispatcherWeb.Authorization.Cache.Dto
{
    public class UserLoginCacheItem : AuditableCacheItem<long>
    {
        public long UserId { get; set; }
        public string LoginProvider { get; set; }
        public string ProviderKey { get; set; }
    }
}
