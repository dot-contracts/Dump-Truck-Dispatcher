using System.Linq;
using System.Threading.Tasks;
using Abp.Dependency;
using Abp.Domain.Repositories;
using Abp.Domain.Uow;
using Abp.Events.Bus.Entities;
using Abp.Events.Bus.Handlers;
using Abp.Runtime.Caching;
using DispatcherWeb.LeaseHaulers.Dto;
using Microsoft.EntityFrameworkCore;

namespace DispatcherWeb.LeaseHaulers
{
    public class LeaseHaulerCache : ILeaseHaulerCache, ISingletonDependency,
        IAsyncEventHandler<EntityUpdatedEventData<LeaseHauler>>
    {
        private readonly ICacheManager _cacheManager;
        private readonly IRepository<LeaseHauler> _leaseHaulerRepository;
        private readonly IUnitOfWorkManager _unitOfWorkManager;
        private const string CacheName = "LeaseHauler-Cache";

        public LeaseHaulerCache(
            ICacheManager cacheManager,
            IRepository<LeaseHauler> leaseHaulerRepository,
            IUnitOfWorkManager unitOfWorkManager
        )
        {
            _cacheManager = cacheManager;
            _leaseHaulerRepository = leaseHaulerRepository;
            _unitOfWorkManager = unitOfWorkManager;
        }

        public async Task<LeaseHaulerCacheItem> GetLeaseHaulerFromCacheOrDefault(int leaseHaulerId)
        {
            var cache = GetCache();
            var cacheItem = await cache.GetAsync(leaseHaulerId, async id =>
            {
                return await _unitOfWorkManager.WithUnitOfWorkAsync(async () =>
                {
                    return await (await _leaseHaulerRepository.GetQueryAsync())
                        .Where(lh => lh.Id == id)
                        .Select(x => new LeaseHaulerCacheItem
                        {
                            Id = x.Id,
                            IsDeleted = x.IsDeleted,
                            DeletionTime = x.DeletionTime,
                            CreationTime = x.CreationTime,
                            LastModificationTime = x.LastModificationTime,
                            Name = x.Name,
                            IsActive = x.IsActive,
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

        public async Task InvalidateCache(int leaseHaulerId)
        {
            var cache = GetCache();
            await cache.RemoveAsync(leaseHaulerId);
        }

        private ITypedCache<int, LeaseHaulerCacheItem> GetCache()
        {
            return _cacheManager
                .GetCache(CacheName)
                .AsTyped<int, LeaseHaulerCacheItem>();
        }

        public async Task HandleEventAsync(EntityUpdatedEventData<LeaseHauler> eventData)
        {
            await InvalidateCache(eventData.Entity.Id);
        }
    }
}
