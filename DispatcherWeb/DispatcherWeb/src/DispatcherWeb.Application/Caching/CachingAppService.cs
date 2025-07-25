using Abp.Authorization;
using Abp.Runtime.Caching;

namespace DispatcherWeb.Caching
{
    [AbpAuthorize]
    public partial class CachingAppService : DispatcherWebAppServiceBase, ICachingAppService
    {
        private readonly ListCacheCollection _listCaches;
        private readonly ICacheManager _cacheManager;

        public CachingAppService(
            ListCacheCollection listCaches,
            ICacheManager cacheManager
        )
        {
            _listCaches = listCaches;
            _cacheManager = cacheManager;
        }
    }
}
