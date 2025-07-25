using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Abp.Authorization;
using Abp.Collections.Extensions;
using Abp.Linq.Extensions;
using Abp.Runtime.Session;
using DispatcherWeb.Caching;
using DispatcherWeb.Infrastructure.Extensions;
using DispatcherWeb.Scheduling.Dto;
using DispatcherWeb.Trucks;
using DispatcherWeb.Trucks.Dto;
using Microsoft.EntityFrameworkCore;

namespace DispatcherWeb.Scheduling
{
    [AbpAuthorize]
    public partial class SchedulingAppService
    {
        private async Task<List<ScheduleTruckDto>> GetScheduleTrucksFromCache(
            IQueryable<Truck> truckQuery,
            Func<ScheduleTruckDto, bool> cacheFilter,
            IGetScheduleInput input,
            bool useShifts,
            bool useLeaseHaulers,
            bool skipTruckFiltering = false
        )
        {
            var cachesToUse = new
            {
                _listCaches.AvailableLeaseHaulerTruck,
                _listCaches.LeaseHaulerTruck,
                _listCaches.Insurance,
                _listCaches.Truck,
                _listCaches.VehicleCategory,
                _listCaches.Driver,
                _listCaches.DriverAssignment,
                _listCaches.Order,
                _listCaches.OrderLine,
                _listCaches.OrderLineTruck,
            };

            var cachesToCheck = new IListCache[]
            {
                _listCaches.AvailableLeaseHaulerTruck,
                _listCaches.LeaseHaulerTruck,
                _listCaches.Insurance,
                _listCaches.Truck,
                _listCaches.VehicleCategory,
                _listCaches.Driver,
                _listCaches.DriverAssignment,
                _listCaches.Order,
                _listCaches.OrderLine,
                _listCaches.OrderLineTruck,
            };

            if (await cachesToCheck.AnyAsync(async c => !await c.IsEnabled()))
            {
                return await GetScheduleTrucks(
                    truckQuery,
                    input,
                    useShifts,
                    useLeaseHaulers,
                    skipTruckFiltering
                );
            }

            var shift = useShifts ? input.Shift : null;
            var dateKey = new ListCacheDateKey(await Session.GetTenantIdAsync(), input.Date, shift);
            var tenantKey = new ListCacheTenantKey(await Session.GetTenantIdAsync());
            var cache = new
            {
                AvailableLeaseHaulerTruck = await cachesToUse.AvailableLeaseHaulerTruck.GetListOrThrow(dateKey),
                LeaseHaulerTruck = await cachesToUse.LeaseHaulerTruck.GetListOrThrow(tenantKey),
                Insurances = await cachesToUse.Insurance.GetListOrThrow(tenantKey),
                Truck = await cachesToUse.Truck.GetListOrThrow(tenantKey),
                VehicleCategory = await cachesToUse.VehicleCategory.GetListOrThrow(ListCacheEmptyKey.Instance),
                Driver = await cachesToUse.Driver.GetListOrThrow(tenantKey),
                DriverAssignment = await cachesToUse.DriverAssignment.GetListOrThrow(dateKey),
                Order = await cachesToUse.Order.GetListOrThrow(dateKey),
                OrderLine = await cachesToUse.OrderLine.GetListOrThrow(dateKey),
                OrderLineTruck = await cachesToUse.OrderLineTruck.GetListOrThrow(dateKey),
            };

            var availableLeaseHaulerTrucks = cache.AvailableLeaseHaulerTruck.Items
                .WhereIf(input.OfficeId.HasValue, x => x.OfficeId == input.OfficeId)
                .OrderByDescending(x => x.Id)
                .ToList();

            if (!useLeaseHaulers)
            {
                availableLeaseHaulerTrucks.Clear();
            }

            var leaseHaulerTruckIds = availableLeaseHaulerTrucks.Select(x => x.TruckId).ToList();

            var trucks = cache.Truck.Items
                .Select(t =>
                {
                    var currentTrailer = t.CurrentTrailerId == null ? null : cache.Truck.Items.FirstOrDefault(c => c.Id == t.CurrentTrailerId);
                    var currentTrailerVehicleCategory = currentTrailer == null ? null : cache.VehicleCategory.Items.FirstOrDefault(vc => vc.Id == currentTrailer.VehicleCategoryId);
                    var leaseHaulerTruck = cache.LeaseHaulerTruck.Items.FirstOrDefault(lht => lht.TruckId == t.Id);
                    var leaseHaulerInsurances = leaseHaulerTruck?.LeaseHaulerId == null
                        ? null
                        : cache.Insurances.Items.Where(lhi => lhi.LeaseHaulerId == leaseHaulerTruck.LeaseHaulerId);
                    return new
                    {
                        Truck = t,
                        VehicleCategory = cache.VehicleCategory.Items.FirstOrDefault(vc => vc.Id == t.VehicleCategoryId),
                        LeaseHaulerTruck = leaseHaulerTruck,
                        LeaseHaulerInsurances = leaseHaulerInsurances,
                        DefaultDriver = t.DefaultDriverId == null ? null : cache.Driver.Items.FirstOrDefault(d => d.Id == t.DefaultDriverId),
                        CurrentTrailer = currentTrailer == null ? null : new
                        {
                            Trailer = currentTrailer,
                            VehicleCategory = currentTrailerVehicleCategory,
                        },
                        CurrentTractor = cache.Truck.Items.FirstOrDefault(c => c.CurrentTrailerId == t.Id),
                        OrderLineTrucks = cache.OrderLineTruck.Items
                            .Where(olt => olt.TruckId == t.Id)
                            .Select(olt => new
                            {
                                OrderLineTruck = olt,
                                OrderLine = cache.OrderLine.Items
                                    .Where(ol => ol.Id == olt.OrderLineId)
                                    .Select(ol => new
                                    {
                                        OrderLine = ol,
                                        Order = cache.Order.Items.FirstOrDefault(o => o.Id == ol.OrderId),
                                    }).FirstOrDefault(),
                            }).ToList(),
                    };
                })
                .WhereIf(!skipTruckFiltering, x =>
                    x.Truck.AlwaysShowOnSchedule
                   || x.LeaseHaulerTruck?.AlwaysShowOnSchedule == true
                   || leaseHaulerTruckIds.Contains(x.Truck.Id))
                .WhereIf(!skipTruckFiltering, x => x.Truck.IsActive)
                .WhereIf(!skipTruckFiltering && input.OfficeId.HasValue, t =>
                    t.Truck.OfficeId == input.OfficeId
                    || leaseHaulerTruckIds.Contains(t.Truck.Id))
                .Select(t => new ScheduleTruckDto
                {
                    Id = t.Truck.Id,
                    TruckCode = t.Truck.TruckCode,
                    OfficeId = t.Truck.OfficeId,
                    VehicleCategory = t.VehicleCategory == null ? null : new VehicleCategoryDto
                    {
                        Id = t.VehicleCategory.Id,
                        Name = t.VehicleCategory.Name,
                        AssetType = t.VehicleCategory.AssetType,
                        IsPowered = t.VehicleCategory.IsPowered,
                        SortOrder = t.VehicleCategory.SortOrder,
                    },
                    BedConstruction = t.Truck.BedConstruction,
                    Year = t.Truck.Year,
                    Make = t.Truck.Make,
                    Model = t.Truck.Model,
                    IsApportioned = t.Truck.IsApportioned,
                    LeaseHaulerId = t.LeaseHaulerTruck?.LeaseHaulerId,
                    AlwaysShowOnSchedule = t.LeaseHaulerTruck?.AlwaysShowOnSchedule == true,
                    CanPullTrailer = t.Truck.CanPullTrailer,
                    IsOutOfService = t.Truck.IsOutOfService,
                    Insurances = t.LeaseHaulerInsurances == null
                                 ? null
                                 : t.LeaseHaulerInsurances.Select(i => new InsuranceDto
                                 {
                                     Id = i.Id,
                                     LeaseHaulerId = i.LeaseHaulerId,
                                     ExpirationDate = i.ExpirationDate,
                                     IsActive = i.IsActive,
                                 }).ToList(),
                    IsActive = t.Truck.IsActive,
                    DefaultDriverId = t.Truck.DefaultDriverId,
                    DefaultDriverName = t.DefaultDriver?.FirstName + " " + t.DefaultDriver?.LastName,
                    DefaultDriverDateOfHire = t.DefaultDriver?.DateOfHire,
                    Trailer = t.CurrentTrailer == null ? null : new ScheduleTruckTrailerDto
                    {
                        Id = t.CurrentTrailer.Trailer.Id,
                        TruckCode = t.CurrentTrailer.Trailer.TruckCode,
                        VehicleCategory = t.CurrentTrailer.VehicleCategory == null ? null : new VehicleCategoryDto
                        {
                            Id = t.CurrentTrailer.VehicleCategory.Id,
                            Name = t.CurrentTrailer.VehicleCategory.Name,
                            AssetType = t.CurrentTrailer.VehicleCategory.AssetType,
                            IsPowered = t.CurrentTrailer.VehicleCategory.IsPowered,
                            SortOrder = t.CurrentTrailer.VehicleCategory.SortOrder,
                        },
                        Year = t.CurrentTrailer.Trailer.Year,
                        Make = t.CurrentTrailer.Trailer.Make,
                        Model = t.CurrentTrailer.Trailer.Model,
                        BedConstruction = t.CurrentTrailer.Trailer.BedConstruction,
                    },
                    Tractor = t.CurrentTractor == null ? null : new ScheduleTruckTractorDto
                    {
                        Id = t.CurrentTractor.Id,
                        TruckCode = t.CurrentTractor.TruckCode,
                    },
                    UtilizationList = t.OrderLineTrucks
                        .Where(olt => olt.OrderLine?.OrderLine.IsComplete == false
                                && olt.OrderLine.Order?.IsPending == false
                                && (!input.OfficeId.HasValue || olt.OrderLine.Order.OfficeId == input.OfficeId))
                        .Select(olt => olt.OrderLineTruck.Utilization)
                        .ToList(),
                }).ToList();

            var driverAssignments = cache.DriverAssignment.Items
                .OrderByDescending(x => x.Id)
                .ToList();

            foreach (var truck in trucks)
            {
                var driverAssignment = driverAssignments.FirstOrDefault(x => x.TruckId == truck.Id);
                var leaseHaulerTruck = availableLeaseHaulerTrucks.FirstOrDefault(x => x.TruckId == truck.Id);
                if (leaseHaulerTruck != null)
                {
                    var driver = cache.Driver.Items.FirstOrDefault(d => d.Id == leaseHaulerTruck.DriverId);

                    truck.DriverId = leaseHaulerTruck.DriverId;
                    truck.DriverName = driver == null ? null : driver.FirstName + " " + driver.LastName;
                    truck.DriverDateOfHire = driver?.DateOfHire;
                    truck.OfficeId = leaseHaulerTruck.OfficeId;
                    truck.IsExternal = true;
                    truck.LeaseHaulerId = leaseHaulerTruck.LeaseHaulerId;
                }
                else if (driverAssignment != null)
                {
                    if (driverAssignment.DriverId != null)
                    {
                        var driver = cache.Driver.Items.FirstOrDefault(d => d.Id == driverAssignment.DriverId);
                        truck.DriverId = driverAssignment.DriverId;
                        truck.DriverName = driver == null ? null : driver.FirstName + " " + driver.LastName;
                        truck.DriverDateOfHire = driver?.DateOfHire;
                    }
                    else
                    {
                        //we intentionally don't fall back to defaultDriverId here. If driverId is explicitly set to null in a driver assignment, use that instead of default driver
                        truck.DriverId = null;
                        truck.DriverName = "[No driver]";
                    }
                }
                else if (truck.DefaultDriverId.HasValue)
                {
                    truck.DriverId = truck.DefaultDriverId;
                    truck.DriverName = truck.DefaultDriverName;
                    truck.DriverDateOfHire = truck.DefaultDriverDateOfHire;
                }
                else
                {
                    truck.DriverId = null;
                    truck.DriverName = "[No driver]";
                }

                if (!truck.IsExternal)
                {
                    truck.HasNoDriver = driverAssignment != null && driverAssignment.DriverId == null;
                    truck.HasDriverAssignment = driverAssignment?.DriverId != null;
                }

                truck.Utilization = !truck.VehicleCategory.IsPowered
                        //previous trailer utilization logic
                        //? (t.UtilizationList.Any() ? 1 : 0)
                        ? 0
                        : truck.UtilizationList.Sum();

                truck.ActualUtilization = truck.Utilization;
            }

            return trucks
                .Where(cacheFilter)
                .OrderByTruck();
        }

