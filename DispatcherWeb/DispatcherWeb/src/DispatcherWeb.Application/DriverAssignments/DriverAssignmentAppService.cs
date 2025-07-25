using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Threading.Tasks;
using Abp.Application.Services.Dto;
using Abp.Authorization;
using Abp.Collections.Extensions;
using Abp.Domain.Repositories;
using Abp.Linq.Extensions;
using Abp.Runtime.Session;
using Abp.Timing;
using Abp.UI;
using DispatcherWeb.Authorization;
using DispatcherWeb.Caching;
using DispatcherWeb.Dispatching;
using DispatcherWeb.DriverAssignments.Dto;
using DispatcherWeb.Drivers;
using DispatcherWeb.Infrastructure.Extensions;
using DispatcherWeb.Infrastructure.RepositoryExtensions;
using DispatcherWeb.LeaseHaulerRequests;
using DispatcherWeb.Orders;
using DispatcherWeb.SyncRequests;
using DispatcherWeb.TimeOffs;
using DispatcherWeb.Trucks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DispatcherWeb.DriverAssignments
{
    [AbpAuthorize]
    public class DriverAssignmentAppService : DispatcherWebAppServiceBase, IDriverAssignmentAppService
    {
        private readonly IRepository<DriverAssignment> _driverAssignmentRepository;
        private readonly IRepository<Truck> _truckRepository;
        private readonly ITruckCache _truckCache;
        private readonly IRepository<Driver> _driverRepository;
        private readonly IRepository<OrderLineTruck> _orderLineTruckRepository;
        private readonly IRepository<Dispatch> _dispatchRepository;
        private readonly IRepository<TimeOff> _timeOffRepository;
        private readonly IRepository<AvailableLeaseHaulerTruck> _availableLeaseHaulerTruckRepository;
        private readonly IOrderLineUpdaterFactory _orderLineUpdaterFactory;
        private readonly ListCacheCollection _listCaches;
        private readonly ISyncRequestSender _syncRequestSender;

        public DriverAssignmentAppService(
            IRepository<DriverAssignment> driverAssignmentRepository,
            IRepository<Truck> truckRepository,
            ITruckCache truckCache,
            IRepository<Driver> driverRepository,
            IRepository<OrderLineTruck> orderLineTruckRepository,
            IRepository<Dispatch> dispatchRepository,
            IRepository<TimeOff> timeOffRepository,
            IRepository<AvailableLeaseHaulerTruck> availableLeaseHaulerTruckRepository,
            IOrderLineUpdaterFactory orderLineUpdaterFactory,
            ListCacheCollection listCaches,
            ISyncRequestSender syncRequestSender
            )
        {
            _driverAssignmentRepository = driverAssignmentRepository;
            _truckRepository = truckRepository;
            _truckCache = truckCache;
            _driverRepository = driverRepository;
            _orderLineTruckRepository = orderLineTruckRepository;
            _dispatchRepository = dispatchRepository;
            _syncRequestSender = syncRequestSender;
            _timeOffRepository = timeOffRepository;
            _availableLeaseHaulerTruckRepository = availableLeaseHaulerTruckRepository;
            _orderLineUpdaterFactory = orderLineUpdaterFactory;
            _listCaches = listCaches;
        }

        [AbpAuthorize(AppPermissions.Pages_DriverAssignment, AppPermissions.LeaseHaulerPortal_Jobs_View)]
        [HttpPost]
        public async Task<ListResultDto<DriverAssignmentLiteDto>> GetAllDriverAssignmentsLite(GetDriverAssignmentsInput input)
        {
            var filterByLeaseHaulerId = await GetLeaseHaulerIdFilterAsync(AppPermissions.Pages_DriverAssignment, AppPermissions.LeaseHaulerPortal_Jobs_View);

            var cachesToUse = new
            {
                _listCaches.DriverAssignment,
                _listCaches.Driver,
                _listCaches.Truck,
                _listCaches.LeaseHaulerDriver,
                _listCaches.LeaseHaulerTruck,
            };

            var cachesToCheck = new IListCache[]
            {
                _listCaches.DriverAssignment,
                _listCaches.Driver,
                _listCaches.Truck,
                _listCaches.LeaseHaulerDriver,
                _listCaches.LeaseHaulerTruck,
            };

            List<DriverAssignmentLiteDto> items;

            if (await cachesToCheck.AnyAsync(async c => !await c.IsEnabled()))
            {
                items = await GetAllDriverAssignmentsLiteFromDb(input, filterByLeaseHaulerId);
            }
            else
            {
                var shift = input.Shift == Shift.NoShift ? null : input.Shift;
                var dateKey = new ListCacheDateKey(await Session.GetTenantIdAsync(), input.Date, shift);
                var tenantKey = new ListCacheTenantKey(await Session.GetTenantIdAsync());
                var cache = new
                {
                    DriverAssignment = await cachesToUse.DriverAssignment.GetListOrThrow(dateKey),
                    Driver = await cachesToUse.Driver.GetListOrThrow(tenantKey),
                    Truck = await cachesToUse.Truck.GetListOrThrow(tenantKey),
                    LeaseHaulerDriver = await cachesToUse.LeaseHaulerDriver.GetListOrThrow(tenantKey),
                    LeaseHaulerTruck = await cachesToUse.LeaseHaulerTruck.GetListOrThrow(tenantKey),
                };

                items = cache.DriverAssignment.Items
                    .WhereIf(input.OfficeId.HasValue, da => da.OfficeId == input.OfficeId)
                    .WhereIf(input.TruckId.HasValue, da => da.TruckId == input.TruckId)
                    .Select(x =>
                    {
                        var driver = cache.Driver.Find(x.DriverId);
                        var truck = cache.Truck.Find(x.TruckId);
                        var leaseHaulerTruck = truck == null ? null : cache.LeaseHaulerTruck.Items.FirstOrDefault(t => t.TruckId == truck.Id);
                        var leaseHaulerDriver = driver == null ? null : cache.LeaseHaulerDriver.Items.FirstOrDefault(d => d.DriverId == driver.Id);

                        if (truck?.OfficeId == null
                            || filterByLeaseHaulerId.HasValue && leaseHaulerDriver?.LeaseHaulerId != filterByLeaseHaulerId)
                        {
                            return null;
                        }

                        return new DriverAssignmentLiteDto
                        {
                            Id = x.Id,
                            OfficeId = x.OfficeId,
                            Shift = x.Shift,
                            Date = input.Date,
                            TruckId = x.TruckId,
                            TruckCode = truck?.TruckCode,
                            TruckLeaseHaulerId = leaseHaulerTruck?.LeaseHaulerId,
                            DriverId = x.DriverId,
                            DriverName = driver?.FirstName + " " + driver?.LastName,
                            DriverFirstName = driver?.FirstName,
                            DriverLastName = driver?.LastName,
                            DriverIsExternal = driver?.IsExternal == true,
                            DriverIsActive = driver?.IsInactive != true,
                            StartTime = x.StartTime,
                        };
                    })
                    .Where(x => x != null)
                    .ToList();
            }

            var timezone = await GetTimezone();
            items.ForEach(x => x.StartTime = x.StartTime?.ConvertTimeZoneTo(timezone));

            return new ListResultDto<DriverAssignmentLiteDto>(items);
        }

        private async Task<List<DriverAssignmentLiteDto>> GetAllDriverAssignmentsLiteFromDb(GetDriverAssignmentsInput input, int? filterByLeaseHaulerId)
        {
            var query = (await _driverAssignmentRepository.GetQueryAsync())
                .WhereIf(input.OfficeId.HasValue, da => da.OfficeId == input.OfficeId)
                .Where(da => da.Date == input.Date && da.Truck.OfficeId.HasValue)
                .WhereIf(input.Shift.HasValue && input.Shift != Shift.NoShift, da => da.Shift == input.Shift.Value)
                .WhereIf(input.Shift.HasValue && input.Shift == Shift.NoShift, da => da.Shift == null)
                .WhereIf(input.TruckId.HasValue, da => da.TruckId == input.TruckId)
                .WhereIf(filterByLeaseHaulerId.HasValue, q => q.Driver.LeaseHaulerDriver.LeaseHaulerId == filterByLeaseHaulerId);

            var items = await query
                .Select(x => new DriverAssignmentLiteDto
                {
                    Id = x.Id,
                    OfficeId = x.OfficeId,
                    Shift = x.Shift,
                    Date = x.Date,
                    TruckId = x.TruckId,
                    TruckCode = x.Truck.TruckCode,
                    TruckLeaseHaulerId = x.Truck.LeaseHaulerTruck.LeaseHaulerId,
                    DriverId = x.DriverId,
                    DriverName = x.Driver.FirstName + " " + x.Driver.LastName,
                    DriverFirstName = x.Driver.FirstName,
                    DriverLastName = x.Driver.LastName,
                    DriverIsExternal = x.Driver.IsExternal == true,
                    DriverIsActive = x.Driver.IsInactive != true,
                    StartTime = x.StartTime,
                })
                .ToListAsync();

            return items;
        }

        [AbpAuthorize(AppPermissions.Pages_DriverAssignment)]
        [HttpPost]
        public async Task<ListResultDto<DriverAssignmentDto>> GetDriverAssignments(GetDriverAssignmentsInput input)
        {
            var itemsLite = (await GetAllDriverAssignmentsLite(input)).Items;
            var totalCount = itemsLite.Count;

            var items = itemsLite.Select(x => x.CopyTo(new DriverAssignmentDto())).ToList();
            await FillFirstTimeOnJob(items, input);

            items = items
                .AsQueryable()
                .OrderBy(input.Sorting)
                .PageBy(input)
                .ToList();

            return new PagedResultDto<DriverAssignmentDto>(
                totalCount,
                items);
        }

        [AbpAuthorize(AppPermissions.Pages_Schedule)]
        public async Task SetNoDriverForTruck(SetNoDriverForTruckInput input)
        {
            if (input.StartDate < await GetToday())
            {
                throw new UserFriendlyException("Cannot set driver for date in past");
            }

            input.StartDate = input.StartDate.Date;
            input.EndDate = input.EndDate.Date;

            if (input.EndDate < input.StartDate)
            {
                throw new UserFriendlyException("End Date should be greater than Start Date");
            }

            var truck = await _truckCache.GetTruckFromCacheOrDbOrDefault(input.TruckId);

            var existingAssignments = await (await _driverAssignmentRepository.GetQueryAsync())
                .Where(da => da.Date >= input.StartDate && da.Date <= input.EndDate && da.Shift == input.Shift && da.TruckId == input.TruckId)
                .ToListAsync();

            var today = await GetToday();
            var syncRequest = new SyncRequest();

            var dispatchesToCancel = await (await _dispatchRepository.GetQueryAsync())
                .Where(d => d.TruckId == input.TruckId
                    && d.OrderLine.Order.DeliveryDate >= input.StartDate
                    && d.OrderLine.Order.DeliveryDate <= input.EndDate
                    && d.OrderLine.Order.Shift == input.Shift
                    && Dispatch.UnacknowledgedStatuses.Contains(d.Status))
                .ToListAsync();
            dispatchesToCancel.ForEach(d =>
            {
                d.Status = DispatchStatus.Canceled;
                d.Canceled = Clock.Now;
            });
            await CurrentUnitOfWork.SaveChangesAsync();
            if (dispatchesToCancel.Any())
            {
                syncRequest
                    .AddChanges(EntityEnum.Dispatch, dispatchesToCancel.Select(x => x.ToChangedEntity()), ChangeType.Removed)
                    .AddLogMessage("Canceled unacknowledged dispatches after no driver was set for the truck");
            }

            var orderLineIdsNeedingStaggeredTimeRecalculation = new List<int>();

            var date = input.StartDate;
            while (date <= input.EndDate)
            {
                var existingAssignmentsForDay = existingAssignments.Where(x => x.Date == date && x.Shift == input.Shift).ToList();
                if (existingAssignmentsForDay.Any())
                {
                    var firstDriverAssignment = true;
                    foreach (var existingAssignment in existingAssignmentsForDay)
                    {
                        var oldDriverId = existingAssignment.DriverId;
                        if (firstDriverAssignment)
                        {
                            existingAssignment.DriverId = null;
                            existingAssignment.OfficeId = truck.OfficeId;
                        }
                        else
                        {
                            await _driverAssignmentRepository.DeleteAsync(existingAssignment);
                        }
                        syncRequest.AddChange(EntityEnum.DriverAssignment,
                            existingAssignment
                                .ToChangedEntity()
                                .SetOldDriverIdToNotify(oldDriverId),
                            changeType: firstDriverAssignment ? ChangeType.Modified : ChangeType.Removed);

                        firstDriverAssignment = false;
                    }
                }
                else
                {
                    var newDriverAssignment = new DriverAssignment
                    {
                        OfficeId = truck.OfficeId,
                        Date = date,
                        Shift = input.Shift,
                        TruckId = input.TruckId,
                        DriverId = null,
                    };
                    await _driverAssignmentRepository.InsertAsync(newDriverAssignment);
                    existingAssignments.Add(newDriverAssignment);
                    syncRequest.AddChange(EntityEnum.DriverAssignment,
                        newDriverAssignment
                            .ToChangedEntity());
                }

                var orderLineTrucksToDelete = await (await _orderLineTruckRepository.GetQueryAsync())
                    // ReSharper disable once AccessToModifiedClosure
                    .Where(olt => olt.TruckId == input.TruckId && olt.OrderLine.Order.DeliveryDate == date && olt.OrderLine.Order.Shift == input.Shift)
                    .ToListAsync();
                foreach (var orderLineTruck in orderLineTrucksToDelete)
                {
                    await _orderLineTruckRepository.DeleteAsync(orderLineTruck);
                    if (date >= today)
                    {
                        orderLineIdsNeedingStaggeredTimeRecalculation.Add(orderLineTruck.OrderLineId);
                    }
                }

                date = date.AddDays(1);
            }

            if (orderLineIdsNeedingStaggeredTimeRecalculation.Any())
            {
                await CurrentUnitOfWork.SaveChangesAsync();
                foreach (var orderLineId in orderLineIdsNeedingStaggeredTimeRecalculation.Distinct().ToList())
                {
                    var orderLineUpdater = _orderLineUpdaterFactory.Create(orderLineId);
                    orderLineUpdater.UpdateStaggeredTimeOnTrucksOnSave();
                    await orderLineUpdater.SaveChangesAsync();
                }
            }

            await CurrentUnitOfWork.SaveChangesAsync();

            await _syncRequestSender.SendSyncRequest(syncRequest.AddLogMessage("Set no driver for truck"));
        }

        [AbpAuthorize(AppPermissions.Pages_Schedule)]
        public async Task<bool> ThereAreOrdersForTruckOnDate(ThereAreOrdersForTruckOnDateInput input)
        {
            input.StartDate = input.StartDate.Date;
            input.EndDate = input.EndDate.Date;

            var hasOrderLineTrucks = await (await _orderLineTruckRepository.GetQueryAsync())
                .Where(olt => olt.TruckId == input.TruckId
                    && olt.OrderLine.Order.DeliveryDate >= input.StartDate
                    && olt.OrderLine.Order.DeliveryDate <= input.EndDate
                    && olt.OrderLine.Order.Shift == input.Shift)
                .AnyAsync();

            return hasOrderLineTrucks;
        }

        public async Task<ThereAreOpenDispatchesForTruckOnDateResult> ThereAreOpenDispatchesForTruckOnDate(ThereAreOpenDispatchesForTruckOnDateInput input)
        {
            var dispatchStatuses = await (await _dispatchRepository.GetQueryAsync())
                .Where(d => d.TruckId == input.TruckId
                            && d.OrderLine.Order.DeliveryDate >= input.StartDate
                            && d.OrderLine.Order.DeliveryDate <= input.EndDate
                            && d.OrderLine.Order.Shift == input.Shift
                            && !Dispatch.ClosedDispatchStatuses.Contains(d.Status))
                .GroupBy(d => d.Status)
                .Select(d => d.Key)
                .ToListAsync();
            return new ThereAreOpenDispatchesForTruckOnDateResult
            {
                ThereAreUnacknowledgedDispatches = dispatchStatuses.Any(ds => ds == DispatchStatus.Created || ds == DispatchStatus.Sent),
                ThereAreAcknowledgedDispatches = dispatchStatuses.Any(ds => ds == DispatchStatus.Acknowledged || ds == DispatchStatus.Loaded),
            };
        }

        public async Task<ThereAreOpenDispatchesForDriverOnDateResult> ThereAreOpenDispatchesForDriverOnDate(ThereAreOpenDispatchesForDriverOnDateInput input)
        {
            var dispatchStatuses = await (await _dispatchRepository.GetQueryAsync())
                .Where(d => d.DriverId == input.DriverId
                            && d.OrderLine.Order.DeliveryDate >= input.StartDate
                            && d.OrderLine.Order.DeliveryDate <= input.EndDate
                            && !Dispatch.ClosedDispatchStatuses.Contains(d.Status))
                .GroupBy(d => d.Status)
                .Select(d => d.Key)
                .ToListAsync();
            return new ThereAreOpenDispatchesForDriverOnDateResult
            {
                ThereAreUnacknowledgedDispatches = dispatchStatuses.Any(ds => ds == DispatchStatus.Created || ds == DispatchStatus.Sent),
                ThereAreAcknowledgedDispatches = dispatchStatuses.Any(ds => ds == DispatchStatus.Acknowledged || ds == DispatchStatus.Loaded),
            };
        }

        private async Task ThrowIfDriverHasTimeOffRequests(int driverId, DateTime startDate, DateTime endDate)
        {
            if (await (await _timeOffRepository.GetQueryAsync())
                    .AnyAsync(x => x.DriverId == driverId && startDate <= x.EndDate && endDate >= x.StartDate))
            {
                throw new UserFriendlyException(L("DriverCantBeAssignedOnDayOff"));
            }
        }

        [AbpAuthorize(AppPermissions.Pages_Schedule)]
        public async Task/*<SetDriverForTruckResult>*/ SetDriverForTruck(SetDriverForTruckInput input)
        {
            await EnsureCanAssignDriverToTruck(input.TruckId);
            if (input.DriverId != null)
            {
                await ThrowIfDriverHasTimeOffRequests(input.DriverId.Value, input.Date, input.Date);
            }

            var truck = await _truckCache.GetTruckFromCacheOrDbOrDefault(input.TruckId);
            var officeIdForDriverAssignment = truck.OfficeId ?? input.OfficeId ?? throw new UserFriendlyException("You need to select the office first");

            var driverAssignments = await GetAllDriverAssignmentsLite(new GetDriverAssignmentsInput
            {
                Date = input.Date,
                OfficeId = officeIdForDriverAssignment,
                Shift = input.Shift,
                TruckId = input.TruckId,
            });

            var driverAssignment = driverAssignments.Items.MaxBy(x => x.Id);
            if (driverAssignment == null)
            {
                input.CreateNewDriverAssignment = true;
            }

            await EditDriverAssignment(new DriverAssignmentEditDto
            {
                Id = !input.CreateNewDriverAssignment ? driverAssignment!.Id : 0,
                Date = input.Date,
                Shift = input.Shift,
                OfficeId = officeIdForDriverAssignment,
                DriverId = input.DriverId,
                TruckId = input.TruckId,
                StartTime = !input.CreateNewDriverAssignment ? driverAssignment!.StartTime : null,
            });
        }

        private async Task EnsureCanAssignDriverToTruck(int truckId)
        {
            var vehicleCategoryIsPowered = await (await _truckRepository.GetQueryAsync()).Where(t => t.Id == truckId).Select(t => t.VehicleCategory.IsPowered).FirstAsync();
            if (!vehicleCategoryIsPowered)
            {
                throw new ArgumentException("Cannot set driver for an unpowered truck!");
            }
        }

        [AbpAuthorize(AppPermissions.Pages_Schedule)]
        public async Task SetDefaultDriverForTruck(SetDefaultDriverForTruckInput input)
        {
            input.StartDate = input.StartDate.Date;
            input.EndDate = input.EndDate.Date;

            if (input.EndDate < input.StartDate)
            {
                throw new UserFriendlyException("End Date should be greater than Start Date");
            }

            int? defaultDriverId = await (await _truckRepository.GetQueryAsync())
                .Where(t => t.Id == input.TruckId)
                .Select(t => t.DefaultDriverId)
                .FirstAsync();
            if (!defaultDriverId.HasValue)
            {
                throw new ArgumentException("The truck has no default driver!");
            }

            await ThrowIfDriverHasTimeOffRequests(defaultDriverId.Value, input.StartDate, input.EndDate);

            var syncRequest = new SyncRequest();
            var existingAssignments = await (await _driverAssignmentRepository.GetQueryAsync())
                .Where(x => x.Date >= input.StartDate && x.Date <= input.EndDate && x.Shift == input.Shift && x.TruckId == input.TruckId)
                .ToListAsync();

            foreach (var dayGroup in existingAssignments.GroupBy(x => x.Date))
            {
                var firstAssignment = true;
                foreach (var driverAssignment in dayGroup)
                {
                    if (!firstAssignment)
                    {
                        syncRequest
                            .AddChange(EntityEnum.DriverAssignment, driverAssignment.ToChangedEntity().SetOldDriverIdToNotify(defaultDriverId), ChangeType.Removed);

                        await _driverAssignmentRepository.DeleteAsync(driverAssignment);
                    }
                    else
                    {
                        var oldDriverId = driverAssignment.DriverId;

                        driverAssignment.DriverId = defaultDriverId.Value;

                        syncRequest
                            .AddChange(EntityEnum.DriverAssignment, driverAssignment.ToChangedEntity().SetOldDriverIdToNotify(oldDriverId));
                    }

                    firstAssignment = false;
                }
            }

            await CurrentUnitOfWork.SaveChangesAsync();

            await _syncRequestSender.SendSyncRequest(syncRequest
                .AddLogMessage("Set default driver for truck"));
        }

        [AbpAuthorize(AppPermissions.Pages_DriverAssignment)]
        public async Task<byte[]> GetDriverAssignmentReport(GetDriverAssignmentsInput input)
        {
            Shift? shift = input.Shift == Shift.NoShift ? null : input.Shift;
            var items = await (await _driverAssignmentRepository.GetQueryAsync(input.Date, shift, input.OfficeId))
                .Where(da => da.Truck.OfficeId.HasValue)
                .Select(da => new DriverAssignmentReportItemDto
                {
                    Id = da.Id,
                    TruckId = da.TruckId,
                    TruckCode = da.Truck.TruckCode,
                    DriverId = da.DriverId,
                    DriverName = da.Driver.FirstName + " " + da.Driver.LastName,
                    DriverFirstName = da.Driver.FirstName,
                    DriverLastName = da.Driver.LastName,
                    DriverIsExternal = da.Driver.IsExternal == true,
                    DriverIsActive = da.Driver.IsInactive != true,
                    StartTime = da.StartTime,
                    OfficeName = da.Office.Name,
                })
                .OrderBy(x => x.TruckCode)
                .ToListAsync();

            var timezone = await GetTimezone();
            items.ForEach(x => x.StartTime = x.StartTime?.ConvertTimeZoneTo(timezone));

            await FillFirstTimeOnJob(items, input);

            var data = new DriverAssignmentReportDto
            {
                Date = input.Date,
                Shift = input.Shift,
                ShiftName = await SettingManager.GetShiftName(input.Shift),
                Items = items,
            };

            return DriverAssignmentReportGenerator.GenerateReport(data);
        }

        [AbpAuthorize(AppPermissions.Pages_Schedule)]
        public async Task ChangeDriverForOrderLineTruck(ChangeDriverForOrderLineTruckInput input)
        {
            var orderLineTruck = await (await _orderLineTruckRepository.GetQueryAsync())
                .Where(x => x.Id == input.OrderLineTruckId)
                .FirstAsync();

            var orderDetails = await (await _orderLineTruckRepository.GetQueryAsync())
                .Where(x => x.Id == input.OrderLineTruckId)
                .Select(x => new
                {
                    IsExternalTruck = x.Truck.OfficeId == null,
                    x.OrderLine.Order.DeliveryDate,
                    x.OrderLine.Order.Shift,
                    OfficeId = x.OrderLine.Order.OfficeId,
                }).FirstAsync();

            await ThrowIfDriverHasTimeOffRequests(input.DriverId, orderDetails.DeliveryDate, orderDetails.DeliveryDate);

            int? oldDriverId = null;
            DriverAssignment driverAssignment = null;

            if (input.ReplaceExistingDriver)
            {
                if (orderDetails.IsExternalTruck)
                {
                    var availableLeaseHaulerTrucks = await (await _availableLeaseHaulerTruckRepository.GetQueryAsync())
                        .Where(x => x.Date == orderDetails.DeliveryDate
                            && x.OfficeId == orderDetails.OfficeId
                            && x.Shift == orderDetails.Shift
                            && x.TruckId == orderLineTruck.TruckId)
                        .ToListAsync();

                    foreach (var availableLeaseHaulerTruck in availableLeaseHaulerTrucks)
                    {
                        availableLeaseHaulerTruck.DriverId = input.DriverId;
                    }
                }
                else
                {
                    driverAssignment = await (await _driverAssignmentRepository.GetQueryAsync(orderDetails.DeliveryDate, orderDetails.Shift, orderDetails.OfficeId))
                        .Where(x => x.TruckId == orderLineTruck.TruckId && x.DriverId == orderLineTruck.DriverId)
                        .OrderByDescending(x => x.Id)
                        .FirstOrDefaultAsync();

                    if (driverAssignment != null)
                    {
                        oldDriverId = driverAssignment.DriverId;
                        //driverAssignment.DriverId = input.DriverId;
                        await EditDriverAssignment(new DriverAssignmentEditDto
                        {
                            Id = driverAssignment.Id,
                            TruckId = driverAssignment.TruckId,
                            DriverId = input.DriverId,
                            StartTime = driverAssignment.StartTime,
                            OfficeId = driverAssignment.OfficeId,
                            Date = driverAssignment.Date,
                            Shift = driverAssignment.Shift,
                        });
                    }
                    else
                    {
                        //handled below
                    }
                }
            }
            else
            {
                //handled below
            }

            //if !ReplaceExistingDriver or if ReplaceExistingDriver but we didn't find a matching DriverAssignment - create a new driver assignment
            if (driverAssignment == null && !orderDetails.IsExternalTruck)
            {
                if (!await (await _driverAssignmentRepository.GetQueryAsync(orderDetails.DeliveryDate, orderDetails.Shift, orderDetails.OfficeId))
                    .AnyAsync(x => x.TruckId == orderLineTruck.TruckId && x.DriverId == input.DriverId))
                {
                    var truck = await _truckCache.GetTruckFromCacheOrDbOrDefault(orderLineTruck.TruckId);

                    await EditDriverAssignment(new DriverAssignmentEditDto
                    {
                        Id = 0,
                        Date = orderDetails.DeliveryDate,
                        Shift = orderDetails.Shift,
                        OfficeId = truck.OfficeId,
                        TruckId = orderLineTruck.TruckId,
                        DriverId = input.DriverId,
                    });
                }
            }

            orderLineTruck.DriverId = input.DriverId;
        }

        private async Task FillFirstTimeOnJob(IEnumerable<DriverAssignmentDto> items, GetDriverAssignmentsInput input)
        {
            var cachesToUse = new
            {
                _listCaches.OrderLineTruck,
                _listCaches.OrderLine,
                _listCaches.Order,
                _listCaches.Location,

                _listCaches.DriverAssignment,
                _listCaches.Driver,
                _listCaches.Truck,
                _listCaches.LeaseHaulerDriver,
                _listCaches.LeaseHaulerTruck,
            };

            var cachesToCheck = new IListCache[]
            {
                _listCaches.OrderLineTruck,
                _listCaches.OrderLine,
                _listCaches.Order,
                _listCaches.Location,

                _listCaches.DriverAssignment,
                _listCaches.Driver,
                _listCaches.Truck,
                _listCaches.LeaseHaulerDriver,
                _listCaches.LeaseHaulerTruck,
            };

            if (await cachesToCheck.AnyAsync(async c => !await c.IsEnabled()))
            {
                await FillFirstTimeOnJobFromDb(items, input);
                return;
            }

            var shift = input.Shift == Shift.NoShift ? null : input.Shift;
            var dateKey = new ListCacheDateKey(await Session.GetTenantIdAsync(), input.Date, shift);
            var tenantKey = new ListCacheTenantKey(await Session.GetTenantIdAsync());
            var cache = new
            {
                OrderLineTruck = await cachesToUse.OrderLineTruck.GetListOrThrow(dateKey),
                OrderLine = await cachesToUse.OrderLine.GetListOrThrow(dateKey),
                Order = await cachesToUse.Order.GetListOrThrow(dateKey),
                Location = await cachesToUse.Location.GetListOrThrow(tenantKey),


                DriverAssignment = await cachesToUse.DriverAssignment.GetListOrThrow(dateKey),
                Driver = await cachesToUse.Driver.GetListOrThrow(tenantKey),
                Truck = await cachesToUse.Truck.GetListOrThrow(tenantKey),
                LeaseHaulerDriver = await cachesToUse.LeaseHaulerDriver.GetListOrThrow(tenantKey),
                LeaseHaulerTruck = await cachesToUse.LeaseHaulerTruck.GetListOrThrow(tenantKey),
            };

            var timesOnJobRaw = cache.OrderLineTruck.Items
                .Select(x =>
                {
                    var orderLine = cache.OrderLine.Find(x.OrderLineId);
                    var loadAt = cache.Location.Find(orderLine?.LoadAtId);

                    if (input.OfficeId.HasValue)
                    {
                        var order = cache.Order.Find(orderLine?.OrderId);
                        if (order?.OfficeId != input.OfficeId)
                        {
                            return null;
                        }
                    }

                    return new
                    {
                        x.TruckId,
                        x.DriverId,
                        OrderLineTimeOnJobUtc = orderLine?.TimeOnJob,
                        TimeOnJobUtc = x.TimeOnJob,
                        LoadAtName = loadAt?.DisplayName,
                    };
                })
                .Where(x => x != null)
                .ToList();
            var timesOnJob = timesOnJobRaw.GroupBy(x => new { x.TruckId, x.DriverId }).ToList();

            var timezone = await GetTimezone();
            foreach (var item in items)
            {
                var timeOnJobToUse = timesOnJob
                    .FirstOrDefault(t => t.Key.TruckId == item.TruckId && t.Key.DriverId == item.DriverId)
                    ?.MinBy(t => t.TimeOnJobUtc ?? t.OrderLineTimeOnJobUtc);
                item.FirstTimeOnJob = (timeOnJobToUse?.TimeOnJobUtc ?? timeOnJobToUse?.OrderLineTimeOnJobUtc)?.ConvertTimeZoneTo(timezone);
                item.LoadAtName = timeOnJobToUse?.LoadAtName;
            }
        }


        private async Task FillFirstTimeOnJobFromDb(IEnumerable<DriverAssignmentDto> items, GetDriverAssignmentsInput input)
        {
            var timesOnJobRaw = await (await _orderLineTruckRepository.GetQueryAsync())
                .WhereIf(input.OfficeId.HasValue, x => x.OrderLine.Order.OfficeId == input.OfficeId)
                .Where(x => x.OrderLine.Order.DeliveryDate == input.Date && x.OrderLine.Order.Shift == input.Shift)
                .Select(x => new
                {
                    x.TruckId,
                    x.DriverId,
                    OrderLineTimeOnJobUtc = x.OrderLine.TimeOnJob,
                    TimeOnJobUtc = x.TimeOnJob,
                    LoadAtName = x.OrderLine.LoadAt.DisplayName,
                }).ToListAsync();
            var timesOnJob = timesOnJobRaw.GroupBy(x => new { x.TruckId, x.DriverId });

            var timezone = await GetTimezone();
            foreach (var item in items)
            {
                var timeOnJobToUse = timesOnJob
                    .FirstOrDefault(t => t.Key.TruckId == item.TruckId && t.Key.DriverId == item.DriverId)
                    ?.MinBy(t => t.TimeOnJobUtc ?? t.OrderLineTimeOnJobUtc);
                item.FirstTimeOnJob = (timeOnJobToUse?.TimeOnJobUtc ?? timeOnJobToUse?.OrderLineTimeOnJobUtc)?.ConvertTimeZoneTo(timezone);
                item.LoadAtName = timeOnJobToUse?.LoadAtName;
            }
        }

        [AbpAuthorize(AppPermissions.Pages_DriverAssignment)]
        public async Task<int> AddUnscheduledTrucks(AddUnscheduledTrucksInput input)
        {
            var unscheduledTrucks = await (await _truckRepository.GetQueryAsync())
                .WhereIf(input.OfficeId.HasValue, t => t.OfficeId == input.OfficeId)
                .Where(t => t.VehicleCategory.IsPowered && t.LeaseHaulerTruck.AlwaysShowOnSchedule != true && t.OfficeId != null
                    && !t.DriverAssignments.Any(da => da.Date == input.Date && da.Shift == input.Shift)
                    && t.IsActive
                    && !t.IsOutOfService
                )
                .Select(t => new
                {
                    t.Id,
                    t.OfficeId,
                    t.DefaultDriverId,
                })
                .ToListAsync();

            var timezone = await GetTimezone();

            var syncRequest = new SyncRequest();
            foreach (var unscheduledTruck in unscheduledTrucks)
            {
                var driverAssignment = new DriverAssignment
                {
                    OfficeId = input.OfficeId ?? unscheduledTruck.OfficeId,
                    Date = input.Date,
                    Shift = input.Shift,
                    TruckId = unscheduledTruck.Id,
                    DriverId = input.Shift == null || input.Shift == Shift.Shift1 ? unscheduledTruck.DefaultDriverId : null,
                    StartTime = input.DefaultStartTime.HasValue
                        ? input.Date.Add(input.DefaultStartTime.Value.TimeOfDay).ConvertTimeZoneFrom(timezone)
                        : (DateTime?)null,
                };
                syncRequest.AddChange(EntityEnum.DriverAssignment, driverAssignment.ToChangedEntity());
                await _driverAssignmentRepository.InsertAsync(driverAssignment);
            }

            await CurrentUnitOfWork.SaveChangesAsync();

            await _syncRequestSender.SendSyncRequest(syncRequest
                .AddLogMessage("Added driver assignments for unscheduled trucks"));

            return unscheduledTrucks.Count;
        }

        [AbpAuthorize(AppPermissions.Pages_Schedule)]
        public async Task AddDefaultDriverAssignments(AddDefaultDriverAssignmentsInput input)
        {
            var unscheduledDrivers = await (await _driverRepository.GetQueryAsync())
                .WhereIf(input.OfficeId.HasValue, d => d.OfficeId == input.OfficeId)
                .Where(d => !d.IsExternal && !d.TimeOffs.Any(to => to.StartDate <= input.Date && to.EndDate >= input.Date)
                    && !d.DriverAssignments.Any(da => da.Date == input.Date && da.Shift == input.Shift)
                    && !d.IsInactive
                    && d.DefaultTrucks.Any(t => t.OfficeId != null && t.LeaseHaulerTruck.AlwaysShowOnSchedule != true)
                )
                .Select(d => new
                {
                    d.Id,
                    DefaultTruckId = (int?)d.DefaultTrucks.FirstOrDefault(t => t.OfficeId != null && t.LeaseHaulerTruck.AlwaysShowOnSchedule != true).Id,
                })
                .ToListAsync();

            unscheduledDrivers.RemoveAll(x => x.DefaultTruckId == null);

            var syncRequest = new SyncRequest();
            foreach (var unscheduledDriver in unscheduledDrivers)
            {
                var driverAssignment = new DriverAssignment
                {
                    OfficeId = input.OfficeId,
                    Date = input.Date,
                    Shift = input.Shift,
                    TruckId = unscheduledDriver.DefaultTruckId!.Value,
                    DriverId = unscheduledDriver.Id,
                    StartTime = null,
                };
                syncRequest.AddChange(EntityEnum.DriverAssignment, driverAssignment.ToChangedEntity());
                await _driverAssignmentRepository.InsertAsync(driverAssignment);
            }

            await CurrentUnitOfWork.SaveChangesAsync();

            await _syncRequestSender.SendSyncRequest(syncRequest
                .AddLogMessage("Added default driver assignments"));
        }

        [AbpAuthorize(AppPermissions.Pages_DriverAssignment)]
        public async Task<EditDriverAssignmentResult> EditDriverAssignment(DriverAssignmentEditDto input)
        {
            var result = new EditDriverAssignmentResult();

            var driverAssignment = input.Id > 0 ? await _driverAssignmentRepository.GetAsync(input.Id) : new DriverAssignment();
            if (driverAssignment.Id == 0)
            {
                driverAssignment.OfficeId = input.OfficeId;
                driverAssignment.Date = input.Date;
                driverAssignment.Shift = input.Shift;
                driverAssignment.TruckId = input.TruckId;
                await _driverAssignmentRepository.InsertAndGetIdAsync(driverAssignment);
            }

            var logMessage = "";
            var oldDriverId = driverAssignment.DriverId;
            var driverAssignmentWasChanged = false;
            var driverAssignmentWasDeleted = false;

            if (driverAssignment.DriverId != input.DriverId || input.IsDelete)
            {
                if (input.DriverId.HasValue)
                {
                    await ThrowIfDriverHasTimeOffRequests(input.DriverId.Value, driverAssignment.Date, driverAssignment.Date);
                }
                var updateOrderLineTrucks = false;
                if (input.Id > 0 && oldDriverId.HasValue)
                {
                    var validationResult = await HasOrderLineTrucks(new HasOrderLineTrucksInput
                    {
                        Date = driverAssignment.Date,
                        OfficeId = driverAssignment.OfficeId,
                        Shift = driverAssignment.Shift,
                        TruckId = driverAssignment.TruckId,
                        DriverId = oldDriverId,
                    });
                    if (validationResult.HasOpenDispatches)
                    {
                        throw new UserFriendlyException(L("CannotDeleteDriverAssignmentBecauseOfDispatchesError"));
                    }
                    if (validationResult.HasOrderLineTrucks)
                    {
                        if (input.DriverId == null)
                        {
                            throw new UserFriendlyException(L("CannotRemoveDriverBecauseOfOrderLineTrucksError"));
                        }

                        updateOrderLineTrucks = true;
                    }
                }
                else if (oldDriverId == null)
                {
                    updateOrderLineTrucks = true;
                }

                if (updateOrderLineTrucks)
                {
                    var orderLineTrucks = await (await _orderLineTruckRepository.GetQueryAsync())
                            .Where(x => driverAssignment.Date == x.OrderLine.Order.DeliveryDate && driverAssignment.Shift == x.OrderLine.Order.Shift && !x.IsDone)
                            .WhereIf(driverAssignment.OfficeId.HasValue, x => driverAssignment.OfficeId == x.OrderLine.Order.OfficeId)
                            .Where(x => oldDriverId == x.DriverId)
                            .Where(x => driverAssignment.TruckId == x.TruckId)
                            .ToListAsync();

                    foreach (var orderLineTruck in orderLineTrucks)
                    {
                        orderLineTruck.DriverId = input.DriverId;
                    }
                }

                driverAssignment.DriverId = input.DriverId;

                var sameTruckDriverAssignments = await (await _driverAssignmentRepository.GetQueryAsync())
                    .Where(x => x.Shift == driverAssignment.Shift
                        && x.OfficeId == driverAssignment.OfficeId
                        && x.Date == driverAssignment.Date
                        && x.TruckId == driverAssignment.TruckId
                        && x.Id != driverAssignment.Id)
                    .ToListAsync();

                var duplicateDriverAssignments = sameTruckDriverAssignments
                    .Where(x => x.DriverId == driverAssignment.DriverId)
                    .ToList();

                if (duplicateDriverAssignments.Any())
                {
                    foreach (var duplicateDriverAssignment in duplicateDriverAssignments)
                    {
                        await _driverAssignmentRepository.DeleteAsync(duplicateDriverAssignment);
                        sameTruckDriverAssignments.Remove(duplicateDriverAssignment);
                    }
                    result.ReloadRequired = true;
                }

                if (input.IsDelete)
                {
                    if (input.Id > 0)
                    {
                        var orderLineTrucks = await (await _orderLineTruckRepository.GetQueryAsync())
                            .Where(x => driverAssignment.Date == x.OrderLine.Order.DeliveryDate && driverAssignment.Shift == x.OrderLine.Order.Shift && !x.IsDone && !x.Dispatches.Any())
                            .WhereIf(driverAssignment.OfficeId.HasValue, x => driverAssignment.OfficeId == x.OrderLine.Order.OfficeId)
                            .Where(x => oldDriverId == x.DriverId)
                            .Where(x => driverAssignment.TruckId == x.TruckId)
                            .ToListAsync();

                        foreach (var orderLineTruck in orderLineTrucks)
                        {
                            await _orderLineTruckRepository.DeleteAsync(orderLineTruck);
                        }

                        await _driverAssignmentRepository.DeleteAsync(driverAssignment);
                        driverAssignmentWasDeleted = true;
                        result.ReloadRequired = true;
                    }
                    else
                    {
                        foreach (var sameTruckDriverAssignment in sameTruckDriverAssignments.ToList())
                        {
                            await _driverAssignmentRepository.DeleteAsync(sameTruckDriverAssignment);
                            sameTruckDriverAssignments.Remove(sameTruckDriverAssignment);
                            result.ReloadRequired = true;
                        }
                    }
                }

                logMessage += $"Changed driver assignment for truck {driverAssignment.TruckId} from driver {oldDriverId?.ToString() ?? "null"} to {input.DriverId?.ToString() ?? "null"}\n";
                driverAssignmentWasChanged = true;
            }

            if (driverAssignment.StartTime != input.StartTime)
            {
                if (input.StartTime.HasValue)
                {
                    input.StartTime = driverAssignment.Date.Add(input.StartTime.Value.TimeOfDay);
                }

                driverAssignment.StartTime = input.StartTime?.ConvertTimeZoneFrom(await GetTimezone());

                logMessage += $"Updated start time to {input.StartTime?.ToShortTimeString()} for driver assignment {driverAssignment.Id}\n";
                driverAssignmentWasChanged = true;
            }

            if (driverAssignmentWasChanged)
            {
                await CurrentUnitOfWork.SaveChangesAsync();

                await _syncRequestSender.SendSyncRequest(new SyncRequest()
                    .AddChange(EntityEnum.DriverAssignment, driverAssignment.ToChangedEntity().SetOldDriverIdToNotify(oldDriverId), driverAssignmentWasDeleted ? ChangeType.Removed : ChangeType.Modified)
                    .AddLogMessage(logMessage));
            }

            return result;
        }

        [AbpAuthorize(AppPermissions.Pages_DriverAssignment)]
        public async Task<HasOrderLineTrucksResult> HasOrderLineTrucks(HasOrderLineTrucksInput input)
        {
            var result = new HasOrderLineTrucksResult();
            result.HasOrderLineTrucks = await (await _orderLineTruckRepository.GetQueryAsync())
                .Where(x => input.Date == x.OrderLine.Order.DeliveryDate && input.Shift == x.OrderLine.Order.Shift && !x.IsDone && x.Dispatches.Any())
                .WhereIf(input.OfficeId.HasValue, x => input.OfficeId == x.OrderLine.Order.OfficeId)
                .WhereIf(input.DriverId.HasValue, x => input.DriverId == x.DriverId)
                .WhereIf(input.TrailerId.HasValue || input.ForceTrailerIdFilter, x => input.TrailerId == x.TrailerId)
                .WhereIf(input.TruckId.HasValue, x => input.TruckId == x.TruckId)
                .AnyAsync();

            result.HasOpenDispatches = await (await _dispatchRepository.GetQueryAsync())
                .Where(x => input.Date == x.OrderLine.Order.DeliveryDate
                    && input.Shift == x.OrderLine.Order.Shift
                    && Dispatch.OpenStatuses.Contains(x.Status))
                .WhereIf(input.OfficeId.HasValue, x => input.OfficeId == x.OrderLine.Order.OfficeId)
                .WhereIf(input.DriverId.HasValue, x => input.DriverId == x.DriverId)
                .WhereIf(input.TrailerId.HasValue, x => input.TrailerId == x.OrderLineTruck.TrailerId)
                .WhereIf(input.TruckId.HasValue, x => input.TruckId == x.TruckId)
                .AnyAsync();

            return result;
        }

        [AbpAuthorize(AppPermissions.Pages_DriverAssignment)]
        public async Task AddDefaultStartTime(AddDefaultStartTimeInput input)
        {
            if (!input.DefaultStartTime.HasValue)
            {
                throw new UserFriendlyException("Default Start Time is required");
            }

            var driverAssignments = await (await _driverAssignmentRepository.GetQueryAsync(input.Date, input.Shift, input.OfficeId))
                .Where(x => x.StartTime == null)
                .ToListAsync();

            if (!driverAssignments.Any())
            {
                return;
            }

            var timezone = await GetTimezone();
            driverAssignments.ForEach(x => x.StartTime = x.Date.Add(input.DefaultStartTime.Value.TimeOfDay).ConvertTimeZoneFrom(timezone));

            await CurrentUnitOfWork.SaveChangesAsync();

            await _syncRequestSender.SendSyncRequest(new SyncRequest()
                .AddChanges(EntityEnum.DriverAssignment, driverAssignments.Select(x => x.ToChangedEntity()))
                .AddLogMessage("Added scheduled start time"));
        }
    }
}
