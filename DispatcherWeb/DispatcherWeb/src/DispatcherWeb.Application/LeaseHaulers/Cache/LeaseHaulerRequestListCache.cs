using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Abp.Dependency;
using Abp.Domain.Repositories;
using DispatcherWeb.Caching;
using DispatcherWeb.LeaseHaulerRequests;
using DispatcherWeb.LeaseHaulers.Dto;
using Microsoft.EntityFrameworkCore;

namespace DispatcherWeb.LeaseHaulers.Cache
{
    public class LeaseHaulerRequestListCache : ListCacheBase<ListCacheDateKey, LeaseHaulerRequestCacheItem, LeaseHaulerRequest>,
        ILeaseHaulerRequestListCache,
        ISingletonDependency
    {
        private readonly IRepository<LeaseHaulerRequest> _leaseHaulerRequestRepository;
        public override string CacheName => ListCacheNames.LeaseHaulerRequest;

        public LeaseHaulerRequestListCache(
            IRepository<LeaseHaulerRequest> leaseHaulerRequestRepository,
            ListCacheBaseDependency listCacheBaseDependency
        ) : base(listCacheBaseDependency)
        {
            _leaseHaulerRequestRepository = leaseHaulerRequestRepository;
        }

        protected override async Task<List<LeaseHaulerRequestCacheItem>> GetAllItemsFromDb(ListCacheDateKey key, DateTime? afterDateTime = null)
        {
            return await ApplyDateFilter(await _leaseHaulerRequestRepository.GetQueryAsync(), afterDateTime)
                .Where(x => x.TenantId == key.TenantId
                            && x.OrderLine.Order.DeliveryDate == key.Date
                            && (x.OrderLine.Order.Shift == key.Shift || key.Shift == null))
                .Select(x => new LeaseHaulerRequestCacheItem
                {
                    Id = x.Id,
                    IsDeleted = x.IsDeleted,
                    DeletionTime = x.DeletionTime,
                    CreationTime = x.CreationTime,
                    LastModificationTime = x.LastModificationTime,
                    OrderLineId = x.OrderLineId,
                    LeaseHaulerId = x.LeaseHaulerId,
                    NumberTrucksRequested = x.NumberTrucksRequested,
                    Status = x.Status,
                })
                .ToListAsync();
        }

        protected override async Task<ListCacheDateKey> GetKeyFromEntity(LeaseHaulerRequest entity)
        {
            if (entity.OrderLineId == null)
            {
                return null;
            }
            if (await DateKeyLookup.IsEnabled())
            {
                return await DateKeyLookup.GetKeyForOrderLine(entity.OrderLineId.Value);
            }

            var keyData = await WithUnitOfWorkAsync(entity, async () =>
            {
                return await (await _leaseHaulerRequestRepository.GetQueryAsync())
                    .Where(x => x.Id == entity.Id)
                    .Select(x => new
                    {
                        x.OrderLine.Order.DeliveryDate,
                        x.OrderLine.Order.Shift,
                    }).FirstAsync();
            });

            var key = new ListCacheDateKey(entity.TenantId, keyData.DeliveryDate, keyData.Shift);

            return key;
        }
    }
}
