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
    public class RequestedLeaseHaulerTruckListCache : ListCacheBase<ListCacheDateKey, RequestedLeaseHaulerTruckCacheItem, RequestedLeaseHaulerTruck>,
        IRequestedLeaseHaulerTruckListCache,
        ISingletonDependency
    {
        private readonly IRepository<RequestedLeaseHaulerTruck> _requestedLeaseHaulerTruckRepository;
        public override string CacheName => ListCacheNames.RequestedLeaseHaulerTruck;

        public RequestedLeaseHaulerTruckListCache(
            IRepository<RequestedLeaseHaulerTruck> requestedLeaseHaulerTruckRepository,
            ListCacheBaseDependency listCacheBaseDependency
        ) : base(listCacheBaseDependency)
        {
            _requestedLeaseHaulerTruckRepository = requestedLeaseHaulerTruckRepository;
        }

        protected override async Task<List<RequestedLeaseHaulerTruckCacheItem>> GetAllItemsFromDb(ListCacheDateKey key, DateTime? afterDateTime = null)
        {
            return await ApplyDateFilter(await _requestedLeaseHaulerTruckRepository.GetQueryAsync(), afterDateTime)
                .Where(x => x.TenantId == key.TenantId
                            && x.LeaseHaulerRequest.OrderLine.Order.DeliveryDate == key.Date
                            && (x.LeaseHaulerRequest.OrderLine.Order.Shift == key.Shift || key.Shift == null))
                .Select(x => new RequestedLeaseHaulerTruckCacheItem
                {
                    Id = x.Id,
                    IsDeleted = x.IsDeleted,
                    DeletionTime = x.DeletionTime,
                    CreationTime = x.CreationTime,
                    LastModificationTime = x.LastModificationTime,
                    LeaseHaulerRequestId = x.LeaseHaulerRequestId,
                    TruckId = x.TruckId,
                    DriverId = x.DriverId,
                })
                .ToListAsync();
        }

        protected override async Task<ListCacheDateKey> GetKeyFromEntity(RequestedLeaseHaulerTruck entity)
        {
            if (await DateKeyLookup.IsEnabled())
            {
                return await DateKeyLookup.GetKeyForLeaseHaulerRequest(entity.LeaseHaulerRequestId);
            }

            var keyData = await WithUnitOfWorkAsync(entity, async () =>
            {
                return await (await _requestedLeaseHaulerTruckRepository.GetQueryAsync())
                    .Where(x => x.Id == entity.Id)
                    .Select(x => new
                    {
                        x.LeaseHaulerRequest.OrderLine.Order.DeliveryDate,
                        x.LeaseHaulerRequest.OrderLine.Order.Shift,
                    }).FirstAsync();
            });

            var key = new ListCacheDateKey(entity.TenantId, keyData.DeliveryDate, keyData.Shift);

            return key;
        }
    }
}
