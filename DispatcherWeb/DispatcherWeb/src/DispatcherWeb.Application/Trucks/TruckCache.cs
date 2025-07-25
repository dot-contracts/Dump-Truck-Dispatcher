using System.Linq;
using System.Threading.Tasks;
using Abp.Dependency;
using Abp.Domain.Repositories;
using Abp.Domain.Uow;
using Abp.Events.Bus.Entities;
using Abp.Events.Bus.Handlers;
using Abp.Runtime.Caching;
using DispatcherWeb.Trucks.Dto;
using Microsoft.EntityFrameworkCore;

namespace DispatcherWeb.Trucks
{
    public class TruckCache : ITruckCache, ISingletonDependency,
        IAsyncEventHandler<EntityUpdatedEventData<Truck>>
    {
        private readonly ICacheManager _cacheManager;
        private readonly IRepository<Truck> _truckRepository;
        private readonly IUnitOfWorkManager _unitOfWorkManager;
        private const string CacheName = "Truck-Cache";

        public TruckCache(
            ICacheManager cacheManager,
            IRepository<Truck> truckRepository,
            IUnitOfWorkManager unitOfWorkManager
        )
        {
            _cacheManager = cacheManager;
            _truckRepository = truckRepository;
            _unitOfWorkManager = unitOfWorkManager;
        }

        public async Task<TruckCacheItem> GetTruckFromCacheOrDbOrDefault(int truckId)
        {
            var cache = GetCache();
            var cacheItem = await cache.GetAsync(truckId, async id =>
            {
                return await _unitOfWorkManager.WithUnitOfWorkAsync(async () =>
                {
                    return await (await _truckRepository.GetQueryAsync())
                        .Where(t => t.Id == id)
                        .Select(x => new TruckCacheItem
                        {
                            Id = x.Id,
                            TruckCode = x.TruckCode,
                            LeaseHaulerId = x.LeaseHaulerTruck.LeaseHaulerId,
                            OfficeId = x.OfficeId,
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

        public async Task InvalidateCache(int truckId)
        {
            var cache = GetCache();
            await cache.RemoveAsync(truckId);
        }

        private ITypedCache<int, TruckCacheItem> GetCache()
        {
            return _cacheManager
                .GetCache(CacheName)
                .AsTyped<int, TruckCacheItem>();
        }

        public async Task HandleEventAsync(EntityUpdatedEventData<Truck> eventData)
        {
            await InvalidateCache(eventData.Entity.Id);
        }
    }
}
