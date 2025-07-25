using Abp.Configuration;
using Abp.Dependency;
using Abp.Domain.Uow;
using Abp.Runtime.Caching;
using DispatcherWeb.SignalR;

namespace DispatcherWeb.Caching
{
    public class ListCacheBaseDependency : ISingletonDependency
    {
        public ICacheManager CacheManager { get; }
        public ISettingManager SettingManager { get; }
        public IUnitOfWorkManager UnitOfWorkManager { get; }
        public ListCacheDateKeyLookupService DateKeyLookup { get; }
        public ISignalRCommunicator SignalRCommunicator { get; }

        public ListCacheBaseDependency(
            ICacheManager cacheManager,
            ISettingManager settingManager,
            IUnitOfWorkManager unitOfWorkManager,
            ListCacheDateKeyLookupService dateKeyLookup,
            ISignalRCommunicator signalRCommunicator
        )
        {
            CacheManager = cacheManager;
            SettingManager = settingManager;
            UnitOfWorkManager = unitOfWorkManager;
            DateKeyLookup = dateKeyLookup;
            SignalRCommunicator = signalRCommunicator;
        }
    }
}
