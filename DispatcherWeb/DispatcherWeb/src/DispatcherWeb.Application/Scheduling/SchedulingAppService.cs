using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Threading.Tasks;
using Abp.Application.Services.Dto;
using Abp.Authorization;
using Abp.Collections.Extensions;
using Abp.Configuration;
using Abp.Domain.Repositories;
using Abp.Domain.Uow;
using Abp.Extensions;
using Abp.Linq.Extensions;
using Abp.Runtime.Session;
using Abp.UI;
using DispatcherWeb.Authorization;
using DispatcherWeb.Caching;
using DispatcherWeb.Configuration;
using DispatcherWeb.Dispatching;
using DispatcherWeb.Dispatching.Dto;
using DispatcherWeb.Drivers;
using DispatcherWeb.Dto;
using DispatcherWeb.Exceptions;
using DispatcherWeb.Features;
using DispatcherWeb.Infrastructure.EntityReadonlyCheckers;
using DispatcherWeb.Infrastructure.Extensions;
using DispatcherWeb.Infrastructure.Telematics;
using DispatcherWeb.LeaseHaulerRequests;
using DispatcherWeb.LeaseHaulerRequests.Dto;
using DispatcherWeb.LeaseHaulers;
using DispatcherWeb.LeaseHaulers.Dto;
using DispatcherWeb.Orders;
using DispatcherWeb.Orders.Dto;
using DispatcherWeb.Scheduling.Dto;
using DispatcherWeb.Scheduling.Exporting;
using DispatcherWeb.Sessions;
using DispatcherWeb.SyncRequests;
using DispatcherWeb.Trucks;
using DispatcherWeb.Trucks.Dto;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DispatcherWeb.Scheduling
{
    [AbpAuthorize]
    public partial class SchedulingAppService : DispatcherWebAppServiceBase, ISchedulingAppService
    {
        private readonly IRepository<Truck> _truckRepository;
        private readonly IRepository<Order> _orderRepository;
        private readonly IRepository<OrderLine> _orderLineRepository;
        private readonly IRepository<OrderLineTruck> _orderLineTruckRepository;
        private readonly IRepository<Ticket> _ticketRepository;
        private readonly IRepository<DriverAssignment> _driverAssignmentRepository;
        private readonly IRepository<Dispatch> _dispatchRepository;
        private readonly IRepository<AvailableLeaseHaulerTruck> _availableLeaseHaulerTruckRepository;
        private readonly IRepository<LeaseHaulerTruck> _leaseHaulerTruckRepository;
        private readonly IRepository<LeaseHaulerRequest> _leaseHaulerRequestRepository;
        private readonly IRepository<RequestedLeaseHaulerTruck> _requestedLeaseHaulerTruckRepository;
        private readonly IRepository<Driver> _driverRepository;
        private readonly OrderTaxCalculator _orderTaxCalculator;
        private readonly IOrderLineUpdaterFactory _orderLineUpdaterFactory;
        private readonly IReadonlyCheckerFactory<OrderLine> _orderLineReadonlyCheckerFactory;
        private readonly ListCacheCollection _listCaches;
        private readonly IDispatchingAppService _dispatchingAppService;
        private readonly IOrderLineScheduledTrucksUpdater _orderLineScheduledTrucksUpdater;
        private readonly ICrossTenantOrderSender _crossTenantOrderSender;
        private readonly ISyncRequestSender _syncRequestSender;
        private readonly IGeotabTelematics _geotabTelematics;
        private readonly IScheduleOrderListCsvExporter _scheduleOrderListCsvExporter;
        private readonly ILeaseHaulerNotifier _leaseHaulerNotifier;

        public SchedulingAppService(
            IRepository<Truck> truckRepository,
            IRepository<Order> orderRepository,
            IRepository<OrderLine> orderLineRepository,
            IRepository<OrderLineTruck> orderLineTruckRepository,
            IRepository<Ticket> ticketRepository,
            IRepository<DriverAssignment> driverAssignmentRepository,
            IRepository<Dispatch> dispatchRepository,
            IRepository<AvailableLeaseHaulerTruck> availableLeaseHaulerTruckRepository,
            IRepository<LeaseHaulerTruck> leaseHaulerTruckRepository,
            IRepository<LeaseHaulerRequest> leaseHaulerRequestRepository,
            IRepository<RequestedLeaseHaulerTruck> requestedLeaseHaulerTruckRepository,
            IRepository<Driver> driverRepository,
            OrderTaxCalculator orderTaxCalculator,
            IOrderLineUpdaterFactory orderLineUpdaterFactory,
            IReadonlyCheckerFactory<OrderLine> orderLineReadonlyCheckerFactory,
            ListCacheCollection listCaches,
            IDispatchingAppService dispatchingAppService,
            IOrderLineScheduledTrucksUpdater orderLineScheduledTrucksUpdater,
            ICrossTenantOrderSender crossTenantOrderSender,
            ISyncRequestSender syncRequestSender,
            IGeotabTelematics geotabTelematics,
            IScheduleOrderListCsvExporter scheduleOrderListCsvExporter,
            ILeaseHaulerNotifier leaseHaulerNotifier
        )
        {
            _truckRepository = truckRepository;
            _orderRepository = orderRepository;
            _orderLineRepository = orderLineRepository;
            _orderLineTruckRepository = orderLineTruckRepository;
            _ticketRepository = ticketRepository;
            _driverAssignmentRepository = driverAssignmentRepository;
            _dispatchRepository = dispatchRepository;
            _availableLeaseHaulerTruckRepository = availableLeaseHaulerTruckRepository;
            _leaseHaulerTruckRepository = leaseHaulerTruckRepository;
            _leaseHaulerRequestRepository = leaseHaulerRequestRepository;
            _requestedLeaseHaulerTruckRepository = requestedLeaseHaulerTruckRepository;
            _driverRepository = driverRepository;
            _orderTaxCalculator = orderTaxCalculator;
            _orderLineUpdaterFactory = orderLineUpdaterFactory;
            _orderLineReadonlyCheckerFactory = orderLineReadonlyCheckerFactory;
            _listCaches = listCaches;
            _dispatchingAppService = dispatchingAppService;
            _orderLineScheduledTrucksUpdater = orderLineScheduledTrucksUpdater;
            _crossTenantOrderSender = crossTenantOrderSender;
            _syncRequestSender = syncRequestSender;
            _geotabTelematics = geotabTelematics;
            _scheduleOrderListCsvExporter = scheduleOrderListCsvExporter;
            _leaseHaulerNotifier = leaseHaulerNotifier;
        }

        //truck tiles
        [AbpAuthorize(AppPermissions.Pages_Schedule, AppPermissions.LeaseHaulerPortal_Schedule)]
        public async Task<ListResultDto<ScheduleTruckDto>> GetScheduleTrucks(GetScheduleTrucksInput input)
        {
            var permissions = new
            {
                ViewSchedule = await IsGrantedAsync(AppPermissions.Pages_Schedule),
                ViewLeaseHaulerSchedule = await IsGrantedAsync(AppPermissions.LeaseHaulerPortal_Schedule),
            };

            List<ScheduleTruckDto> trucks;

            if (permissions.ViewSchedule)
            {
                var showTrailersOnSchedule = await SettingManager.GetSettingValueAsync<bool>(AppSettings.DispatchingAndMessaging.ShowTrailersOnSchedule);
                trucks = (
                    await GetScheduleTrucksFromCache(
                        (await _truckRepository.GetQueryAsync())
                            .WhereIf(input.TruckCategoryId.HasValue, t => t.VehicleCategoryId == input.TruckCategoryId.Value)
                            .WhereIf(!showTrailersOnSchedule, t => t.VehicleCategory.IsPowered),
                        t => true, //the above whereIfs are repeated below
                        input,
                        await SettingManager.UseShifts(),
                        await FeatureChecker.IsEnabledAsync(AppFeatures.AllowLeaseHaulersFeature)
                    ))
                    .WhereIf(input.TruckCategoryId.HasValue, t => t.VehicleCategory.Id == input.TruckCategoryId)
                    .WhereIf(!showTrailersOnSchedule, t => t.VehicleCategory.IsPowered)
                    .ToList();
            }
            else if (permissions.ViewLeaseHaulerSchedule)
            {
                var leaseHaulerIdFilter = Session.GetLeaseHaulerIdOrThrow(this);
                trucks = await GetScheduleTrucksFromCache(
                    (await _leaseHaulerTruckRepository.GetQueryAsync())
                        .Where(q => q.LeaseHaulerId == leaseHaulerIdFilter)
                        .Select(s => s.Truck),
                    t => t.LeaseHaulerId == leaseHaulerIdFilter,
                    input,
                    await SettingManager.UseShifts(),
                    useLeaseHaulers: false,
                    skipTruckFiltering: true
                );
            }
            else
            {
                throw new AbpAuthorizationException();
            }

            return new ListResultDto<ScheduleTruckDto>(trucks);
        }

        [AbpAuthorize(AppPermissions.Pages_Schedule)]
        [HttpPost]
        public async Task<PagedResultDto<TruckToAssignDto>> GetTrucksToAssign(GetTrucksToAssignInput input)
        {
            var query = (await _truckRepository.GetQueryAsync())
                .Where(t => t.IsActive && !t.IsOutOfService);
            if (input.UseAndForTrailerCondition)
            {
                query = query.Where(t => (!input.PowerUnitsVehicleCategoryIds.Any() || input.PowerUnitsVehicleCategoryIds.Contains(t.VehicleCategoryId))
                    && (string.IsNullOrEmpty(input.PowerUnitsMake) || t.Make == input.PowerUnitsMake)
                    && (string.IsNullOrEmpty(input.PowerUnitsModel) || t.Model == input.PowerUnitsModel)
                    && (!input.PowerUnitsBedConstruction.HasValue || t.BedConstruction == input.PowerUnitsBedConstruction)
                    && (!input.IsApportioned || t.IsApportioned == input.IsApportioned)

                    && (!input.TrailersVehicleCategoryIds.Any() || t.CurrentTrailer != null && input.TrailersVehicleCategoryIds.Contains(t.CurrentTrailer.VehicleCategoryId) || t.VehicleCategory.AssetType == AssetType.Trailer && input.TrailersVehicleCategoryIds.Contains(t.VehicleCategoryId))
                    && (string.IsNullOrEmpty(input.TrailersMake) || t.CurrentTrailer.Make == input.TrailersMake || t.VehicleCategory.AssetType == AssetType.Trailer && t.Make == input.TrailersMake)
                    && (string.IsNullOrEmpty(input.TrailersModel) || t.CurrentTrailer.Model == input.TrailersModel || t.VehicleCategory.AssetType == AssetType.Trailer && t.Model == input.TrailersModel)
                    && (!input.TrailersBedConstruction.HasValue || t.CurrentTrailer.BedConstruction == input.TrailersBedConstruction || t.VehicleCategory.AssetType == AssetType.Trailer && t.BedConstruction == input.TrailersBedConstruction));
            }
            else
            {
                query = query.Where(t => (!input.PowerUnitsVehicleCategoryIds.Any() || input.PowerUnitsVehicleCategoryIds.Contains(t.VehicleCategoryId))
                    && (string.IsNullOrEmpty(input.PowerUnitsMake) || t.Make == input.PowerUnitsMake)
                    && (string.IsNullOrEmpty(input.PowerUnitsModel) || t.Model == input.PowerUnitsModel)
                    && (!input.PowerUnitsBedConstruction.HasValue || t.BedConstruction == input.PowerUnitsBedConstruction)
                    && (!input.IsApportioned || t.IsApportioned == input.IsApportioned)

                    || (!input.TrailersVehicleCategoryIds.Any() || t.CurrentTrailer != null && input.TrailersVehicleCategoryIds.Contains(t.CurrentTrailer.VehicleCategoryId) || t.VehicleCategory.AssetType == AssetType.Trailer && input.TrailersVehicleCategoryIds.Contains(t.VehicleCategoryId))
                    && (string.IsNullOrEmpty(input.TrailersMake) || t.CurrentTrailer.Make == input.TrailersMake || t.VehicleCategory.AssetType == AssetType.Trailer && t.Make == input.TrailersMake)
                    && (string.IsNullOrEmpty(input.TrailersModel) || t.CurrentTrailer.Model == input.TrailersModel || t.VehicleCategory.AssetType == AssetType.Trailer && t.Model == input.TrailersModel)
                    && (!input.TrailersBedConstruction.HasValue || t.CurrentTrailer.BedConstruction == input.TrailersBedConstruction || t.VehicleCategory.AssetType == AssetType.Trailer && t.BedConstruction == input.TrailersBedConstruction));
            }
            var trucks = await GetScheduleTrucksFromCache(
                query,
                t => (
                    input.UseAndForTrailerCondition
                    ? //todo see if we can reduce code duplication for these 4 conditions
                        (!input.PowerUnitsVehicleCategoryIds.Any() || input.PowerUnitsVehicleCategoryIds.Contains(t.VehicleCategory.Id))
                        && (string.IsNullOrEmpty(input.PowerUnitsMake) || t.Make == input.PowerUnitsMake)
                        && (string.IsNullOrEmpty(input.PowerUnitsModel) || t.Model == input.PowerUnitsModel)
                        && (!input.PowerUnitsBedConstruction.HasValue || t.BedConstruction == input.PowerUnitsBedConstruction)
                        && (!input.IsApportioned || t.IsApportioned == input.IsApportioned)

                        && (!input.TrailersVehicleCategoryIds.Any() || t.Trailer != null && input.TrailersVehicleCategoryIds.Contains(t.Trailer.VehicleCategory.Id) || t.VehicleCategory.AssetType == AssetType.Trailer && input.TrailersVehicleCategoryIds.Contains(t.VehicleCategory.Id))
                        && (string.IsNullOrEmpty(input.TrailersMake) || t.Trailer?.Make == input.TrailersMake || t.VehicleCategory.AssetType == AssetType.Trailer && t.Make == input.TrailersMake)
                        && (string.IsNullOrEmpty(input.TrailersModel) || t.Trailer?.Model == input.TrailersModel || t.VehicleCategory.AssetType == AssetType.Trailer && t.Model == input.TrailersModel)
                        && (!input.TrailersBedConstruction.HasValue || t.Trailer?.BedConstruction == input.TrailersBedConstruction || t.VehicleCategory.AssetType == AssetType.Trailer && t.BedConstruction == input.TrailersBedConstruction)
                    :
                        (!input.PowerUnitsVehicleCategoryIds.Any() || input.PowerUnitsVehicleCategoryIds.Contains(t.VehicleCategory.Id))
                        && (string.IsNullOrEmpty(input.PowerUnitsMake) || t.Make == input.PowerUnitsMake)
                        && (string.IsNullOrEmpty(input.PowerUnitsModel) || t.Model == input.PowerUnitsModel)
                        && (!input.PowerUnitsBedConstruction.HasValue || t.BedConstruction == input.PowerUnitsBedConstruction)
                        && (!input.IsApportioned || t.IsApportioned == input.IsApportioned)

                        || (!input.TrailersVehicleCategoryIds.Any() || t.Trailer != null && input.TrailersVehicleCategoryIds.Contains(t.Trailer.VehicleCategory.Id) || t.VehicleCategory.AssetType == AssetType.Trailer && input.TrailersVehicleCategoryIds.Contains(t.VehicleCategory.Id))
                        && (string.IsNullOrEmpty(input.TrailersMake) || t.Trailer?.Make == input.TrailersMake || t.VehicleCategory.AssetType == AssetType.Trailer && t.Make == input.TrailersMake)
                        && (string.IsNullOrEmpty(input.TrailersModel) || t.Trailer?.Model == input.TrailersModel || t.VehicleCategory.AssetType == AssetType.Trailer && t.Model == input.TrailersModel)
                        && (!input.TrailersBedConstruction.HasValue || t.Trailer?.BedConstruction == input.TrailersBedConstruction || t.VehicleCategory.AssetType == AssetType.Trailer && t.BedConstruction == input.TrailersBedConstruction)
                ),
                input,
                await SettingManager.UseShifts(),
                await FeatureChecker.IsEnabledAsync(AppFeatures.AllowLeaseHaulersFeature)
            );

            if (await SettingManager.GetSettingValueAsync<bool>(AppSettings.DispatchingAndMessaging.ValidateUtilization))
            {
                trucks.RemoveAll(x => x.Utilization >= 1 && x.VehicleCategory.IsPowered);
            }

            if (!await SettingManager.GetSettingValueAsync<bool>(AppSettings.DispatchingAndMessaging.AllowSchedulingTrucksWithoutDrivers))
            {
                trucks.RemoveAll(x => x.HasNoDriver || !x.HasDefaultDriver && !x.HasDriverAssignment && (!x.IsExternal || x.DriverId == null));
            }
            if (await SettingManager.GetSettingValueAsync<bool>(AppSettings.LeaseHaulers.NotAllowSchedulingLeaseHaulersWithExpiredInsurance))
            {
                trucks.RemoveAll(x => x.Insurances != null
                                 && (!x.Insurances.Any(c => c.IsActive)
                                 || x.Insurances.Any(c => c.IsActive && c.ExpirationDate < input.Date)));
            }

            var assignedTruckIds = await (await _orderLineTruckRepository.GetQueryAsync())
                .Where(x => x.OrderLineId == input.OrderLineId)
                .Select(x => x.TruckId)
                .Distinct()
                .ToListAsync();
            trucks.RemoveAll(x => assignedTruckIds.Contains(x.Id));

            foreach (var truck in trucks.ToList())
            {
                if (truck.VehicleCategory.AssetType != AssetType.Trailer)
                {
                    continue;
                }

                if (trucks.Any(t => t.Trailer?.Id == truck.Id))
                {
                    trucks.Remove(truck);
                }
                else
                {
                    truck.Trailer = new ScheduleTruckTrailerDto
                    {
                        Id = truck.Id,
                        TruckCode = truck.TruckCode,
                        BedConstruction = truck.BedConstruction,
                        Make = truck.Make,
                        Model = truck.Model,
                        VehicleCategory = truck.VehicleCategory,
                    };
                    truck.Id = 0;
                    truck.TruckCode = null;
                    truck.BedConstruction = 0;
                    truck.Make = null;
                    truck.Model = null;
                    truck.VehicleCategory = null;
                }
            }

            var items = trucks.Select(x => new TruckToAssignDto
            {
                TruckId = x.Id == 0 ? null : x.Id,
                TrailerId = x.Trailer?.Id,
                TruckCode = x.TruckCode,
                TruckCodeWithModelInfo = x.Id == 0 ? null : x.TruckCode + " " + x.VehicleCategory.Name + ", "
                    + string.Join(", ", new[] {
                        x.Make,
                        x.Model,
                        x.BedConstructionFormatted,
                    }.Where(s => !string.IsNullOrWhiteSpace(s))),
                TrailerTruckCodeWithModelInfo = x.Trailer == null ? null : x.Trailer.TruckCode + " " + x.Trailer.VehicleCategory.Name + ", "
                    + string.Join(", ", new[] {
                        x.Trailer.Make,
                        x.Trailer.Model,
                        x.Trailer.BedConstructionFormatted,
                    }.Where(s => !string.IsNullOrWhiteSpace(s))),
                LeaseHaulerId = x.LeaseHaulerId,
                BedConstruction = x.BedConstruction,
                DriverId = x.DriverId,
                DriverName = x.DriverName,
                IsApportioned = x.IsApportioned,
            })
            .WhereIf(!string.IsNullOrEmpty(input.DriverName), t => t.DriverName.Contains(input.DriverName))
            .AsQueryable()
            .OrderBy(input.Sorting)
            .ToList();

            return new PagedResultDto<TruckToAssignDto>(
                trucks.Count,
                items);
        }

        [HttpPost]
        [AbpAuthorize(AppPermissions.Pages_Schedule)]
        public async Task<FileDto> GetScheduleOrdersToCsv(GetScheduleOrdersInput input)
        {
            var query = from orderLine in await _orderLineRepository.GetQueryAsync()
                        join orderLineTruck in await _orderLineTruckRepository.GetQueryAsync()
                            on orderLine.Id equals orderLineTruck.OrderLineId into orderLineTruckGroup
                        from orderLineTruck in orderLineTruckGroup.DefaultIfEmpty()
                        select new
                        {
                            OrderLine = orderLine,
                            OrderLineTruck = orderLineTruck,
                            Order = orderLine.Order,
                            LoadAt = orderLine.LoadAt,
                            DeliverTo = orderLine.DeliverTo,
                        };

            var items = await query
                .Where(x => !x.OrderLine.IsCancelled
                    && x.Order.DeliveryDate == input.Date
                )
                .Select(x => new ExportScheduleOrderDto
                {
                    Customer = x.Order.Customer.Name,
                    DriverName = x.OrderLineTruck.Driver == null ? null : x.OrderLineTruck.Driver.LastName + ", " + x.OrderLineTruck.Driver.FirstName,
                    TruckCode = x.OrderLineTruck.Truck.TruckCode,
                    DeliveryDate = x.OrderLine.Order.DeliveryDate,
                    TimeOnJobUtc = x.OrderLineTruck.TimeOnJob ?? x.OrderLine.TimeOnJob,
                    JobNumber = x.OrderLine.JobNumber,
                    StartName = x.LoadAt.Name,
                    StartAddress = x.LoadAt == null ? null : $"{x.LoadAt.StreetAddress}, {x.LoadAt.City} {x.LoadAt.State} {x.LoadAt.ZipCode}",
                    DeliverTo = x.DeliverTo.Name,
                    DeliverToAddress = x.DeliverTo == null ? null : $"{x.DeliverTo.StreetAddress}, {x.DeliverTo.City} {x.DeliverTo.State} {x.DeliverTo.ZipCode}",
                    FreightItemName = x.OrderLine.FreightItem.Name,
                    MaterialItemName = x.OrderLine.MaterialItem.Name,
                    FreightPricePerUnit = x.OrderLine.FreightPricePerUnit,
                    MaterialPricePerUnit = x.OrderLine.MaterialPricePerUnit,
                    ChargeTo = x.Order.ChargeTo,
                    Contact = $"{x.Order.CustomerContact.Name} {x.Order.CustomerContact.PhoneNumber}",
                    AdditionalNotes = x.OrderLine.Note,
                })
                .OrderBy(x => x.DriverName)
                .ThenBy(x => x.TimeOnJobUtc)
                .ToListAsync();

            if (!items.Any())
            {
                throw new UserFriendlyException(L("ThereIsNoDataToExport"));
            }

            var timeZone = await GetTimezone();
            items.ForEach(x => x.TimeZone = timeZone);

            return await _scheduleOrderListCsvExporter.ExportToFileAsync(items);
        }

        [AbpAuthorize(AppPermissions.Pages_Schedule)]
        public async Task<PagedResultDto<TruckOrderLineDto>> GetTruckOrderLinesPaged(GetTruckOrdersInput input)
        {
            var truckOrderLines = await GetTruckOrderLines(input);
            return new PagedResultDto<TruckOrderLineDto>(truckOrderLines.Count, truckOrderLines.ToList());
        }

        [AbpAuthorize(AppPermissions.Pages_Schedule)]
        public async Task<IList<TruckOrderLineDto>> GetTruckOrderLines(GetTruckOrdersInput input)
        {
            var truckOrders = await (await _orderLineTruckRepository.GetQueryAsync())
                .Where(olt => olt.OrderLine.Order.DeliveryDate == input.ScheduleDate
                        && olt.OrderLine.Order.Shift == input.Shift
                        && olt.TruckId == input.TruckId)
                .Select(olt => new TruckOrderLineDto
                {
                    OrderLineId = olt.OrderLineId,
                    OrderId = olt.OrderLine.OrderId,
                    DriverId = olt.DriverId,
                    DriverName = olt.Driver.FirstName + " " + olt.Driver.LastName,
                    TruckStartTime = olt.TimeOnJob,
                    OrderLineStartTime = olt.OrderLine.TimeOnJob,
                    Customer = olt.OrderLine.Order.Customer.Name,
                    LoadAtName = olt.OrderLine.LoadAt.DisplayName,
                    DeliverToName = olt.OrderLine.DeliverTo.DisplayName,
                    Designation = olt.OrderLine.Designation,
                    Utilization = olt.Utilization,
                    FreightItem = olt.OrderLine.FreightItem.Name,
                    MaterialItem = olt.OrderLine.MaterialItem.Name,
                    MaterialUom = olt.OrderLine.MaterialUom.Name,
                    FreightUom = olt.OrderLine.FreightUom.Name,
                    MaterialQuantity = olt.OrderLine.MaterialQuantity,
                    FreightQuantity = olt.OrderLine.FreightQuantity,
                })
                .ToListAsync();

            truckOrders = truckOrders
                .GroupBy(x => new { x.OrderLineId, x.DriverId })
                .Select(g => new TruckOrderLineDto
                {
                    OrderLineId = g.Key.OrderLineId,
                    OrderId = g.First().OrderId,
                    DriverId = g.Key.DriverId,
                    DriverName = g.First().DriverName,
                    TruckStartTime = g.First().TruckStartTime,
                    OrderLineStartTime = g.First().OrderLineStartTime,
                    Customer = g.First().Customer,
                    LoadAtName = g.First().LoadAtName,
                    DeliverToName = g.First().DeliverToName,
                    Designation = g.First().Designation,
                    Utilization = g.Sum(x => x.Utilization),
                    FreightItem = g.First().FreightItem,
                    MaterialItem = g.First().MaterialItem,
                    MaterialUom = g.First().MaterialUom,
                    FreightUom = g.First().FreightUom,
                    MaterialQuantity = g.First().MaterialQuantity,
                    FreightQuantity = g.First().FreightQuantity,
                })
                .OrderBy(p => p.StartTime)
                .ToList();

            var timezone = await GetTimezone();
            foreach (var truckOrder in truckOrders)
            {
                truckOrder.TruckStartTime = truckOrder.TruckStartTime?.ConvertTimeZoneTo(timezone);
                truckOrder.OrderLineStartTime = truckOrder.OrderLineStartTime?.ConvertTimeZoneTo(timezone);
            }

            return truckOrders;
        }

        [AbpAuthorize(AppPermissions.Pages_Schedule)]
        public async Task<DateTime?> GetStartTimeForTruckOrderLines(GetTruckOrdersInput input)
        {
            var result = await (await _driverAssignmentRepository.GetQueryAsync())
                .Where(da => da.Date == input.ScheduleDate && da.Shift == input.Shift && da.TruckId == input.TruckId)
                .Select(da => da.StartTime)
                .MinAsync();

            result = result?.ConvertTimeZoneTo(await GetTimezone());

            return result;
        }

        [AbpAuthorize(AppPermissions.Pages_Schedule)]
        public async Task<IList<OrderLineTruckDto>> GetOrderLineTrucks(int orderLineId)
        {
            return await (await _orderLineRepository.GetQueryAsync())
                .Where(ol => ol.Id == orderLineId)
                .SelectMany(ol => ol.OrderLineTrucks)
                .Select(olt => new OrderLineTruckDto
                {
                    TruckId = olt.TruckId,
                    TruckCode = olt.Truck.TruckCode,
                })
                .ToListAsync();
        }

        [AbpAuthorize(AppPermissions.Pages_Schedule, AppPermissions.LeaseHaulerPortal_Schedule)]
        public async Task<AddOrderTruckResult> AddOrderLineTruck(AddOrderLineTruckInput input)
        {
            await CheckTruckEditPermissions(AppPermissions.Pages_Schedule, AppPermissions.LeaseHaulerPortal_Schedule,
                _truckRepository, input.TruckId);

            await CheckOrderLineEditPermissions(AppPermissions.Pages_Schedule, AppPermissions.LeaseHaulerPortal_Schedule,
                _orderLineRepository, input.OrderLineId);

            return await AddOrderLineTruckInternal(new AddOrderLineTruckInternalInput(input, 1));
        }

        private async Task<AddOrderTruckResult> AddOrderLineTruckInternal(AddOrderLineTruckInternalInput input)
        {
            var scheduleOrderLine = await GetScheduleOrders(
                    (await _orderLineRepository.GetQueryAsync())
                        .Where(x => x.Id == input.OrderLineId)
                )
                .FirstAsync();

            //we'll be using UTC for internal logic
            //ConvertScheduleOrderTimesFromUtc(scheduleOrderLine, await GetTimezone());

            var isOrderForPast = scheduleOrderLine.Date < await GetToday();

            var truck = (await GetScheduleTrucksFromCache(
                (await _truckRepository.GetQueryAsync())
                    .Where(x => x.Id == input.TruckId),
                t => t.Id == input.TruckId,
                new GetScheduleInput
                {
                    OfficeId = scheduleOrderLine.OfficeId,
                    Date = scheduleOrderLine.Date,
                    Shift = scheduleOrderLine.Shift,
                },
                await SettingManager.UseShifts(),
                await FeatureChecker.IsEnabledAsync(AppFeatures.AllowLeaseHaulersFeature),
                skipTruckFiltering: true)).First();

            if (truck.VehicleCategory.AssetType == AssetType.Trailer)
            {
                truck.Utilization = 0;
            }

            var utilization = Math.Min(input.Utilization, await GetRemainingTruckUtilizationForOrderLineAsync(scheduleOrderLine, truck));
            if (utilization <= 0 && !isOrderForPast)
            {
                return new AddOrderTruckResult
                {
                    IsFailed = true,
                    ErrorMessage = "Truck or Order is fully utilized",
                };
            }

            if (scheduleOrderLine.Utilization > 0
                && (scheduleOrderLine.IsMaterialPriceOverridden || scheduleOrderLine.IsFreightPriceOverridden)
                && truck.VehicleCategory.AssetType != AssetType.Trailer)
            {
                return new AddOrderTruckResult
                {
                    IsFailed = true,
                    ErrorMessage = L("OrderLineWithOverriddenTotalCanOnlyHaveSingleTicketError"),
                };
            }

            var orderLineTruck = new OrderLineTruck
            {
                OrderLineId = input.OrderLineId,
                TruckId = input.TruckId,
                DriverId = input.DriverId ?? truck.DriverId,
                TrailerId = input.TrailerId ?? truck.Trailer?.Id,
                ParentOrderLineTruckId = input.ParentId,
                Utilization = utilization,
                TimeOnJob = GetTimeOnJobUtcForNewOrderLineTruck(scheduleOrderLine, truck.VehicleCategory),
            };

            await _orderLineTruckRepository.InsertAsync(orderLineTruck);

            if (!isOrderForPast)
            {
                await CreateDriverAssignmentWhenAddingOrderLineTruck(scheduleOrderLine.Date, scheduleOrderLine.Shift, truck, scheduleOrderLine.OfficeId);
            }
            await SaveOrThrowConcurrencyErrorAsync();

            var trailer = truck.Trailer;
            if (orderLineTruck.TrailerId.HasValue && orderLineTruck.TrailerId != truck.Trailer?.Id)
            {
                trailer = await (await _truckRepository.GetQueryAsync())
                    .Where(x => x.Id == orderLineTruck.Id)
                    .Select(x => new ScheduleTruckTrailerDto
                    {
                        Id = x.Id,
                        TruckCode = x.TruckCode,
                        VehicleCategory = new VehicleCategoryDto
                        {
                            Id = x.VehicleCategoryId,
                            Name = x.VehicleCategory.Name,
                            AssetType = x.VehicleCategory.AssetType,
                            IsPowered = x.VehicleCategory.IsPowered,
                            SortOrder = x.VehicleCategory.SortOrder,
                        },
                        Make = x.Make,
                        Model = x.Model,
                        BedConstruction = x.BedConstruction,
                    })
                    .FirstOrDefaultAsync();
            }

            if (truck.LeaseHaulerId.HasValue)
            {
                var hasAvailableLeaseHaulerTruck = await (await _availableLeaseHaulerTruckRepository.GetQueryAsync())
                    .Where(x => x.Date == scheduleOrderLine.Date
                        && x.OfficeId == scheduleOrderLine.OfficeId
                        && x.Shift == scheduleOrderLine.Shift
                        && x.LeaseHaulerId == truck.LeaseHaulerId
                        && x.TruckId == truck.Id
                        && !x.LeaseHaulerRequest.SuppressLeaseHaulerDispatcherNotification
                    )
                    .AnyAsync();

                if (hasAvailableLeaseHaulerTruck)
                {
                    var message = "{CompanyName} has assigned your trucks to jobs. Please visit the {LinkToSchedule} to see the specifics.";
                    var notificationData = new NotifyLeaseHaulerInput
                    {
                        LeaseHaulerId = truck.LeaseHaulerId.Value,
                        Message = message,
                    };
                    await _leaseHaulerNotifier.NotifyLeaseHaulerDispatchers(notificationData);
                }
            }

            return new AddOrderTruckResult
            {
                Item = new ScheduleOrderLineTruckDto
                {
                    Id = orderLineTruck.Id,
                    ParentId = orderLineTruck.ParentOrderLineTruckId,
                    TruckId = orderLineTruck.TruckId,
                    TruckCode = truck.TruckCode,
                    Trailer = orderLineTruck.TrailerId.HasValue ? trailer : null,
                    DriverId = orderLineTruck.DriverId,
                    OrderId = scheduleOrderLine.OrderId,
                    OrderLineId = orderLineTruck.OrderLineId,
                    IsExternal = truck.IsExternal,
                    OfficeId = truck.OfficeId,
                    Utilization = orderLineTruck.Utilization,
                    VehicleCategory = new VehicleCategoryDto
                    {
                        Id = truck.VehicleCategory.Id,
                        Name = truck.VehicleCategory.Name,
                        AssetType = truck.VehicleCategory.AssetType,
                        IsPowered = truck.VehicleCategory.IsPowered,
                        SortOrder = truck.VehicleCategory.SortOrder,
                    },
                    AlwaysShowOnSchedule = truck.AlwaysShowOnSchedule,
                    CanPullTrailer = truck.CanPullTrailer,
                    IsDone = false,
                    TimeOnJob = orderLineTruck.TimeOnJob ?? scheduleOrderLine.Time,
                    LeaseHaulerId = truck.LeaseHaulerId,
                    Dispatches = new List<ScheduleOrderLineTruckDispatchDto>(),
                },
                OrderUtilization = scheduleOrderLine.Utilization + (truck.VehicleCategory.IsPowered ? utilization : 0),
            };
        }

        private static DateTime? GetTimeOnJobUtcForNewOrderLineTruck(ScheduleOrderLineDto orderLine, VehicleCategoryDto vehicleCategory)
        {
            var lastTruckTimeOnJob = orderLine.Trucks
                .Where(t => t.TimeOnJob != null && t.VehicleCategory.AssetType != AssetType.Trailer)
                .OrderByDescending(t => t.Id)
                .Select(t => t.TimeOnJob)
                .FirstOrDefault();

            if (orderLine.StaggeredTimeKind == StaggeredTimeKind.None)
            {
                return orderLine.Time;
            }

            if (orderLine.StaggeredTimeKind == StaggeredTimeKind.SetInterval)
            {
                if (lastTruckTimeOnJob == null)
                {
                    return orderLine.FirstStaggeredTimeOnJob;
                }
                if (vehicleCategory.AssetType == AssetType.Trailer)
                {
                    return lastTruckTimeOnJob;
                }
                return lastTruckTimeOnJob?.AddMinutes(orderLine.StaggeredTimeInterval ?? 0);
            }

            return null;
        }

        [AbpAuthorize(AppPermissions.Pages_Schedule)]
        public async Task<decimal> GetTruckUtilization(GetTruckUtilizationInput input)
        {
            return await (await _orderLineTruckRepository.GetQueryAsync())
                .Where(olt => olt.TruckId == input.TruckId
                              && olt.OrderLine.Order.DeliveryDate == input.Date
                              && olt.OrderLine.Order.Shift == input.Shift)
                .Select(olt => olt.Utilization)
                .FirstOrDefaultAsync();
        }

        [AbpAuthorize(AppPermissions.Pages_Schedule)]
        public async Task<bool> IsTrailerAssignedToAnotherTruck(IsTrailerAssignedToAnotherTruckInput input)
        {
            return await (await _orderLineTruckRepository.GetQueryAsync())
                .Where(olt => olt.TrailerId == input.TrailerId
                              && olt.TruckId != input.TruckId
                              && olt.OrderLine.Order.DeliveryDate == input.Date
                              && olt.OrderLine.Order.Shift == input.Shift
                              && !olt.IsDone)
                .AnyAsync();
        }

        private async Task CreateDriverAssignmentWhenAddingOrderLineTruck(DateTime date, Shift? shift, ScheduleTruckDto truck, int officeId)
        {
            if (!truck.VehicleCategory.IsPowered
                || !await FeatureChecker.AllowLeaseHaulersFeature() && truck.AlwaysShowOnSchedule)
            {
                return;
            }

            var truckId = truck.Id;

            if (await DriverAssignmentWithDriverExists())
            {
                return;
            }

            int? defaultDriverId = await GetDriverIdFromAvailableLeaseHaulerTruck() ?? await GetDefaultDriverId();
            if (defaultDriverId == null && !await SettingManager.GetSettingValueAsync<bool>(AppSettings.DispatchingAndMessaging.AllowSchedulingTrucksWithoutDrivers))
            {
                throw new UserFriendlyException("Cannot add OrderLineTruck for a truck without a default driver!");
            }
            await CreateDriverAssignment();

            // Local functions
            async Task<bool> DriverAssignmentWithDriverExists()
            {
                var driverAssignment = await (await _driverAssignmentRepository.GetQueryAsync())
                    .Where(da => da.TruckId == truckId && da.Date == date && da.Shift == shift)
                    .Select(x => new
                    {
                        x.Id,
                        x.DriverId,
                    })
                    .OrderByDescending(x => x.Id)
                    .FirstOrDefaultAsync();

                if (driverAssignment != null)
                {
                    if (driverAssignment.DriverId == null && !await SettingManager.GetSettingValueAsync<bool>(AppSettings.DispatchingAndMessaging.AllowSchedulingTrucksWithoutDrivers))
                    {
                        throw new UserFriendlyException("Unable to add or move truck since it has no driver assigned to it");
                    }
                    return true;
                }
                return false;
            }

            async Task<int?> GetDriverIdFromAvailableLeaseHaulerTruck()
            {
                if (!await FeatureChecker.AllowLeaseHaulersFeature() || !truck.IsExternal)
                {
                    return null;
                }
                return (await (await _availableLeaseHaulerTruckRepository.GetQueryAsync())
                    .Where(alht => alht.TruckId == truckId && alht.Date == date && alht.Shift == shift)
                    .Select(alht => new { alht.DriverId })
                    .FirstOrDefaultAsync())?.DriverId;
            }

            async Task<int?> GetDefaultDriverId() => await (await _truckRepository.GetQueryAsync()).Where(t => t.Id == truckId).Select(t => t.DefaultDriverId).FirstOrDefaultAsync();

            async Task CreateDriverAssignment()
            {
                var driverAssignment = new DriverAssignment
                {
                    Date = date,
                    Shift = shift,
                    DriverId = defaultDriverId,
                    OfficeId = officeId,
                    TruckId = truckId,
                };
                await _driverAssignmentRepository.InsertAsync(driverAssignment);

                await CurrentUnitOfWork.SaveChangesAsync();

                await _syncRequestSender.SendSyncRequest(new SyncRequest()
                    .AddChange(EntityEnum.DriverAssignment, driverAssignment.ToChangedEntity())
                    .AddLogMessage("Created driver assignment when adding order line truck"));
            }
        }

        [AbpAuthorize(AppPermissions.Pages_Schedule)]
        public async Task DeleteOrderLineTrucks(DeleteOrderLineTrucksInput input)
        {
            await _orderLineScheduledTrucksUpdater.DeleteOrderLineTrucks(input);
        }

        [AbpAuthorize(AppPermissions.Pages_Schedule)]
        public async Task<DeleteOrderLineTruckResult> DeleteOrderLineTruck(DeleteOrderLineTruckInput input)
        {
            var orderLineTruckDetails = await (await _orderLineTruckRepository.GetQueryAsync())
                .Where(x => x.Id == input.OrderLineTruckId)
                .Select(x => new
                {
                    x.TruckId,
                    x.OrderLineId,
                    x.OrderLine.Order.DeliveryDate,
                }).FirstOrDefaultAsync();

            var orderLineId = orderLineTruckDetails?.OrderLineId;

            //this was happening too often because an OrderLineTruck is already deleted by the time this was called
            //instead of throwing an exception, we'll silently skip the deletion and return the current utilization
            if (orderLineTruckDetails != null)
            {
                await ThrowIfTruckHasDispatches(input);

                await _dispatchingAppService.CancelOrEndAllDispatches(new CancelOrEndAllDispatchesInput
                {
                    OrderLineId = orderLineTruckDetails.OrderLineId,
                    TruckId = orderLineTruckDetails.TruckId,
                });

                if (input.MarkAsDone)
                {
                    var orderLineTrucks = await (await _orderLineTruckRepository.GetQueryAsync())
                        .Where(x => x.Id == input.OrderLineTruckId || x.ParentOrderLineTruckId == input.OrderLineTruckId)
                        .ToListAsync();
                    foreach (var orderLineTruck in orderLineTrucks)
                    {
                        orderLineTruck.IsDone = true;
                        orderLineTruck.Utilization = 0;
                    }
                }
                else
                {
                    await _orderLineTruckRepository.DeleteAsync(x => x.Id == input.OrderLineTruckId || x.ParentOrderLineTruckId == input.OrderLineTruckId);
                    await CurrentUnitOfWork.SaveChangesAsync();
                    if (orderLineTruckDetails.DeliveryDate >= await GetToday())
                    {
                        var orderLineUpdater = _orderLineUpdaterFactory.Create(orderLineTruckDetails.OrderLineId);
                        orderLineUpdater.UpdateStaggeredTimeOnTrucksOnSave();
                        await orderLineUpdater.SaveChangesAsync();
                    }
                }
                await SaveOrThrowConcurrencyErrorAsync();
            }
            else
            {
                // It's fine to trust user input in this case since we're basically just returning the utilization for the order line that they request and are not editing data based on potentially mismatching ids
#pragma warning disable CS0618 // Type or member is obsolete
                orderLineId = input.OrderLineId;
#pragma warning restore CS0618 // Type or member is obsolete
            }

            var orderLineUtilization = orderLineId == 0 ? 0 : await (await _orderLineRepository.GetQueryAsync())
                .Where(ol => ol.Id == orderLineId)
                .Select(ol => ol.OrderLineTrucks.Where(t => t.Truck.VehicleCategory.IsPowered).Sum(olt => olt.Utilization))
                .FirstAsync();

            return new DeleteOrderLineTruckResult
            {
                OrderLineUtilization = orderLineUtilization,
            };
        }

        private async Task ThrowIfTruckHasDispatches(DeleteOrderLineTruckInput input)
        {
            var hasDispatches = await HasDispatches(input);
            if (hasDispatches.AcknowledgedOrLoaded)
            {
                throw new UserFriendlyException(L("TruckHasDispatch_YouMustCancelItFirstToRemoveTruck", hasDispatches.TruckCode));
            }
        }

        [AbpAuthorize(AppPermissions.Pages_Schedule)]
        public async Task<MoveTruckResult> MoveTruck(MoveTruckInput input)
        {
            var sourceOrderLineTruck = await (await _orderLineTruckRepository.GetQueryAsync())
                .Where(x => x.Id == input.SourceOrderLineTruckId)
                .Select(x => new
                {
                    x.Id,
                    x.TruckId,
                    x.Utilization,
                    x.TrailerId,
                    x.DriverId,
                    x.ParentOrderLineTruckId,
                    x.IsDone,
                    x.OrderLine.OrderId,
                }).FirstOrDefaultAsync();

            if (sourceOrderLineTruck == null)
            {
                throw await GetOrderLineTruckNotFoundException(new EntityDto(input.SourceOrderLineTruckId));
            }

            var order = await (await _orderLineTruckRepository.GetQueryAsync())
                .Where(olt => olt.Id == input.SourceOrderLineTruckId)
                .Select(ol => new
                {
                    ol.OrderLine.Order.DeliveryDate,
                })
                .FirstOrDefaultAsync();

            if (order == null)
            {
                throw await GetOrderNotFoundException(new EntityDto(sourceOrderLineTruck.OrderId));
            }

            var orderDate = order.DeliveryDate;

            var today = await GetToday();
            if (orderDate < today)
            {
                throw new UserFriendlyException("You cannot move trucks for past orders");
            }
            bool markAsDone = today == orderDate;

            var result = new MoveTruckResult();

            var existingDestinationOrderLineTruck = await (await _orderLineTruckRepository.GetQueryAsync())
                .Where(olt => olt.OrderLineId == input.DestinationOrderLineId && olt.TruckId == sourceOrderLineTruck.TruckId)
                .Select(x => new
                {
                    x.Id,
                    x.IsDone,
                })
                .FirstOrDefaultAsync();

            if (existingDestinationOrderLineTruck?.IsDone == false)
            {
                result.OrderLineTruckExists = true;
                return result;
            }

            var utilization = !sourceOrderLineTruck.IsDone ? sourceOrderLineTruck.Utilization : 1;

            await DeleteOrderLineTruck(new DeleteOrderLineTruckInput
            {
                OrderLineTruckId = sourceOrderLineTruck.Id,
                MarkAsDone = markAsDone,
            });

            if (existingDestinationOrderLineTruck == null)
            {
                var addOrderTruckResult = await AddOrderLineTruckInternal(new AddOrderLineTruckInternalInput
                {
                    OrderLineId = input.DestinationOrderLineId,
                    TruckId = sourceOrderLineTruck.TruckId,
                    TrailerId = sourceOrderLineTruck.TrailerId,
                    DriverId = sourceOrderLineTruck.DriverId,
                    ParentId = sourceOrderLineTruck.ParentOrderLineTruckId,
                    Utilization = utilization,
                });
                if (addOrderTruckResult.IsFailed)
                {
                    throw new UserFriendlyException(addOrderTruckResult.ErrorMessage);
                }
            }
            else
            {
                await ActivateClosedTrucks(new ActivateClosedTrucksInput
                {
                    OrderLineId = input.DestinationOrderLineId,
                    TruckIds = new[] { sourceOrderLineTruck.TruckId },
                });

                await SetOrderTruckUtilization(new OrderLineTruckUtilizationEditDto
                {
                    OrderLineId = input.DestinationOrderLineId,
                    OrderLineTruckId = existingDestinationOrderLineTruck.Id,
                    Utilization = utilization,
                });
            }

            return result;
        }

        [AbpAuthorize(AppPermissions.Pages_Schedule)]
        public async Task<HasDispatchesResult> HasDispatches(DeleteOrderLineTruckInput input)
        {
            var orderLineTruck = await (await _orderLineTruckRepository.GetQueryAsync())
                .Where(olt => olt.Id == input.OrderLineTruckId)
                .Select(olt => new
                {
                    olt.TruckId,
                    olt.Truck.TruckCode,
                    Dispatches = olt.OrderLine.Dispatches.Where(x => x.TruckId == olt.TruckId).Select(d => new
                    {
                        d.Status,
                    }).ToList(),
                }).FirstOrDefaultAsync();

            return new HasDispatchesResult
            {
                TruckCode = orderLineTruck?.TruckCode ?? string.Empty,
                Unacknowledged = orderLineTruck?.Dispatches.Any(d => d.Status.IsIn(DispatchStatus.Created, DispatchStatus.Sent)) ?? false,
                AcknowledgedOrLoaded = orderLineTruck?.Dispatches.Any(d => d.Status.IsIn(DispatchStatus.Acknowledged, DispatchStatus.Loaded)) ?? false,
            };

            //await (await _orderLineTruckRepository.GetQueryAsync())
            //    .Where(olt => olt.Id == input.OrderLineTruckId)
            //    .Select(olt => new HasDispatchesResult
            //    {
            //        Unacknowledged = olt.OrderLine.Dispatches.Any(d => (d.Status == DispatchStatus.Created || d.Status == DispatchStatus.Sent) && d.Truck.OrderLineTrucks.Any(dolt => dolt.Id == input.OrderLineTruckId)),
            //        AcknowledgedOrLoaded = olt.OrderLine.Dispatches.Any(d => (d.Status == DispatchStatus.Acknowledged || d.Status == DispatchStatus.Loaded) && d.Truck.OrderLineTrucks.Any(dolt => dolt.Id == input.OrderLineTruckId))
            //    })
            //    .FirstAsync();
        }

        [AbpAuthorize(AppPermissions.Pages_Schedule)]
        public async Task<List<HasDispatchesResult>> OrderLineHasDispatches(OrderLineHasDispatchesInput input)
        {
            var existingDispatches = await (await _orderLineTruckRepository.GetQueryAsync())
                    .Where(olt => olt.OrderLineId == input.OrderLineId)
                    .Select(olt => new
                    {
                        olt.TruckId,
                        olt.Truck.TruckCode,
                        Dispatches = olt.OrderLine.Dispatches.Where(x => x.TruckId == olt.TruckId).Select(d => new
                        {
                            d.Status,
                        }).ToList(),
                    }).ToListAsync();

            return existingDispatches.Select(x => new HasDispatchesResult
            {
                TruckCode = x.TruckCode,
                Unacknowledged = x.Dispatches.Any(d => d.Status.IsIn(DispatchStatus.Created, DispatchStatus.Sent)),
                AcknowledgedOrLoaded = x.Dispatches.Any(d => d.Status.IsIn(DispatchStatus.Acknowledged, DispatchStatus.Loaded)),
            }).ToList();
        }

        [AbpAuthorize(AppPermissions.Pages_Schedule)]
        public async Task<HasDispatchesResult> SomeOrderLineTrucksHaveDispatches(SomeOrderLineTrucksHaveDispatchesInput input)
        {
            var dispatchQuery = (await _orderLineTruckRepository.GetQueryAsync())
                .Where(olt => olt.OrderLineId == input.OrderLineId && input.TruckIds.Contains(olt.TruckId))
                .SelectMany(olt => olt.OrderLine.Dispatches);
            return new HasDispatchesResult
            {
                Unacknowledged = await dispatchQuery.AnyAsync(d => d.Status == DispatchStatus.Created || d.Status == DispatchStatus.Sent),
                AcknowledgedOrLoaded = await dispatchQuery.AnyAsync(d => d.Status == DispatchStatus.Acknowledged || d.Status == DispatchStatus.Loaded),
            };
        }

        [AbpAuthorize(AppPermissions.Pages_Schedule)]
        public async Task<bool> IsOrderLineFieldReadonly(IsOrderLineFieldReadonlyInput input)
        {
            var readonlyChecker = _orderLineReadonlyCheckerFactory.Create(input.OrderLineId);
            return await readonlyChecker.IsFieldReadonlyAsync(input.FieldName);
        }


        private class RemainingTruckUtilizationAndNumber
        {
            public decimal RemainingUtilization { get; set; }
            public int TruckNumber { get; set; }
        }

        [UnitOfWork(IsDisabled = true)]
        [AbpAuthorize(AppPermissions.Pages_Schedule, AppPermissions.Pages_Orders_Edit, RequireAllPermissions = true)]
        public async Task<CopyOrderTrucksResult> CopyOrdersTrucks(CopyOrdersTrucksInput input)
        {
            return await UnitOfWorkManager.WithUnitOfWorkNoCompleteAsync(new UnitOfWorkOptions { IsTransactional = true }, async unitOfWork =>
            {
                var allResult = new CopyOrderTrucksResult
                {
                    Completed = true,
                };

                foreach (var newOrderId in input.NewOrderIds)
                {
                    var result = await CopyOrderTrucksInternal(new CopyOrderTrucksInput
                    {
                        NewOrderId = newOrderId,
                        OriginalOrderId = input.OriginalOrderId,
                        OrderLineId = input.OrderLineId,
                        ProceedOnConflict = input.ProceedOnConflict,
                    });
                    if (!result.Completed)
                    {
                        allResult.Completed = false;
                        allResult.ConflictingTrucks ??= new List<string>();
                        allResult.ConflictingTrucks.AddRange(result.ConflictingTrucks.Where(x => !allResult.ConflictingTrucks.Contains(x)));
                    }
                    allResult.SomeTrucksAreNotCopied = allResult.SomeTrucksAreNotCopied || result.SomeTrucksAreNotCopied;
                }

                if (allResult.Completed)
                {
                    await unitOfWork.CompleteAsync();
                }

                return allResult;
            });
        }

        [AbpAuthorize(AppPermissions.Pages_Schedule, AppPermissions.Pages_Orders_Edit, RequireAllPermissions = true)]
        public async Task<CopyOrderTrucksResult> CopyOrderTrucks(CopyOrderTrucksInput input)
        {
            return await CopyOrderTrucksInternal(input);
        }

        private async Task<CopyOrderTrucksResult> CopyOrderTrucksInternal(CopyOrderTrucksInput input)
        {
            var result = new CopyOrderTrucksResult();
            var timezone = await GetTimezone();

            var newOrder = await (await _orderRepository.GetQueryAsync())
                .Where(x => x.Id == input.NewOrderId)
                .Select(x => new
                {
                    x.OfficeId,
                    x.DeliveryDate,
                    x.Shift,
                })
                .FirstAsync();

            var originalOrderLines = await (await _orderLineRepository.GetQueryAsync())
                .Where(ol => ol.OrderId == input.OriginalOrderId)
                .WhereIf(input.OrderLineId.HasValue, ol => ol.Id == input.OrderLineId.Value)
                .Select(x => new
                {
                    x.Id,
                    x.LineNumber,
                })
                .ToListAsync();

            var newOrderLines = await (await _orderLineRepository.GetQueryAsync())
                .Where(ol => ol.OrderId == input.NewOrderId)
                .Select(x => new
                {
                    x.Id,
                    x.LineNumber,
                })
                .ToListAsync();

            var originalOrderLineTrucks = await (await _orderLineTruckRepository.GetQueryAsync())
                .Include(x => x.Truck)
                .AsNoTracking()
                .Where(x => x.OrderLine.OrderId == input.OriginalOrderId)
                .WhereIf(input.OrderLineId.HasValue, olt => olt.OrderLineId == input.OrderLineId.Value)
                .ToListAsync();

            var originalTrucksIds = originalOrderLineTrucks.Select(x => x.TruckId).ToList();

            var trucks = (await GetScheduleTrucksFromCache(
                (await _truckRepository.GetQueryAsync())
                    .Where(t => originalTrucksIds.Contains(t.Id)),
                t => originalTrucksIds.Contains(t.Id),
                new GetScheduleInput
                {
                    OfficeId = newOrder.OfficeId,
                    Date = newOrder.DeliveryDate,
                    Shift = newOrder.Shift,
                },
                await SettingManager.UseShifts(),
                await FeatureChecker.IsEnabledAsync(AppFeatures.AllowLeaseHaulersFeature),
                skipTruckFiltering: false)
            ).ToList();

            var truckInfos = await (await _truckRepository.GetQueryAsync())
                .Where(t => originalTrucksIds.Contains(t.Id))
                .Select(t => new
                {
                    t.Id,
                    t.DefaultDriverId,
                    t.CurrentTrailerId,
                    t.OfficeId,
                }).ToListAsync();

            var existingDriverAssignments = await (await _driverAssignmentRepository.GetQueryAsync())
                .Where(x => x.Date == newOrder.DeliveryDate && x.Shift == newOrder.Shift)
                .ToListAsync();

            var passedOrderLineTrucks = new List<OrderLineTruck>();

            var truckRemainingUtilizationQuery =
                from olt in originalOrderLineTrucks
                group olt by olt.TruckId
                into truckGroup
                select new
                {
                    TruckId = truckGroup.Key,
                    RemainingUtilization = 1 - truckGroup.Sum(x => x.Utilization),
                    TruckNumber = truckGroup.Count(),
                };
            var truckRemainingUtilizationDictionary = truckRemainingUtilizationQuery.ToDictionary(x => x.TruckId, x => new RemainingTruckUtilizationAndNumber { RemainingUtilization = x.RemainingUtilization, TruckNumber = x.TruckNumber });

            foreach (var originalOrderLineTruck in originalOrderLineTrucks.ToList())
            {
                var truck = trucks.FirstOrDefault(x => x.Id == originalOrderLineTruck.TruckId);
                var truckInfo = truckInfos.FirstOrDefault(x => x.Id == originalOrderLineTruck.TruckId);
                if (truck == null || truck.IsOutOfService || !truck.IsActive || truckInfo == null)
                {
                    originalOrderLineTrucks.Remove(originalOrderLineTruck);
                    continue;
                }

                if (await AllowLeaseHaulerAndTruckIsLeaseHauler(truck))
                {
                    originalOrderLineTrucks.Remove(originalOrderLineTruck);
                    continue;
                }

                int? newDriverId = null;
                if (truck.VehicleCategory.AssetType != AssetType.Trailer && truck.VehicleCategory.IsPowered)
                {
                    var existingDriverAssignment = existingDriverAssignments.FirstOrDefault(da => da.TruckId == truck.Id);
                    if (existingDriverAssignment != null)
                    {
                        newDriverId = existingDriverAssignment.DriverId;
                    }
                    else
                    {
                        newDriverId = truckInfo.DefaultDriverId;
                        var newDriverAssignment = new DriverAssignment
                        {
                            Date = newOrder.DeliveryDate,
                            Shift = newOrder.Shift,
                            DriverId = truckInfo.DefaultDriverId,
                            TruckId = truck.Id,
                            //StartTime = originalDriverAssignment.StartTime.HasValue
                            //        ? newOrder.Date.Date.Add(originalDriverAssignment.StartTime.Value.TimeOfDay)
                            //        : (DateTime?)null,
                            OfficeId = truckInfo.OfficeId, //originalDriverAssignment.OfficeId,
                            //Note = originalDriverAssignment.Note,
                        };
                        existingDriverAssignments.Add(newDriverAssignment);
                        await _driverAssignmentRepository.InsertAsync(newDriverAssignment);
                    }
                }

                int? newTrailerId = null;
                if (truck.CanPullTrailer)
                {
                    newTrailerId = truckInfo.CurrentTrailerId;
                }

                int newOrderLineId = MapOriginalOrderLineIdToNewOrderLineId(originalOrderLineTruck.OrderLineId);
                var newOrderLine = await (await _orderLineRepository.GetQueryAsync()) //todo use cache?
                    .Where(ol => ol.Id == newOrderLineId)
                    .Select(x => new
                    {
                        Utilization = x.OrderLineTrucks.Where(t => t.Truck.VehicleCategory.IsPowered).Select(t => t.Utilization).Sum(),
                        x.ScheduledTrucks,
                        x.NumberOfTrucks,
                    })
                    .FirstAsync();

                var validateUtilization = await SettingManager.GetSettingValueAsync<bool>(AppSettings.DispatchingAndMessaging.ValidateUtilization);
                var remainingUtilization = await GetRemainingTruckUtilizationForOrderAsync(new GetRemainingTruckUtilizationForOrderInput
                {
                    OrderUtilization = newOrderLine.Utilization,
                    OrderMaxUtilization = newOrderLine.ScheduledTrucks.HasValue ? Convert.ToDecimal(newOrderLine.ScheduledTrucks.Value) : 0,
                    OrderRequestedNumberOfTrucks = newOrderLine.NumberOfTrucks.HasValue ? Convert.ToDecimal(newOrderLine.NumberOfTrucks.Value) : 0,
                    TruckUtilization = truck.Utilization,
                    AssetType = truck.VehicleCategory.AssetType,
                    IsPowered = truck.VehicleCategory.IsPowered,
                });
                if (validateUtilization && (remainingUtilization < originalOrderLineTruck.Utilization || remainingUtilization == 0))
                {
                    continue;
                }

                if (truck.VehicleCategory.IsPowered && truck.VehicleCategory.AssetType != AssetType.Trailer
                    && !truck.IsExternal && !truck.AlwaysShowOnSchedule
                    && (
                        existingDriverAssignments.Any(da => da.TruckId == originalOrderLineTruck.TruckId && da.DriverId == null)
                        || !truck.HasDefaultDriver && !existingDriverAssignments.Any(da => da.TruckId == originalOrderLineTruck.TruckId && da.DriverId != null)
                    )
                    && validateUtilization
                )
                {
                    result.SomeTrucksAreNotCopied = true;
                    originalOrderLineTrucks.Remove(originalOrderLineTruck);
                    continue;
                }

                var utilizationToAssign = originalOrderLineTruck.IsDone ?
                        GetTruckUtilization(truck.Id, remainingUtilization) : originalOrderLineTruck.Utilization;

                if (utilizationToAssign == 0)
                {
                    if (validateUtilization)
                    {
                        continue;
                    }
                    utilizationToAssign = 1;
                }

                passedOrderLineTrucks.Add(new OrderLineTruck
                {
                    OrderLineId = newOrderLineId,
                    TruckId = originalOrderLineTruck.TruckId,
                    DriverId = newDriverId,
                    TrailerId = newTrailerId,
                    Utilization = utilizationToAssign,
                    TimeOnJob = newOrder.DeliveryDate.AddTimeOrNull(originalOrderLineTruck.TimeOnJob?.ConvertTimeZoneTo(timezone))?.ConvertTimeZoneFrom(timezone),
                });

                originalOrderLineTrucks.Remove(originalOrderLineTruck);
            }

            if (originalOrderLineTrucks.Any())
            {
                result.ConflictingTrucks = originalOrderLineTrucks.Select(x => x.Truck.TruckCode).ToList();
                if (!input.ProceedOnConflict)
                {
                    result.Completed = false;
                    return result;
                }
            }

            foreach (var passedOrderTruck in passedOrderLineTrucks)
            {
                await _orderLineTruckRepository.InsertAsync(passedOrderTruck);
            }

            result.Completed = true;
            return result;

            // Local functions
            int MapOriginalOrderLineIdToNewOrderLineId(int originalOrderLineId)
            {
                if (input.OrderLineId.HasValue)
                {
                    Debug.Assert(newOrderLines.Count == 1);
                    return newOrderLines.Single().Id;
                }
                var originalOrderLine = originalOrderLines.Single(ol => ol.Id == originalOrderLineId);
                return newOrderLines.Where(ol => ol.LineNumber == originalOrderLine.LineNumber).Select(ol => ol.Id).Single();
            }

            decimal GetTruckUtilization(int truckId, decimal remainingOrderUtilization)
            {
                var truckRemainingUtilization = truckRemainingUtilizationDictionary[truckId];
                if (truckRemainingUtilization.TruckNumber == 0)
                {
                    return 0;
                }
                var utilization = truckRemainingUtilization.TruckNumber == 1 ?
                    truckRemainingUtilization.RemainingUtilization :
                    Math.Round(truckRemainingUtilization.RemainingUtilization / truckRemainingUtilization.TruckNumber, 2);

                if (utilization > remainingOrderUtilization)
                {
                    utilization = remainingOrderUtilization;
                }

                truckRemainingUtilization.RemainingUtilization -= utilization;
                truckRemainingUtilization.TruckNumber--;

                return utilization;
            }

            async Task<bool> AllowLeaseHaulerAndTruckIsLeaseHauler(ScheduleTruckDto truck) =>
                await FeatureChecker.IsEnabledAsync(AppFeatures.AllowLeaseHaulersFeature) && (truck.IsExternal || truck.AlwaysShowOnSchedule);
        }

        private async Task DeleteOrderLineTrucksIfQuantityAndNumberOfTrucksAreZero(OrderLine orderLine)
        {
            if ((orderLine.MaterialQuantity ?? 0) == 0 && (orderLine.FreightQuantity ?? 0) == 0 && (orderLine.NumberOfTrucks ?? 0) < 0.01)
            {
                await _orderLineScheduledTrucksUpdater.DeleteOrderLineTrucks(new DeleteOrderLineTrucksInput
                {
                    OrderLineId = orderLine.Id,
                });
                await CurrentUnitOfWork.SaveChangesAsync();
            }
        }

        [AbpAuthorize(AppPermissions.Pages_Schedule)]
        public async Task<SetOrderLineScheduledTrucksResult> SetOrderLineScheduledTrucks(SetOrderLineScheduledTrucksInput input)
        {
            var order = await (await _orderRepository.GetQueryAsync())
                .FirstAsync(o => o.OrderLines.Any(ol => ol.Id == input.OrderLineId));

            var orderLine = await (await _orderLineRepository.GetQueryAsync())
                .FirstAsync(ol => ol.Id == input.OrderLineId);

            await _orderLineScheduledTrucksUpdater.UpdateScheduledTrucks(orderLine, input.ScheduledTrucks);

            return new SetOrderLineScheduledTrucksResult
            {
                ScheduledTrucks = orderLine.ScheduledTrucks,
                OrderUtilization = await _orderLineScheduledTrucksUpdater.GetOrderLineUtilization(orderLine.Id),
            };
        }

        [AbpAuthorize(AppPermissions.Pages_Schedule)]
        public async Task<SetOrderLineMaterialQuantityResult> SetOrderLineMaterialQuantity(SetOrderLineMaterialQuantityInput input)
        {
            var order = await (await _orderRepository.GetQueryAsync())
                .Include(o => o.OrderLines)
                .Where(o => o.OrderLines.Any(ol => ol.Id == input.OrderLineId))
                .FirstAsync();
            var orderLine = order.OrderLines.First(ol => ol.Id == input.OrderLineId);
            if (orderLine.MaterialQuantity != input.MaterialQuantity)
            {
                orderLine.MaterialQuantity = input.MaterialQuantity;

                if (!orderLine.IsMaterialPriceOverridden)
                {
                    await EnsureOrderIsNotPaid(order.Id);
                    orderLine.MaterialPrice = orderLine.MaterialPricePerUnit * input.MaterialQuantity ?? 0;
                }

                if (!orderLine.IsQuantityValid())
                {
                    throw new UserFriendlyException(L("QuantityIsRequiredWhenTotalIsSpecified"));
                }

                await CurrentUnitOfWork.SaveChangesAsync();

                await _orderTaxCalculator.CalculateTotalsAsync(order.Id);

                await DeleteOrderLineTrucksIfQuantityAndNumberOfTrucksAreZero(orderLine);
            }

            return new SetOrderLineMaterialQuantityResult
            {
                MaterialQuantity = orderLine.MaterialQuantity,
            };
        }

        [AbpAuthorize(AppPermissions.Pages_Schedule)]
        public async Task<SetOrderLineFreightQuantityResult> SetOrderLineFreightQuantity(SetOrderLineFreightQuantityInput input)
        {
            var order = await (await _orderRepository.GetQueryAsync())
                .Include(o => o.OrderLines)
                .Where(o => o.OrderLines.Any(ol => ol.Id == input.OrderLineId))
                .FirstAsync();
            var orderLine = order.OrderLines.First(ol => ol.Id == input.OrderLineId);
            if (orderLine.FreightQuantity != input.FreightQuantity)
            {
                orderLine.FreightQuantity = input.FreightQuantity;

                if (!orderLine.IsFreightPriceOverridden)
                {
                    await EnsureOrderIsNotPaid(order.Id);
                    orderLine.FreightPrice = orderLine.FreightPricePerUnit * input.FreightQuantity ?? 0;
                }

                if (!orderLine.IsQuantityValid())
                {
                    throw new UserFriendlyException(L("QuantityIsRequiredWhenTotalIsSpecified"));
                }

                await CurrentUnitOfWork.SaveChangesAsync();

                await _orderTaxCalculator.CalculateTotalsAsync(order.Id);

                await DeleteOrderLineTrucksIfQuantityAndNumberOfTrucksAreZero(orderLine);
            }

            return new SetOrderLineFreightQuantityResult
            {
                FreightQuantity = orderLine.FreightQuantity,
            };
        }

        //for any office
        private async Task<bool> IsOrderPaid(int orderId)
        {
            return await (await _orderRepository.GetQueryAsync())
                    .Where(x => x.Id == orderId)
                    .SelectMany(x => x.OrderPayments)
                    .Select(x => x.Payment)
                    .Where(x => !x.IsCancelledOrRefunded)
                    .AnyAsync(x => x.AuthorizationCaptureDateTime != null);
        }

        private async Task EnsureOrderIsNotPaid(int orderId)
        {
            if (await IsOrderPaid(orderId))
            {
                throw new UserFriendlyException(L("CannotChangeRatesAndAmountBecauseOrderIsPaid"));
            }
        }

        [AbpAuthorize(AppPermissions.Pages_Schedule)]
        public async Task<SetOrderLineLoadsResult> SetOrderLineLoads(SetOrderLineLoadsInput input)
        {
            var orderLine = await (await _orderLineRepository.GetQueryAsync())
                .Include(ol => ol.MaterialUom)
                .FirstAsync(ol => ol.Id == input.OrderLineId)
            ;
            orderLine.Loads = input.Loads;
            if (orderLine.MaterialUom.Name.Equals("tons", StringComparison.InvariantCultureIgnoreCase)
                || orderLine.MaterialUom.Name.Equals("ton", StringComparison.InvariantCultureIgnoreCase))
            {
                orderLine.EstimatedAmount = input.Loads * 20;
            }
            if (orderLine.MaterialUom.Name.Equals("loads", StringComparison.InvariantCultureIgnoreCase)
                || orderLine.MaterialUom.Name.Equals("load", StringComparison.InvariantCultureIgnoreCase))
            {
                orderLine.EstimatedAmount = input.Loads;
            }
            if (orderLine.MaterialUom.Name.Equals("hours", StringComparison.InvariantCultureIgnoreCase)
                || orderLine.MaterialUom.Name.Equals("hour", StringComparison.InvariantCultureIgnoreCase))
            {
                orderLine.EstimatedAmount = orderLine.MaterialQuantity / 2;
            }

            return new SetOrderLineLoadsResult
            {
                Loads = orderLine.Loads,
                EstimatedAmount = orderLine.EstimatedAmount,
            };
        }

        [AbpAuthorize(AppPermissions.Pages_Schedule)]
        public async Task SetOrderDirections(SetOrderDirectionsInput input)
        {
            var order = await _orderRepository.GetAsync(input.OrderId);
            order.Directions = input.Directions;
        }

        [AbpAuthorize(AppPermissions.Pages_Schedule)]
        public async Task SetOrderLineNote(SetOrderLineNoteInput input)
        {
            if (!input.OrderLineId.HasValue)
            {
                throw new ArgumentNullException(nameof(input.OrderLineId));
            }

            var orderLineUpdater = _orderLineUpdaterFactory.Create(input.OrderLineId.Value);
            await orderLineUpdater.UpdateFieldAsync(o => o.Note, input.Note);
            await orderLineUpdater.SaveChangesAsync();
        }

        [AbpAuthorize(AppPermissions.Pages_Schedule)]
        public async Task SetOrderLineTime(SetOrderLineTimeInput input)
        {
            var orderLineUpdater = _orderLineUpdaterFactory.Create(input.OrderLineId);

            var order = await orderLineUpdater.GetOrderAsync();
            var date = order.DeliveryDate;
            var timezone = await GetTimezone();

            await orderLineUpdater.UpdateFieldAsync(o => o.TimeOnJob, date.AddTimeOrNull(input.Time)?.ConvertTimeZoneFrom(timezone));

            await orderLineUpdater.SaveChangesAsync();
        }

        [AbpAuthorize(AppPermissions.Pages_Schedule, AppPermissions.LeaseHaulerPortal_Schedule)]
        public async Task SetOrderLineIsComplete(SetOrderLineIsCompleteInput input)
        {
            await CheckOrderLineEditPermissions(AppPermissions.Pages_Schedule, AppPermissions.LeaseHaulerPortal_Schedule,
                _orderLineRepository, input.OrderLineId);

            var leaseHaulerIdFilter = await GetLeaseHaulerIdFilterAsync(AppPermissions.Pages_Schedule, AppPermissions.LeaseHaulerPortal_Schedule);
            var permissions = new
            {
                DispatcherSchedule = await IsGrantedAsync(AppPermissions.Pages_Schedule),
                LeaseHaulerSchedule = await IsGrantedAsync(AppPermissions.LeaseHaulerPortal_Schedule),
            };

            if (input.IsCancelled)
            {
                input.IsComplete = true;
            }

            if (input.IsComplete)
            {
                //CancelOrEndAllDispatches is public and will have its own permissions check and LH filtering
                await _dispatchingAppService.CancelOrEndAllDispatches(new CancelOrEndAllDispatchesInput
                {
                    OrderLineId = input.OrderLineId,
                });
            }

            var orderLineUpdater = _orderLineUpdaterFactory.Create(input.OrderLineId);
            if (permissions.DispatcherSchedule)
            {
                await orderLineUpdater.UpdateFieldAsync(x => x.IsComplete, input.IsComplete);
                await orderLineUpdater.UpdateFieldAsync(x => x.IsCancelled, input.IsComplete && input.IsCancelled);
            }
            var order = await orderLineUpdater.GetOrderAsync();
            var today = await GetToday();

            var orderLineTrucks = await (await _orderLineTruckRepository.GetQueryAsync())
                .Where(x => x.OrderLineId == input.OrderLineId)
                .WhereIf(leaseHaulerIdFilter.HasValue, q => q.Truck.LeaseHaulerTruck.LeaseHaulerId == leaseHaulerIdFilter)
                .ToListAsync();

            await CheckTruckEditPermissions(AppPermissions.Pages_Schedule, AppPermissions.LeaseHaulerPortal_Schedule,
                _truckRepository, orderLineTrucks.Select(x => x.TruckId).ToArray());

            if (input.IsComplete)
            {
                if (input.IsCancelled)
                {
                    var tickets = await (await _ticketRepository.GetQueryAsync())
                        .Where(x => x.OrderLineId == input.OrderLineId)
                        .Select(x => new
                        {
                            x.TruckId,
                        }).ToListAsync();

                    var dispatches = await (await _dispatchRepository.GetQueryAsync())
                        .Where(x => x.OrderLineId == input.OrderLineId && (x.Status == DispatchStatus.Loaded || x.Status == DispatchStatus.Completed))
                        .Select(x => new
                        {
                            x.TruckId,
                            x.Status,
                        }).ToListAsync();

                    foreach (var orderLineTruck in orderLineTrucks)
                    {
                        if (!tickets.Any(t => t.TruckId == orderLineTruck.TruckId) && !dispatches.Any(d => d.TruckId == orderLineTruck.TruckId))
                        {
                            await _orderLineTruckRepository.DeleteAsync(orderLineTruck);
                            if (order.DeliveryDate >= today)
                            {
                                orderLineUpdater.UpdateStaggeredTimeOnTrucksOnSave();
                            }
                        }
                        else
                        {
                            orderLineTruck.IsDone = true;
                            orderLineTruck.Utilization = 0;
                        }
                    }
                }
                else
                {
                    foreach (var orderLineTruck in orderLineTrucks)
                    {
                        orderLineTruck.IsDone = true;
                        orderLineTruck.Utilization = 0;
                    }
                }
            }

            await CurrentUnitOfWork.SaveChangesAsync(); //save deleted OrderLineTrucks first
            await orderLineUpdater.SaveChangesAsync();
        }

        [AbpAuthorize(AppPermissions.LeaseHaulerPortal_Schedule, AppPermissions.LeaseHaulerPortal_Jobs_Accept)]
        public async Task SetLeaseHaulerRequestAsAccepted(SetLeaseHaulerRequestAsAcceptedInput input)
        {
            var leaseHaulerTruckRequest = await _leaseHaulerRequestRepository.GetAsync(input.LeaseHaulerRequestId);
            if (leaseHaulerTruckRequest == null)
            {
                throw new UserFriendlyException("Lease hauler truck(s) request cannot be found");
            }

            await CheckEntitySpecificPermissions(
                anyEntityPermissionName: null,
                specificEntityPermissionName: AppPermissions.LeaseHaulerPortal_Jobs_Accept,
                Session.LeaseHaulerId,
                leaseHaulerTruckRequest.LeaseHaulerId);

            if (leaseHaulerTruckRequest.OrderLineId.HasValue)
            {
                leaseHaulerTruckRequest.Status = LeaseHaulerRequestStatus.Approved;

                // assigned trucks/drivers to job
                var requestedLeaseHaulerTrucks = await (await _requestedLeaseHaulerTruckRepository.GetQueryAsync())
                    .Where(q => q.LeaseHaulerRequestId == input.LeaseHaulerRequestId)
                    .ToListAsync();
                foreach (var leaseHaulerTruck in requestedLeaseHaulerTrucks)
                {
                    await AddOrderLineTruck(new AddOrderLineTruckInput
                    {
                        OrderLineId = leaseHaulerTruckRequest.OrderLineId.Value,
                        TruckId = leaseHaulerTruck.TruckId,
                        DriverId = leaseHaulerTruck.DriverId,
                    });

                    // remove this lease hauler truck from requested trucks
                    await _requestedLeaseHaulerTruckRepository.DeleteAsync(leaseHaulerTruck);
                }
            }
        }

        [AbpAuthorize(AppPermissions.Pages_Schedule)]
        public async Task<PagedResultDto<SelectListDto>> GetOrderLinesToAssignTrucksToSelectList(GetSelectListIdInput input)
        {
            var separateItems = await FeatureChecker.IsEnabledAsync(AppFeatures.SeparateMaterialAndFreightItems);
            var validateUtilization = await SettingManager.GetSettingValueAsync<bool>(AppSettings.DispatchingAndMessaging.ValidateUtilization);

            if (await _listCaches.OrderLine.IsEnabled())
            {
                var key = await _listCaches.DateKeyLookup.GetKeyForOrderLine(input.Id);
                key.TenantId = await Session.GetTenantIdAsync();
                var cache = new
                {
                    Order = await _listCaches.Order.GetList(key),
                    OrderLine = await _listCaches.OrderLine.GetList(key),
                    OrderLineTruck = await _listCaches.OrderLineTruck.GetList(key),
                    Customer = await _listCaches.Customer.GetList(new ListCacheTenantKey(await Session.GetTenantIdAsync())),
                    Location = await _listCaches.Location.GetList(new ListCacheTenantKey(await Session.GetTenantIdAsync())),
                    Item = await _listCaches.Item.GetList(new ListCacheTenantKey(await Session.GetTenantIdAsync())),
                };

                var inputOrderLine = cache.OrderLine.Find(input.Id);
                var inputOrder = cache.Order.Find(inputOrderLine?.OrderId);

                if (inputOrderLine == null || inputOrder == null)
                {
                    throw await GetOrderLineNotFoundException(new EntityDto(input.Id));
                }

                return cache.OrderLine.Items
                    .Select(ol =>
                    {
                        var order = cache.Order.Find(ol.OrderId);
                        var orderLineTrucks = cache.OrderLineTruck.Items.Where(olt => olt.OrderLineId == ol.Id);

                        var result = new
                        {
                            ol.Id,
                            ol.Designation,
                            Order = order,
                            Customer = cache.Customer.Find(order?.CustomerId),
                            DeliverTo = cache.Location.Find(ol.DeliverToId),
                            MaterialItem = cache.Item.Find(ol.MaterialItemId),
                            FreightItem = cache.Item.Find(ol.FreightItemId),
                        };

                        if (order == null
                            || order.OfficeId != inputOrder.OfficeId
                            || order.DeliveryDate != inputOrder.DeliveryDate
                            || order.Shift != inputOrder.Shift
                            || ol.IsComplete
                            || ol.Id == input.Id
                            || validateUtilization
                                && (
                                    !ol.ScheduledTrucks.HasValue
                                    || (decimal)ol.ScheduledTrucks.Value <= (orderLineTrucks.Sum(olt => (decimal?)olt.Utilization) ?? 0)
                                )
                        )
                        {
                            return null;
                        }

                        return result;
                    })
                    .Where(x => x != null)
                    .Select(ol => new SelectListDto
                    {
                        Id = ol.Id.ToString(),
                        Name = ol.Customer?.Name + ", " + ol.DeliverTo?.DisplayName + ", " +
                            (separateItems
                                ? ol.Designation == DesignationEnum.MaterialOnly
                                    ? ol.MaterialItem?.Name
                                    : ol.FreightItem?.Name + ", " + ol.MaterialItem?.Name
                                : ol.FreightItem?.Name),
                    })
                    .ToList()
                    .GetSelectListResult(input);
            }
            else
            {
                var orderLine = await (await _orderLineRepository.GetQueryAsync())
                    .Where(ol => ol.Id == input.Id)
                    .Select(ol => new
                    {
                        ol.Order.OfficeId,
                        ol.Order.DeliveryDate,
                        ol.Order.Shift,
                    })
                    .FirstAsync();
                return await (await _orderLineRepository.GetQueryAsync())
                    .Where(ol =>
                        ol.Order.OfficeId == orderLine.OfficeId
                        && ol.Order.DeliveryDate == orderLine.DeliveryDate
                        && ol.Order.Shift == orderLine.Shift
                        && !ol.IsComplete
                        && ol.Id != input.Id
                        && (
                            !validateUtilization
                            || ol.ScheduledTrucks.HasValue
                            && (decimal)ol.ScheduledTrucks.Value > (ol.OrderLineTrucks.Sum(olt => (decimal?)olt.Utilization) ?? 0)
                        )
                    )
                    .Select(ol => new SelectListDto
                    {
                        Id = ol.Id.ToString(),
                        Name = ol.Order.Customer.Name + ", " + ol.DeliverTo.DisplayName + ", " +
                            (separateItems
                                ? ol.Designation == DesignationEnum.MaterialOnly
                                    ? ol.MaterialItem.Name
                                    : ol.FreightItem.Name + ", " + ol.MaterialItem.Name
                                : ol.FreightItem.Name),
                    })
                    .GetSelectListResult(input);
            }
        }

        [AbpAuthorize(AppPermissions.Pages_Schedule)]
        public async Task<IList<SelectListDto>> GetTrucksSelectList(int orderLineId)
        {
            return await (await _orderLineTruckRepository.GetQueryAsync())
                .Where(olt => olt.OrderLineId == orderLineId && olt.Truck.IsActive && !olt.Truck.IsOutOfService)
                .Select(olt => new SelectListDto
                {
                    Id = olt.TruckId.ToString(),
                    Name = olt.Truck.TruckCode,
                })
                .ToListAsync();
        }

        [AbpAuthorize(AppPermissions.Pages_Schedule)]
        public async Task<OrderLineTruckToChangeDriverDto> GetOrderLineTruckToChangeDriverModel(int orderLineTruckId)
        {
            if (orderLineTruckId == 0)
            {
                throw new UserFriendlyException(L("InvalidRequestMissingId"));
            }

            var orderLineTruck = await (await _orderLineTruckRepository.GetQueryAsync())
                .Where(olt => olt.Id == orderLineTruckId)
                .Select(olt => new
                {
                    TruckId = olt.TruckId,
                    OrderLineId = olt.OrderLineId,
                    DriverId = olt.DriverId,
                    DriverName = olt.Driver.FirstName + " " + olt.Driver.LastName,
                    LeaseHaulerId = (int?)olt.Truck.LeaseHaulerTruck.LeaseHaulerId,
                })
                .FirstOrDefaultAsync();

            var result = new OrderLineTruckToChangeDriverDto
            {
                HasTicketsOrLoads = await (await _ticketRepository.GetQueryAsync())
                    .AnyAsync(t => t.TruckId == orderLineTruck.TruckId && t.OrderLineId == orderLineTruck.OrderLineId),
                OrderLineTruckId = orderLineTruckId,
                DriverId = orderLineTruck.DriverId,
                DriverName = orderLineTruck.DriverName,
                IsExternal = orderLineTruck.LeaseHaulerId.HasValue,
                LeaseHaulerId = orderLineTruck.LeaseHaulerId,
            };

            return result;
        }

        [AbpAuthorize(AppPermissions.Pages_Schedule)]
        public async Task ReassignTrucks(ReassignTrucksInput input)
        {
            var orderDate = await (await _orderLineRepository.GetQueryAsync()).Where(ol => ol.Id == input.SourceOrderLineId).Select(ol => ol.Order.DeliveryDate).FirstAsync();
            var today = await GetToday();
            if (orderDate < today)
            {
                throw new UserFriendlyException("You cannot reassign trucks for past orders");
            }
            var sourceOrderLineTrucks = await (await _orderLineTruckRepository.GetQueryAsync())
                .Where(olt => olt.OrderLineId == input.SourceOrderLineId && input.TruckIds.Contains(olt.TruckId))
                .AsNoTracking()
                .ToListAsync();
            var destinationOrderLineTrucks = await (await _orderLineTruckRepository.GetQueryAsync())
                .Where(olt => olt.OrderLineId == input.DestinationOrderLineId)
                .Select(olt => new
                {
                    olt.TruckId,
                    olt.Utilization,
                    olt.IsDone,
                    olt.Id,
                })
                .ToListAsync();
            bool markAsDone = today == orderDate;
            foreach (var sourceOrderLineTruck in sourceOrderLineTrucks)
            {
                var destinationOrderLineTruck = destinationOrderLineTrucks.FirstOrDefault(x => sourceOrderLineTruck.TruckId == x.TruckId);
                if (destinationOrderLineTruck != null && destinationOrderLineTruck.IsDone)
                {
                    var utilization = sourceOrderLineTruck.Utilization;
                    await DeleteOrderLineTruck(sourceOrderLineTruck);
                    await ActivateClosedTruck(sourceOrderLineTruck);
                    await SetOrderLineTruckUtilization(destinationOrderLineTruck.Id, utilization);
                }
                else if (destinationOrderLineTruck == null)
                {
                    await DeleteOrderLineTruck(sourceOrderLineTruck);
                    await CreateOrderLineTruck(sourceOrderLineTruck);
                }
            }

            // Local functions
            async Task DeleteOrderLineTruck(OrderLineTruck sourceOrderLineTruck)
            {
                var deleteOrderLineTruckInput = new DeleteOrderLineTruckInput
                {
                    OrderLineTruckId = sourceOrderLineTruck.Id,
                    MarkAsDone = markAsDone,
                };
                await this.DeleteOrderLineTruck(deleteOrderLineTruckInput);
            }

            async Task CreateOrderLineTruck(OrderLineTruck sourceOrderLineTruck)
            {
                var addOrderLineTruckInput = new AddOrderLineTruckInternalInput
                {
                    OrderLineId = input.DestinationOrderLineId,
                    TruckId = sourceOrderLineTruck.TruckId,
                    TrailerId = sourceOrderLineTruck.TrailerId,
                    DriverId = sourceOrderLineTruck.DriverId,
                    ParentId = sourceOrderLineTruck.ParentOrderLineTruckId,
                    Utilization = !sourceOrderLineTruck.IsDone ? sourceOrderLineTruck.Utilization : 1,
                };
                var result = await AddOrderLineTruckInternal(addOrderLineTruckInput);
                if (result.IsFailed)
                {
                    throw new UserFriendlyException("There are too many trucks being moved to the new order line. Please increase the scheduled number of trucks or reduce the number of trucks being transferred.");
                }
            }

            async Task ActivateClosedTruck(OrderLineTruck sourceOrderLineTruck)
            {
                var activateClosedTrucksInput = new ActivateClosedTrucksInput
                {
                    OrderLineId = input.DestinationOrderLineId,
                    TruckIds = new[] { sourceOrderLineTruck.TruckId },
                };
                await ActivateClosedTrucks(activateClosedTrucksInput);
            }

            async Task SetOrderLineTruckUtilization(int orderLineTruckId, decimal utilization)
            {
                var orderLineTruckUtilizationEditDto = new OrderLineTruckUtilizationEditDto
                {
                    OrderLineId = input.DestinationOrderLineId,
                    OrderLineTruckId = orderLineTruckId,
                    Utilization = utilization,
                };
                await SetOrderTruckUtilization(orderLineTruckUtilizationEditDto);
            }
        }

        [AbpAuthorize(AppPermissions.Pages_Schedule)]
        public async Task AssignTrucks(AssignTrucksInput input)
        {
            var trucks = await (await _truckRepository.GetQueryAsync())
                .Where(x => input.TruckIds.Contains(x.Id))
                .Select(x => new
                {
                    x.Id,
                    x.CurrentTrailerId,
                    x.CanPullTrailer,
                }).ToListAsync();

            foreach (var truckId in input.TruckIds)
            {
                var addOrderTruckResult = await AddOrderLineTruckInternal(new AddOrderLineTruckInternalInput
                {
                    OrderLineId = input.OrderLineId,
                    TruckId = truckId,
                    Utilization = 1,
                });
                if (addOrderTruckResult.IsFailed)
                {
                    //throw new UserFriendlyException(addOrderTruckResult.ErrorMessage);
                    throw new UserFriendlyException("There are too many trucks being assigned to the new order line. Please increase the scheduled number of trucks or reduce the number of trucks being assigned.");
                }
            }
        }

        [AbpAuthorize(AppPermissions.Pages_Schedule)]
        public async Task<IList<SelectListDto>> GetClosedTrucksSelectList(int orderLineId)
        {
            return await (await _orderLineTruckRepository.GetQueryAsync())
                .Where(olt => olt.OrderLineId == orderLineId && olt.IsDone && !olt.Truck.IsOutOfService && olt.Truck.IsActive)
                .Select(olt => new SelectListDto
                {
                    Id = olt.TruckId.ToString(),
                    Name = olt.Truck.TruckCode,
                })
                .ToListAsync();
        }

        [AbpAuthorize(AppPermissions.Pages_Schedule)]
        public async Task<bool> ActivateClosedTrucks(ActivateClosedTrucksInput input)
        {
            var orderLineTrucksToActivate = await (await _orderLineTruckRepository.GetQueryAsync())
                .Where(olt => olt.OrderLineId == input.OrderLineId
                              && input.TruckIds.Contains(olt.TruckId)
                              && olt.Truck.IsActive
                              && !olt.Truck.IsOutOfService)
                .ToListAsync();

            var orderLineDetails = await (await _orderLineRepository.GetQueryAsync())
                .Where(x => x.Id == input.OrderLineId)
                .Select(x => new
                {
                    x.Order.DeliveryDate,
                    x.Order.Shift,
                    x.NumberOfTrucks,
                }).FirstAsync();

            var numberOfTrucks = Convert.ToDecimal(orderLineDetails.NumberOfTrucks ?? 0);
            var validateUtilization = await SettingManager.GetSettingValueAsync<bool>(AppSettings.DispatchingAndMessaging.ValidateUtilization);

            var truckUtilizationList = validateUtilization
                ? await (await _orderLineTruckRepository.GetQueryAsync())
                    .Where(olt =>
                        input.TruckIds.Contains(olt.TruckId)
                        && olt.OrderLine.Order.DeliveryDate == orderLineDetails.DeliveryDate
                        && olt.OrderLine.Order.Shift == orderLineDetails.Shift
                    )
                    .GroupBy(x => x.TruckId)
                    .Select(g => new
                    {
                        TruckId = g.Key,
                        Utilization = g.Sum(olt => olt.Utilization),
                    }).ToListAsync()
                : null;

            bool result = true;
            foreach (var orderLineTruck in orderLineTrucksToActivate)
            {
                decimal utilizationToAssign;

                if (!validateUtilization)
                {
                    utilizationToAssign = numberOfTrucks <= 0
                        ? 1
                        : Math.Min(numberOfTrucks, 1);
                }
                else
                {
                    var truckUtilization = truckUtilizationList.FirstOrDefault(x => x.TruckId == orderLineTruck.TruckId);

                    utilizationToAssign = 1 - (truckUtilization?.Utilization ?? 0);
                    if (utilizationToAssign == 0)
                    {
                        result = false;
                        continue;
                    }
                }

                orderLineTruck.IsDone = false;
                orderLineTruck.Utilization = utilizationToAssign;
            }

            return result;
        }


        [AbpAuthorize(AppPermissions.Pages_Schedule)]
        public async Task<SetOrderOfficeIdInput> GetOrderOfficeIdForEdit(EntityDto input)
        {
            var order = await (await _orderRepository.GetQueryAsync())
                .Where(o => o.OrderLines.Any(ol => ol.Id == input.Id))
                .Select(o => new SetOrderOfficeIdInput
                {
                    OrderId = o.Id,
                    OfficeId = o.OfficeId,
                    OfficeName = o.Office.Name,
                    OrderLineId = o.OrderLines.Count == 1 ? (int?)null : input.Id,
                })
                .FirstOrDefaultAsync();

            if (order == null)
            {
                throw await GetOrderNotFoundException(input);
            }

            return order;
        }

        [AbpAuthorize(AppPermissions.Pages_Schedule)]
        public async Task<SetOrderDirectionsInput> GetOrderDirectionsForEdit(EntityDto input)
        {
            var order = await (await _orderRepository.GetQueryAsync())
                .Where(o => o.OrderLines.Any(ol => ol.Id == input.Id))
                .Select(o => new SetOrderDirectionsInput
                {
                    OrderId = o.Id,
                    Directions = o.Directions,
                })
                .FirstOrDefaultAsync();

            if (order == null)
            {
                throw await GetOrderNotFoundException(input);
            }

            return order;
        }

        [AbpAuthorize(AppPermissions.Pages_Schedule)]
        public async Task<SetOrderLineNoteInput> GetOrderLineNoteForEdit(NullableIdDto input)
        {
            if (!input.Id.HasValue)
            {
                return new SetOrderLineNoteInput();
            }

            var orderLine = await (await _orderLineRepository.GetQueryAsync())
                .Where(ol => ol.Id == input.Id)
                .Select(ol => new SetOrderLineNoteInput
                {
                    OrderLineId = ol.Id,
                    Note = ol.Note,
                })
                .FirstOrDefaultAsync();

            if (orderLine == null)
            {
                throw await GetOrderLineNotFoundException(new EntityDto(input.Id.Value));
            }

            return orderLine;
        }

        [AbpAuthorize(AppPermissions.Pages_Schedule)]
        public async Task<SetOrderDateInput> GetSetOrderDateInput(int orderLineId)
        {
            var setOrderDateInput = await (await _orderRepository.GetQueryAsync())
                .Where(o => o.OrderLines.Any(ol => ol.Id == orderLineId))
                .Select(o => new SetOrderDateInput
                {
                    OrderId = o.Id,
                    Date = o.DeliveryDate,
                    OrderLineId = o.OrderLines.Count == 1 ? (int?)null : orderLineId,
                })
                .FirstOrDefaultAsync();

            return setOrderDateInput;
        }

        [AbpAuthorize(AppPermissions.Pages_Schedule)]
        public async Task<OrderLineTruckDetailsDto> GetOrderTruckUtilizationForEdit(EntityDto input)
        {
            if (input.Id == 0)
            {
                throw new UserFriendlyException(L("InvalidRequestMissingId"));
            }

            var orderLineTruck = await (await _orderLineTruckRepository.GetQueryAsync())
                .Where(olt => olt.Id == input.Id)
                .Select(olt => new
                {
                    olt.Utilization,
                    olt.OrderLineId,
                    olt.Truck.VehicleCategory.IsPowered,
                    olt.Truck.VehicleCategory.AssetType,
                    olt.TruckId,
                    olt.Truck.TruckCode,
                    TimeOnJobUtc = olt.TimeOnJob,
                })
                .FirstOrDefaultAsync();

            if (orderLineTruck == null)
            {
                throw await GetOrderLineTruckNotFoundException(input);
            }

            var orderLine = await (await _orderLineRepository.GetQueryAsync()) //todo use cache?
                .Where(ol => ol.Id == orderLineTruck.OrderLineId)
                .Select(x => new
                {
                    Utilization = x.OrderLineTrucks.Where(t => t.Truck.VehicleCategory.IsPowered).Select(t => t.Utilization).Sum(),
                    x.ScheduledTrucks,
                    x.NumberOfTrucks,
                    x.Order.DeliveryDate,
                    x.Order.Shift,
                })
                .FirstOrDefaultAsync();

            if (orderLine == null)
            {
                throw await GetOrderLineNotFoundException(new EntityDto(orderLineTruck.OrderLineId));
            }

            var orderLineMaxUtilization = orderLine.ScheduledTrucks.HasValue ? Convert.ToDecimal(orderLine.ScheduledTrucks.Value) : 0;
            var orderLineRequestedNumberOfTrucks = orderLine.NumberOfTrucks.HasValue ? Convert.ToDecimal(orderLine.NumberOfTrucks.Value) : 0;

            var currentTruckUtilization = orderLineTruck.Utilization;
            var truckUtilization = await (await _orderLineTruckRepository.GetQueryAsync())
                .Where(olt => olt.TruckId == orderLineTruck.TruckId && olt.OrderLine.Order.DeliveryDate == orderLine.DeliveryDate && olt.OrderLine.Order.Shift == orderLine.Shift)
                .SumAsync(olt => olt.Utilization);
            var remainingTruckUtilization = await GetRemainingTruckUtilizationForOrderAsync(new GetRemainingTruckUtilizationForOrderInput
            {
                OrderMaxUtilization = orderLineMaxUtilization,
                OrderRequestedNumberOfTrucks = orderLineRequestedNumberOfTrucks,
                OrderUtilization = orderLine.Utilization,
                AssetType = orderLineTruck.AssetType,
                IsPowered = orderLineTruck.IsPowered,
                TruckUtilization = truckUtilization,
            });
            var maxUtilization = await SettingManager.GetSettingValueAsync<bool>(AppSettings.DispatchingAndMessaging.ValidateUtilization)
                ? Math.Min(1, currentTruckUtilization + remainingTruckUtilization)
                : 1;
            return new OrderLineTruckDetailsDto
            {
                OrderLineTruckId = input.Id,
                OrderLineId = orderLineTruck.OrderLineId,
                Utilization = orderLineTruck.Utilization,
                MaxUtilization = maxUtilization,
                TruckCode = orderLineTruck.TruckCode,
                TimeOnJob = orderLineTruck.TimeOnJobUtc?.ConvertTimeZoneTo(await GetTimezone()),
            };
        }

        private async Task<Exception> GetOrderLineTruckNotFoundException(EntityDto input)
        {
            var deletedOrderLineTruck = await _orderLineTruckRepository.GetDeletedEntity(input, CurrentUnitOfWork);
            if (deletedOrderLineTruck == null)
            {
                return new Exception($"OrderLineTruck with id {input.Id} wasn't found and is not deleted");
            }

            var orderLine = await _orderLineRepository.GetMaybeDeletedEntity(new EntityDto(deletedOrderLineTruck.OrderLineId), CurrentUnitOfWork);
            if (orderLine == null)
            {
                return new Exception($"OrderLineTruck with id {input.Id} wasn't found and is not deleted");
            }

            if (await _orderRepository.IsEntityDeleted(new EntityDto(orderLine.OrderId), CurrentUnitOfWork))
            {
                return new EntityDeletedException("Order", "This order has been deleted and can’t be edited");
            }

            if (orderLine.IsDeleted)
            {
                return new EntityDeletedException("OrderLine", "This order item has been deleted and can’t be edited");
            }

            return new EntityDeletedException("OrderLineTruck", "This truck assignment has been deleted and can’t be edited");
        }

        private async Task<Exception> GetOrderNotFoundException(EntityDto input)
        {
            if (await _orderRepository.IsEntityDeleted(input, CurrentUnitOfWork))
            {
                return new EntityDeletedException("Order", "This order has been deleted and can’t be edited");
            }

            return new Exception($"Order with id {input.Id} wasn't found and is not deleted");
        }

        private async Task<Exception> GetOrderLineNotFoundException(EntityDto input)
        {
            if (await _orderLineRepository.IsEntityDeleted(input, CurrentUnitOfWork))
            {
                return new EntityDeletedException("Order Line", "This order line has been deleted and can’t be edited");
            }

            return new Exception($"Order Line with id {input.Id} wasn't found and is not deleted");
        }

        [AbpAuthorize(AppPermissions.Pages_Schedule, AppPermissions.Pages_Orders_Edit, RequireAllPermissions = true)]
        public async Task SetOrderLineTruckDetails(OrderLineTruckDetailsDto input)
        {
            var orderLineTruck = await _orderLineTruckRepository.GetAsync(input.OrderLineTruckId);

            var orderDetails = await (await _orderLineRepository.GetQueryAsync())
                .Where(x => x.Id == orderLineTruck.OrderLineId)
                .Select(x => new
                {
                    x.Order.DeliveryDate,
                }).FirstAsync();

            var date = orderDetails.DeliveryDate;

            var newValue = date.AddTimeOrNull(input.TimeOnJob)?.ConvertTimeZoneFrom(await GetTimezone());
            if (newValue != orderLineTruck.TimeOnJob)
            {
                orderLineTruck.TimeOnJob = newValue;

                var orderLineUpdater = _orderLineUpdaterFactory.Create(orderLineTruck.OrderLineId);
                var orderLine = await orderLineUpdater.GetEntityAsync();
                if (orderLine.StaggeredTimeKind != StaggeredTimeKind.None)
                {
                    orderLineUpdater.UpdateOrderLineTrucksTimeOnJobIfNeeded(false);
                    if (orderLine.TimeOnJob == null && orderLine.FirstStaggeredTimeOnJob != null)
                    {
                        await orderLineUpdater.UpdateFieldAsync(x => x.TimeOnJob, orderLine.FirstStaggeredTimeOnJob);
                    }
                    await orderLineUpdater.UpdateFieldAsync(x => x.StaggeredTimeKind, StaggeredTimeKind.None);
                    await orderLineUpdater.UpdateFieldAsync(x => x.FirstStaggeredTimeOnJob, null);
                    await orderLineUpdater.UpdateFieldAsync(x => x.StaggeredTimeInterval, null);
                    await orderLineUpdater.SaveChangesAsync();
                }
            }

            if (input.UpdateDispatchesTimeOnJob == true)
            {
                var dispatches = await (await _dispatchRepository.GetQueryAsync())
                    .Where(x => x.OrderLineTruckId == input.OrderLineTruckId
                                && Dispatch.OpenStatuses.Contains(x.Status)
                                && x.TimeOnJob != newValue)
                    .ToListAsync();

                if (dispatches.Any())
                {
                    dispatches.ForEach(x => x.TimeOnJob = newValue);

                    await CurrentUnitOfWork.SaveChangesAsync();

                    await _syncRequestSender.SendSyncRequest(new SyncRequest()
                        .AddChanges(EntityEnum.Dispatch, dispatches.Select(x => x.ToChangedEntity()))
                        .AddLogMessage("Updated OrderLineTruck has affected dispatch(es)"));
                }
            }
        }

        [AbpAuthorize(AppPermissions.Pages_Schedule)]
        public async Task SetOrderTruckUtilization(OrderLineTruckUtilizationEditDto input)
        {
            var originalEditDto = await GetOrderTruckUtilizationForEdit(new EntityDto(input.OrderLineTruckId));
            var orderLineTruck = await _orderLineTruckRepository.GetAsync(input.OrderLineTruckId);

            if (input.Utilization <= 0)
            {
                await DeleteOrderLineTruck(new DeleteOrderLineTruckInput
                {
                    OrderLineTruckId = input.OrderLineTruckId,
                });
            }
            else
            {
                if (input.Utilization > originalEditDto.MaxUtilization)
                {
                    input.Utilization = originalEditDto.MaxUtilization;
                }
                orderLineTruck.Utilization = input.Utilization;
            }

        }

        [AbpAuthorize(AppPermissions.Pages_Schedule)]
        public async Task ChangeOrderLineUtilization(ChangeOrderLineUtilizationInput input)
        {
            var orderLineTrucks = await (await _orderLineTruckRepository.GetQueryAsync())
                .Include(t => t.Truck)
                    .ThenInclude(t => t.VehicleCategory)
                .Where(t => t.OrderLineId == input.OrderLineId && !t.IsDone && t.Truck.VehicleCategory.IsPowered)
                .ToListAsync();

            if (!orderLineTrucks.Any())
            {
                return;
            }

            var orderLine = await (await _orderLineRepository.GetQueryAsync())
                .Where(ol => ol.Id == input.OrderLineId)
                .Select(x => new
                {
                    x.ScheduledTrucks,
                    x.NumberOfTrucks,
                    x.Order.DeliveryDate,
                    x.Order.Shift,
                })
                .FirstOrDefaultAsync();

            if (orderLine == null)
            {
                throw await GetOrderLineNotFoundException(new EntityDto(input.OrderLineId));
            }

            if (input.Utilization <= 0)
            {
                var existingDispatches = await OrderLineHasDispatches(new OrderLineHasDispatchesInput { OrderLineId = input.OrderLineId });

                var hasAcknowledgedOrLoadedDispatches = existingDispatches.FirstOrDefault(t => t.AcknowledgedOrLoaded);
                if (hasAcknowledgedOrLoadedDispatches != null)
                {
                    throw new UserFriendlyException(L("TruckHasDispatch_YouMustCancelItFirstToRemoveTruck", hasAcknowledgedOrLoadedDispatches.TruckCode));
                }

                await _dispatchingAppService.CancelOrEndAllDispatches(new CancelOrEndAllDispatchesInput
                {
                    OrderLineId = input.OrderLineId,
                });

                await _orderLineTruckRepository.DeleteAsync(x => x.OrderLineId == input.OrderLineId);

                return;
            }

            if (!await SettingManager.GetSettingValueAsync<bool>(AppSettings.DispatchingAndMessaging.ValidateUtilization))
            {
                foreach (var orderLineTruck in orderLineTrucks)
                {
                    orderLineTruck.Utilization = input.Utilization;
                }
                return;
            }

            var orderLineMaxUtilization = orderLine.ScheduledTrucks.HasValue ? Convert.ToDecimal(orderLine.ScheduledTrucks.Value) : 0;
            var orderLineRequestedNumberOfTrucks = orderLine.NumberOfTrucks.HasValue ? Convert.ToDecimal(orderLine.NumberOfTrucks.Value) : 0;
            var orderLineUtilization = 0M;

            foreach (var orderLineTruck in orderLineTrucks)
            {
                var currentTruckUtilization = orderLineTruck.Utilization;
                var truckUtilization = await (await _orderLineTruckRepository.GetQueryAsync())
                    .Where(olt => olt.TruckId == orderLineTruck.TruckId && olt.OrderLine.Order.DeliveryDate == orderLine.DeliveryDate && olt.OrderLine.Order.Shift == orderLine.Shift)
                    .SumAsync(olt => olt.Utilization);
                var remainingTruckUtilization = await GetRemainingTruckUtilizationForOrderAsync(new GetRemainingTruckUtilizationForOrderInput
                {
                    OrderMaxUtilization = orderLineMaxUtilization,
                    OrderRequestedNumberOfTrucks = orderLineRequestedNumberOfTrucks,
                    OrderUtilization = orderLineUtilization,
                    AssetType = orderLineTruck.Truck.VehicleCategory.AssetType,
                    IsPowered = orderLineTruck.Truck.VehicleCategory.IsPowered,
                    TruckUtilization = truckUtilization,
                });
                var maxTruckUtilization = Math.Min(1, currentTruckUtilization + remainingTruckUtilization);

                if (input.Utilization > maxTruckUtilization)
                {
                    throw new UserFriendlyException(L("TruckCantHaveUtilizationHigherThan", orderLineTruck.Truck.TruckCode, Math.Round(maxTruckUtilization, 2)));
                }

                orderLineUtilization += input.Utilization;
                if (orderLineUtilization > orderLineMaxUtilization)
                {
                    throw new UserFriendlyException(L("UtilizationWouldExceedNumberOfTrucks"));
                }

                orderLineTruck.Utilization = input.Utilization;
            }
        }

        private async Task<decimal> GetRemainingTruckUtilizationForOrderLineAsync(ScheduleOrderLineDto orderLine, ScheduleTruckDto truck)
        {
            return await GetRemainingTruckUtilizationForOrderAsync(GetRemainingTruckUtilizationForOrderInput.From(orderLine, truck));
        }

        private async Task<decimal> GetRemainingTruckUtilizationForOrderAsync(GetRemainingTruckUtilizationForOrderInput input)
        {
            if (!await SettingManager.GetSettingValueAsync<bool>(AppSettings.DispatchingAndMessaging.ValidateUtilization))
            {
                if (input.OrderRequestedNumberOfTrucks <= 0)
                {
                    return 1;
                }
                return Math.Min(input.OrderRequestedNumberOfTrucks, 1);
            }

            if (input.OrderMaxUtilization == 0)
            {
                return 0;
            }

            if (!input.IsPowered)
            {
                //previous trailer utilization logic
                //return input.TruckUtilization > 0 ? 0 : 1;
                return 1;
            }

            var remainToUtilize = input.OrderMaxUtilization - input.OrderUtilization;
            if (remainToUtilize <= 0 || input.TruckUtilization >= 1)
            {
                return 0;
            }

            return Math.Min((1 - input.TruckUtilization), remainToUtilize);
        }

        [AbpAuthorize(AppPermissions.Pages_Schedule)]
        public async Task<string> GetDeviceIdsStringForOrderLineTrucks(int orderLineId)
        {
            var truckCodes = await (await _orderLineTruckRepository.GetQueryAsync())
                .Where(olt => olt.OrderLineId == orderLineId)
                .Select(olt => olt.Truck.TruckCode)
                .Distinct()
                .ToArrayAsync();
            return (await _geotabTelematics.GetDeviceIdsByTruckCodesAsync(truckCodes)).JoinAsString(",");
        }

        [AbpAuthorize(AppPermissions.Pages_Schedule, AppPermissions.LeaseHaulerPortal_Schedule)]
        public async Task<List<LeaseHaulerSelectionTruckDto>> GetLeaseHaulerTrucks(IdListInput input)
        {
            var leaseHaulerFilter = await GetLeaseHaulerIdFilterAsync(AppPermissions.Pages_Schedule, AppPermissions.LeaseHaulerPortal_Schedule);

            return await (await _truckRepository.GetQueryAsync())
                .WhereIf(!leaseHaulerFilter.HasValue, x => x.IsActive && x.LeaseHaulerTruck.AlwaysShowOnSchedule != true && input.Ids.Contains(x.LeaseHaulerTruck.LeaseHaulerId))
                .WhereIf(leaseHaulerFilter.HasValue, x => x.LeaseHaulerTruck.LeaseHaulerId == leaseHaulerFilter.Value)
                .Select(x => new LeaseHaulerSelectionTruckDto
                {
                    LeaseHaulerId = x.LeaseHaulerTruck.LeaseHaulerId,
                    TruckId = x.Id,
                    TruckCode = x.TruckCode,
                    DefaultDriverId = x.DefaultDriverId == null ? 0 : x.DefaultDriverId.Value,
                })
                .OrderBy(x => x.LeaseHaulerId)
                .ThenBy(x => x.TruckCode)
                .ToListAsync();
        }

        [AbpAuthorize(AppPermissions.Pages_Schedule, AppPermissions.LeaseHaulerPortal_Schedule)]
        public async Task<List<LeaseHaulerSelectionDriverDto>> GetLeaseHaulerDrivers(IdListInput input)
        {
            var leaseHaulerFilter = await GetLeaseHaulerIdFilterAsync(AppPermissions.Pages_Schedule, AppPermissions.LeaseHaulerPortal_Schedule);

            return await (await _driverRepository.GetQueryAsync())
                .WhereIf(!leaseHaulerFilter.HasValue, x => input.Ids.Contains(x.LeaseHaulerDriver.LeaseHaulerId) && !x.IsInactive)
                .WhereIf(leaseHaulerFilter.HasValue, x => x.LeaseHaulerDriver.LeaseHaulerId == leaseHaulerFilter.Value)
                .Select(x => new LeaseHaulerSelectionDriverDto
                {
                    LeaseHaulerId = x.LeaseHaulerDriver.LeaseHaulerId,
                    DriverId = x.Id,
                    DriverName = x.FirstName + " " + x.LastName,
                })
                .OrderBy(x => x.LeaseHaulerId)
                .ThenBy(x => x.DriverName)
                .ToListAsync();
        }

        [AbpAuthorize(AppPermissions.Pages_Schedule)]
        public async Task SetAllOrderLinesIsComplete(GetScheduleOrdersInput input)
        {
            input.HideCompletedOrders = true;
            var query = await GetScheduleQueryAsync(input);

            var items = await query
                .Select(x => new
                {
                    x.Id,
                })
                .ToListAsync();

            foreach (var item in items)
            {
                await SetOrderLineIsComplete(new SetOrderLineIsCompleteInput
                {
                    OrderLineId = item.Id,
                    IsComplete = true,
                    IsCancelled = false,
                });
            }
        }

        [AbpAuthorize(AppPermissions.Pages_Schedule)]
        public async Task<SendOrderLineToHaulingCompanyInput> GetInputForSendOrderLineToHaulingCompany(int orderLineId)
        {
            return await _crossTenantOrderSender.GetInputForSendOrderLineToHaulingCompany(orderLineId);
        }

        [AbpAuthorize(AppPermissions.Pages_Schedule)]
        public async Task SendOrderLineToHaulingCompany(SendOrderLineToHaulingCompanyInput input)
        {
            await _crossTenantOrderSender.SendOrderLineToHaulingCompany(input);
        }

        [AbpAuthorize(AppPermissions.Pages_Schedule)]
        public async Task DeleteRequestedLeaseHaulerTruck(int id)
        {
            await _requestedLeaseHaulerTruckRepository.DeleteAsync(q => q.Id == id);
        }
    }
}
