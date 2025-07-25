using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Abp.Dependency;
using Abp.Domain.Repositories;
using DispatcherWeb.Caching;
using DispatcherWeb.Trucks.Dto;
using Microsoft.EntityFrameworkCore;

namespace DispatcherWeb.Trucks.Cache
{
    public class TruckListCache : ListCacheBase<ListCacheTenantKey, TruckListCacheItem, Truck>,
        ITruckListCache,
        ISingletonDependency
    {
        private readonly IRepository<Truck> _truckRepository;
        public override string CacheName => ListCacheNames.Truck;

        public TruckListCache(
            IRepository<Truck> truckRepository,
            ListCacheBaseDependency listCacheBaseDependency
        ) : base(listCacheBaseDependency)
        {
            _truckRepository = truckRepository;
        }

        protected override async Task<List<TruckListCacheItem>> GetAllItemsFromDb(ListCacheTenantKey key, DateTime? afterDateTime = null)
        {
            return await ApplyDateFilter(await _truckRepository.GetQueryAsync(), afterDateTime)
                .Select(t => new TruckListCacheItem
                {
                    Id = t.Id,
                    IsDeleted = t.IsDeleted,
                    DeletionTime = t.DeletionTime,
                    CreationTime = t.CreationTime,
                    LastModificationTime = t.LastModificationTime,
                    TruckCode = t.TruckCode,
                    OfficeId = t.OfficeId,
                    VehicleCategoryId = t.VehicleCategoryId,
                    CanPullTrailer = t.CanPullTrailer,
                    IsOutOfService = t.IsOutOfService,
                    IsActive = t.IsActive,
                    DefaultDriverId = t.DefaultDriverId,
                    BedConstruction = t.BedConstruction,
                    Year = t.Year,
                    Make = t.Make,
                    Model = t.Model,
                    IsApportioned = t.IsApportioned,
                    CurrentTrailerId = t.CurrentTrailerId,
                    AlwaysShowOnSchedule = t.AlwaysShowOnSchedule,
                    CargoCapacity = t.CargoCapacity,
                    CargoCapacityCyds = t.CargoCapacityCyds,
                })
                .ToListAsync();
        }

        protected override Task<ListCacheTenantKey> GetKeyFromEntity(Truck entity)
        {
            return Task.FromResult(new ListCacheTenantKey(entity.TenantId));
        }
    }
}
