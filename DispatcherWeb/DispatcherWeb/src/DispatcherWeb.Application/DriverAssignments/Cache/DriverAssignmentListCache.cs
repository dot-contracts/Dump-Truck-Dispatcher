using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Abp.Dependency;
using Abp.Domain.Repositories;
using DispatcherWeb.Caching;
using DispatcherWeb.DriverAssignments.Dto;
using DispatcherWeb.Drivers;
using Microsoft.EntityFrameworkCore;

namespace DispatcherWeb.DriverAssignments.Cache
{
    public class DriverAssignmentListCache : ListCacheBase<ListCacheDateKey, DriverAssignmentCacheItem, DriverAssignment>,
        IDriverAssignmentListCache,
        ISingletonDependency
    {
        private readonly IRepository<DriverAssignment> _repository;
        public override string CacheName => ListCacheNames.DriverAssignment;

        public DriverAssignmentListCache(
            IRepository<DriverAssignment> repository,
            ListCacheBaseDependency listCacheBaseDependency
        ) : base(listCacheBaseDependency)
        {
            _repository = repository;
        }

        protected override async Task<List<DriverAssignmentCacheItem>> GetAllItemsFromDb(ListCacheDateKey key, DateTime? afterDateTime = null)
        {
            return await ApplyDateFilter(await _repository.GetQueryAsync(), afterDateTime)
                .Where(x => x.TenantId == key.TenantId
                    && x.Date == key.Date
                    && (x.Shift == key.Shift || key.Shift == null))
                .Select(x => new DriverAssignmentCacheItem
                {
                    Id = x.Id,
                    IsDeleted = x.IsDeleted,
                    DeletionTime = x.DeletionTime,
                    CreationTime = x.CreationTime,
                    LastModificationTime = x.LastModificationTime,
                    Shift = x.Shift,
                    StartTime = x.StartTime,
                    OfficeId = x.OfficeId,
                    TruckId = x.TruckId,
                    DriverId = x.DriverId,
                })
                .ToListAsync();
        }

        protected override Task<ListCacheDateKey> GetKeyFromEntity(DriverAssignment entity)
        {
            return Task.FromResult(new ListCacheDateKey(entity.TenantId, entity.Date, entity.Shift));
        }
    }
}
