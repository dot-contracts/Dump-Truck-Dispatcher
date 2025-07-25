using Abp.Configuration;
using Abp.Dependency;
using Abp.Domain.Uow;
using Abp.Runtime.Caching;

namespace DispatcherWeb.Caching
{
    public partial class ListCacheDateKeyLookupService
    {
        public class BaseCacheDependency : ISingletonDependency
        {
            public ICacheManager CacheManager { get; }
            public ISettingManager SettingManager { get; }
            public IUnitOfWorkManager UnitOfWorkManager { get; }

            public BaseCacheDependency(
                ICacheManager cacheManager,
                ISettingManager settingManager,
                IUnitOfWorkManager unitOfWorkManager
            )
            {
                CacheManager = cacheManager;
                SettingManager = settingManager;
                UnitOfWorkManager = unitOfWorkManager;
            }
        }
    }
}