        private async Task<List<ScheduleTruckDto>> GetScheduleTrucks(
            IQueryable<Truck> truckQuery,
            IGetScheduleInput input,
            bool useShifts,
            bool useLeaseHaulers,
            bool skipTruckFiltering = false
        )
        {
            var leaseHaulerTrucks = await truckQuery
                .Where(x => useLeaseHaulers && x.OfficeId == null)
                .SelectMany(x => x.AvailableLeaseHaulerTrucks)
                .WhereIf(input.OfficeId.HasValue, x => x.OfficeId == input.OfficeId)
                .WhereIf(useShifts, x => x.Shift == input.Shift)
                .Where(x => x.Date == input.Date)
                .Select(x => new
                {
                    x.Id,
                    x.LeaseHaulerId,
                    x.TruckId,
                    x.DriverId,
                    DriverName = x.Driver.FirstName + " " + x.Driver.LastName,
                    DriverDateOfHire = x.Driver.DateOfHire,
                    x.OfficeId,
                })
                .OrderByDescending(x => x.Id)
                .ToListAsync();

            var leaseHaulerTruckIds = leaseHaulerTrucks.Select(x => x.TruckId).ToList();

            var trucks = await truckQuery
                .WhereIf(!skipTruckFiltering, t => t.AlwaysShowOnSchedule
                    || t.LeaseHaulerTruck.AlwaysShowOnSchedule == true
                    || leaseHaulerTruckIds.Contains(t.Id))
                .WhereIf(!skipTruckFiltering, t => t.IsActive)
                .WhereIf(!skipTruckFiltering && input.OfficeId.HasValue, t =>
                    t.OfficeId == input.OfficeId
                    || leaseHaulerTruckIds.Contains(t.Id))
                .Select(t => new ScheduleTruckDto
                {
                    Id = t.Id,
                    TruckCode = t.TruckCode,
                    OfficeId = t.OfficeId,
                    VehicleCategory = new VehicleCategoryDto
                    {
                        Id = t.VehicleCategory.Id,
                        Name = t.VehicleCategory.Name,
                        AssetType = t.VehicleCategory.AssetType,
                        IsPowered = t.VehicleCategory.IsPowered,
                        SortOrder = t.VehicleCategory.SortOrder,
                    },
                    BedConstruction = t.BedConstruction,
                    Year = t.Year,
                    Make = t.Make,
                    Model = t.Model,
                    IsApportioned = t.IsApportioned,
                    LeaseHaulerId = t.LeaseHaulerTruck.LeaseHaulerId,
                    AlwaysShowOnSchedule = t.LeaseHaulerTruck.AlwaysShowOnSchedule == true,
                    CanPullTrailer = t.CanPullTrailer,
                    IsOutOfService = t.IsOutOfService,
                    Insurances = t.LeaseHaulerTruck == null
                                 ? null
                                 : t.LeaseHaulerTruck.LeaseHauler.LeaseHaulerInsurances.Select(i => new InsuranceDto
                                 {
                                     Id = i.Id,
                                     LeaseHaulerId = i.LeaseHaulerId,
                                     ExpirationDate = i.ExpirationDate,
                                     IsActive = i.IsActive,
                                 }).ToList(),
                    IsActive = t.IsActive,
                    DefaultDriverId = t.DefaultDriverId,
                    DefaultDriverName = t.DefaultDriver.FirstName + " " + t.DefaultDriver.LastName,
                    DefaultDriverDateOfHire = t.DefaultDriver.DateOfHire,
                    Trailer = t.CurrentTrailer == null ? null : new ScheduleTruckTrailerDto
                    {
                        Id = t.CurrentTrailer.Id,
                        TruckCode = t.CurrentTrailer.TruckCode,
                        VehicleCategory = new VehicleCategoryDto
                        {
                            Id = t.CurrentTrailer.VehicleCategory.Id,
                            Name = t.CurrentTrailer.VehicleCategory.Name,
                            AssetType = t.CurrentTrailer.VehicleCategory.AssetType,
                            IsPowered = t.CurrentTrailer.VehicleCategory.IsPowered,
                            SortOrder = t.CurrentTrailer.VehicleCategory.SortOrder,
                        },
                        Year = t.CurrentTrailer.Year,
                        Make = t.CurrentTrailer.Make,
                        Model = t.CurrentTrailer.Model,
                        BedConstruction = t.CurrentTrailer.BedConstruction,
                    },
                    Tractor = t.CurrentTractors.Select(x => new ScheduleTruckTractorDto
                    {
                        Id = x.Id,
                        TruckCode = x.TruckCode,
                    }).FirstOrDefault(),
                    UtilizationList = t.OrderLineTrucksOfTruck
                        .Where(olt => !olt.OrderLine.IsComplete
                                && olt.OrderLine.Order.DeliveryDate == input.Date
                                && olt.OrderLine.Order.Shift == input.Shift
                                && !olt.OrderLine.Order.IsPending
                                && (!input.OfficeId.HasValue || olt.OrderLine.Order.OfficeId == input.OfficeId))
                        .Select(olt => olt.Utilization)
                        .ToList(),
                })
                .ToListAsync();

            var driverAssignments = await (await _driverAssignmentRepository.GetQueryAsync())
                .Where(x => x.Date == input.Date && x.Shift == input.Shift)
                .Select(x => new
                {
                    x.Id,
                    x.TruckId,
                    x.DriverId,
                    DriverName = x.Driver.FirstName + " " + x.Driver.LastName,
                    DriverDateOfHire = x.Driver.DateOfHire,
                })
                .OrderByDescending(x => x.Id)
                .ToListAsync();

            foreach (var truck in trucks)
            {
                var driverAssignment = driverAssignments.FirstOrDefault(x => x.TruckId == truck.Id);
                var leaseHaulerTruck = leaseHaulerTrucks.FirstOrDefault(x => x.TruckId == truck.Id);
                if (leaseHaulerTruck != null)
                {
                    truck.DriverId = leaseHaulerTruck.DriverId;
                    truck.DriverName = leaseHaulerTruck.DriverName;
                    truck.DriverDateOfHire = leaseHaulerTruck.DriverDateOfHire;
                    truck.OfficeId = leaseHaulerTruck.OfficeId;
                    truck.IsExternal = true;
                    truck.LeaseHaulerId = leaseHaulerTruck.LeaseHaulerId;
                }
                else if (driverAssignment != null)
                {
                    if (driverAssignment.DriverId != null)
                    {
                        truck.DriverId = driverAssignment.DriverId;
                        truck.DriverName = driverAssignment.DriverName;
                        truck.DriverDateOfHire = driverAssignment.DriverDateOfHire;
                    }
                    else
                    {
                        //we intentionally don't fall back to defaultDriverId here. If driverId is explicitly set to null in a driver assignment, use that instead of default driver
                        truck.DriverId = null;
                        truck.DriverName = "[No driver]";
                    }
                }
                else if (truck.DefaultDriverId.HasValue)
                {
                    truck.DriverId = truck.DefaultDriverId;
                    truck.DriverName = truck.DefaultDriverName;
                    truck.DriverDateOfHire = truck.DefaultDriverDateOfHire;
                }
                else
                {
                    truck.DriverId = null;
                    truck.DriverName = "[No driver]";
                }

                if (!truck.IsExternal)
                {
                    truck.HasNoDriver = driverAssignment != null && driverAssignment.DriverId == null;
                    truck.HasDriverAssignment = driverAssignment?.DriverId != null;
                }

                truck.Utilization = !truck.VehicleCategory.IsPowered
                        //previous trailer utilization logic
                        //? (t.UtilizationList.Any() ? 1 : 0)
                        ? 0
                        : truck.UtilizationList.Sum();

                truck.ActualUtilization = truck.Utilization;
            }

            return trucks.OrderByTruck();
        }
    }
}
