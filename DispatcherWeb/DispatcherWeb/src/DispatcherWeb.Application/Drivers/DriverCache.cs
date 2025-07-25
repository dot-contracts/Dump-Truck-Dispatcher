using System.Linq;
using System.Threading.Tasks;
using Abp.Dependency;
using Abp.Domain.Repositories;
using Abp.Domain.Uow;
using Abp.Events.Bus.Entities;
using Abp.Events.Bus.Handlers;
using Abp.Runtime.Caching;
using DispatcherWeb.Drivers.Dto;
using Microsoft.EntityFrameworkCore;

namespace DispatcherWeb.Drivers
{
    public class DriverCache : IDriverCache, ISingletonDependency,
        IAsyncEventHandler<EntityUpdatedEventData<Driver>>
    {
        private readonly ICacheManager _cacheManager;
        private readonly IRepository<Driver> _driverRepository;
        private readonly IUnitOfWorkManager _unitOfWorkManager;
        private const string CacheName = "Driver-Cache";

        public DriverCache(
            ICacheManager cacheManager,
            IRepository<Driver> driverRepository,
            IUnitOfWorkManager unitOfWorkManager
        )
        {
            _cacheManager = cacheManager;
            _driverRepository = driverRepository;
            _unitOfWorkManager = unitOfWorkManager;
        }

        public async Task<DriverCacheItem> GetDriverFromCacheOrDefault(int driverId)
        {
            var cache = GetCache();
            var cacheItem = await cache.GetAsync(driverId, async id =>
            {
                return await _unitOfWorkManager.WithUnitOfWorkAsync(async () =>
                {
                    return await (await _driverRepository.GetQueryAsync())
                        .Where(d => d.Id == id)
                        .Select(x => new DriverCacheItem
                        {
                            Id = x.Id,
                            IsDeleted = x.IsDeleted,
                            DeletionTime = x.DeletionTime,
                            CreationTime = x.CreationTime,
                            LastModificationTime = x.LastModificationTime,
                            FirstName = x.FirstName,
                            LastName = x.LastName,
                            DateOfHire = x.DateOfHire,
                            IsExternal = x.IsExternal,
                            IsInactive = x.IsInactive,
                            UserId = x.UserId,
                        })
                        .FirstOrDefaultAsync();
                }, new UnitOfWorkOptions { IsTransactional = false });
            });

            return cacheItem;
        }

        public async Task InvalidateCache()
        {
            var cache = GetCache();
            await cache.ClearAsync();
        }

        public async Task InvalidateCache(int driverId)
        {
            var cache = GetCache();
            await cache.RemoveAsync(driverId);
        }

        private ITypedCache<int, DriverCacheItem> GetCache()
        {
            return _cacheManager
                .GetCache(CacheName)
                .AsTyped<int, DriverCacheItem>();
        }

        public async Task HandleEventAsync(EntityUpdatedEventData<Driver> eventData)
        {
            await InvalidateCache(eventData.Entity.Id);
        }
    }
}
