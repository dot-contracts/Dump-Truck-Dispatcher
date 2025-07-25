using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Threading.Tasks;
using Abp.Application.Services;
using Abp.Application.Services.Dto;
using Abp.Authorization;
using Abp.Configuration;
using Abp.Domain.Repositories;
using Abp.Extensions;
using Abp.Linq.Extensions;
using Abp.Runtime.Session;
using Abp.Timing;
using Abp.UI;
using DispatcherWeb.Authorization;
using DispatcherWeb.Caching;
using DispatcherWeb.Configuration;
using DispatcherWeb.Dispatching;
using DispatcherWeb.Drivers;
using DispatcherWeb.Dto;
using DispatcherWeb.Features;
using DispatcherWeb.Infrastructure.AzureBlobs;
using DispatcherWeb.Infrastructure.Extensions;
using DispatcherWeb.LeaseHaulers;
using DispatcherWeb.Offices;
using DispatcherWeb.Orders;
using DispatcherWeb.Sessions;
using DispatcherWeb.SyncRequests;
using DispatcherWeb.TimeOffs;
using DispatcherWeb.Trucks.Dto;
using DispatcherWeb.Trucks.Exporting;
using DispatcherWeb.VehicleCategories;
using DispatcherWeb.VehicleCategories.Dto;
using DispatcherWeb.VehicleMaintenance;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using VehicleCategoryDto = DispatcherWeb.Trucks.Dto.VehicleCategoryDto;

namespace DispatcherWeb.Trucks
{
    [AbpAuthorize]
    public class TruckAppService : DispatcherWebAppServiceBase, ITruckAppService
    {
        private readonly IRepository<Truck> _truckRepository;
        private readonly ITruckCache _truckCache;
        private readonly IRepository<VehicleCategory> _vehicleCategoryRepository;
        private readonly IRepository<TruckFile> _truckFileRepository;
        private readonly IRepository<OrderLineTruck> _orderLineTruckRepository;
        private readonly IRepository<OutOfServiceHistory> _outOfServiceHistoryRepository;
        private readonly IRepository<DriverAssignment> _driverAssignmentRepository;
        private readonly IRepository<Dispatch> _dispatchRepository;
        private readonly IRepository<Ticket> _ticketRepository;
        private readonly IRepository<TimeOff> _timeOffRepository;
        private readonly IRepository<MobileDevice> _mobileDeviceRepository;
        private readonly IRepository<LeaseHaulerTruck> _leaseHaulerTruckRepository;
        private readonly IOrderLineUpdaterFactory _orderLineUpdaterFactory;
        private readonly ListCacheCollection _listCaches;
        private readonly ISingleOfficeAppService _singleOfficeService;
        private readonly ITruckListCsvExporter _truckListCsvExporter;
        private readonly IAttachmentHelper _attachmentHelper;
        private readonly ISyncRequestSender _syncRequestSender;
        private readonly ICrossTenantOrderSender _crossTenantOrderSender;
        private readonly ITruckCommonDomainService _truckCommonDomainService;
        private readonly ITruckTelematicsAppService _truckTelematicsAppService;

        public TruckAppService(
            IRepository<Truck> truckRepository,
            ITruckCache truckCache,
            IRepository<VehicleCategory> vehicleCategoryRepository,
            IRepository<TruckFile> truckFileRepository,
            IRepository<OrderLineTruck> orderLineTruckRepository,
            IRepository<OutOfServiceHistory> outOfServiceHistoryRepository,
            IRepository<DriverAssignment> driverAssignmentRepository,
            IRepository<Dispatch> dispatchRepository,
            IRepository<Ticket> ticketRepository,
            IRepository<TimeOff> timeOffRepository,
            IRepository<MobileDevice> mobileDeviceRepository,
            IRepository<LeaseHaulerTruck> leaseHaulerTruckRepository,
            IOrderLineUpdaterFactory orderLineUpdaterFactory,
            ListCacheCollection listCaches,
            ISingleOfficeAppService singleOfficeService,
            ITruckListCsvExporter truckListCsvExporter,
            IAttachmentHelper attachmentHelper,
            ISyncRequestSender syncRequestSender,
            ICrossTenantOrderSender crossTenantOrderSender,
            ITruckCommonDomainService truckCommonDomainService,
            ITruckTelematicsAppService truckTelematicsAppService
        )
        {
            _truckRepository = truckRepository;
            _truckCache = truckCache;
            _vehicleCategoryRepository = vehicleCategoryRepository;
            _truckFileRepository = truckFileRepository;
            _orderLineTruckRepository = orderLineTruckRepository;
            _outOfServiceHistoryRepository = outOfServiceHistoryRepository;
            _driverAssignmentRepository = driverAssignmentRepository;
            _dispatchRepository = dispatchRepository;
            _ticketRepository = ticketRepository;
            _timeOffRepository = timeOffRepository;
            _mobileDeviceRepository = mobileDeviceRepository;
            _leaseHaulerTruckRepository = leaseHaulerTruckRepository;
            _orderLineUpdaterFactory = orderLineUpdaterFactory;
            _listCaches = listCaches;
            _singleOfficeService = singleOfficeService;
            _truckListCsvExporter = truckListCsvExporter;
            _attachmentHelper = attachmentHelper;
            _syncRequestSender = syncRequestSender;
            _crossTenantOrderSender = crossTenantOrderSender;
            _truckCommonDomainService = truckCommonDomainService;
            _truckTelematicsAppService = truckTelematicsAppService;
        }

        [AbpAuthorize(AppPermissions.Pages_Trucks)]
        public async Task<PagedResultDto<TruckDto>> GetTrucks(GetTrucksInput input)
        {
            var today = await GetToday();
            var query = await GetFilteredTruckQueryAsync(input, today);

            var totalCount = await query.CountAsync();

            var items = await query
                .Select(x => new TruckDto
                {
                    Id = x.Id,
                    TruckCode = x.TruckCode,
                    OfficeName = x.Office.Name,
                    VehicleCategoryName = x.VehicleCategory.Name,
                    DefaultDriverName = x.DefaultDriver != null ? x.DefaultDriver.LastName + ", " + x.DefaultDriver.FirstName : "",
                    YearMakeModel = x.Year + " " + x.Make + " " + x.Model,
                    IsActive = x.IsActive,
                    IsOutOfService = x.IsOutOfService,
                    CurrentMileage = x.CurrentMileage,
                    DueDateStatus = x.PreventiveMaintenances
                            .OrderBy(pm => pm.DueDate)
                            .Select(pm => pm.DueDate < today ? true : (pm.WarningDate < today ? false : (bool?)null))
                            .FirstOrDefault(),
                    DueMileageStatus = x.PreventiveMaintenances
                            .OrderBy(pm => pm.DueMileage)
                            .Select(pm => pm.DueMileage < x.CurrentMileage ? true : (pm.WarningMileage < x.CurrentMileage ? false : (bool?)null))
                            .FirstOrDefault(),
                    SoldDate = x.SoldDate,
                })
                .OrderBy(input.Sorting)
                .PageBy(input)
                .ToListAsync();

            return new PagedResultDto<TruckDto>(
                totalCount,
                items);
        }

        private async Task<IQueryable<Truck>> GetFilteredTruckQueryAsync(IGetTruckListFilter input, DateTime today)
        {
            return (await _truckRepository.GetQueryAsync())
                .Where(x => x.OfficeId.HasValue && x.LeaseHaulerTruck.AlwaysShowOnSchedule != true)
                .WhereIf(!input.TruckCode.IsNullOrEmpty(), x => x.TruckCode.StartsWith(input.TruckCode))
                .WhereIf(input.OfficeId.HasValue, x => x.OfficeId == input.OfficeId)
                .WhereIf(input.VehicleCategoryId > 0, x => x.VehicleCategoryId == input.VehicleCategoryId)
                .WhereIf(input.Status == FilterActiveStatus.Active, x => x.IsActive)
                .WhereIf(input.Status == FilterActiveStatus.Inactive, x => !x.IsActive)
                .WhereIf(input.IsOutOfService.HasValue, x => x.IsOutOfService == input.IsOutOfService)
                .WhereIf(input.PlatesExpiringThisMonth, t =>
                    t.PlateExpiration.HasValue
                    && t.PlateExpiration.Value.Year == today.Year
                    && t.PlateExpiration.Value.Month == today.Month
                )
                .WhereIf(!input.IncludeSold, x => !x.SoldDate.HasValue);
        }

        [AbpAuthorize(AppPermissions.Pages_Trucks)]
        [HttpPost]
        public async Task<FileDto> GetTrucksToCsv(GetTrucksInput input)
        {
            var today = await GetToday();
            var query = await GetFilteredTruckQueryAsync(input, today);
            var items = await GetTruckEditDtoQuery(query)
                .OrderBy(input.Sorting)
                .ToListAsync();

            if (!items.Any())
            {
                throw new UserFriendlyException(L("ThereIsNoDataToExport"));
            }

            return await _truckListCsvExporter.ExportToFileAsync(items);

        }

        private static IQueryable<TruckEditDto> GetTruckEditDtoQuery(IQueryable<Truck> query)
        {
            return query.Select(x => new TruckEditDto
            {
                Id = x.Id,
                TruckCode = x.TruckCode,
                OfficeName = x.Office.Name,
                VehicleCategoryId = x.VehicleCategoryId,
                VehicleCategoryName = x.VehicleCategory.Name,
                DefaultDriverName = x.DefaultDriver != null ? x.DefaultDriver.LastName + ", " + x.DefaultDriver.FirstName : "",
                CurrentTrailerCode = x.CurrentTrailer.TruckCode,
                IsActive = x.IsActive,
                InactivationDate = x.InactivationDate,
                IsOutOfService = x.IsOutOfService,
                Reason = x.IsOutOfService ? x.OutOfServiceHistories.OrderByDescending(oosh => oosh.OutOfServiceDate).Select(oosh => oosh.Reason).FirstOrDefault() : "",
                Year = x.Year,
                Make = x.Make,
                Model = x.Model,
                InServiceDate = x.InServiceDate,
                Vin = x.Vin,
                Plate = x.Plate,
                PlateExpiration = x.PlateExpiration,
                CargoCapacity = x.CargoCapacity,
                CargoCapacityCyds = x.CargoCapacityCyds,
                InsurancePolicyNumber = x.InsurancePolicyNumber,
                InsuranceValidUntil = x.InsuranceValidUntil,
                PurchaseDate = x.PurchaseDate,
                PurchasePrice = x.PurchasePrice,
                SoldDate = x.SoldDate,
                SoldPrice = x.SoldPrice,
                TruxTruckId = x.TruxTruckId,
                BedConstruction = x.BedConstruction,
                CanPullTrailer = x.CanPullTrailer,
                CurrentHours = x.CurrentHours,
                CurrentMileage = x.CurrentMileage,
                DefaultDriverId = x.DefaultDriverId,
                CurrentTrailerId = x.CurrentTrailerId,
                DtdTrackerDeviceTypeId = x.DtdTrackerDeviceTypeId,
                DtdTrackerDeviceTypeName = x.DtdTrackerDeviceTypeName,
                DtdTrackerPassword = x.DtdTrackerPassword,
                DtdTrackerServerAddress = x.DtdTrackerServerAddress,
                DtdTrackerUniqueId = x.DtdTrackerUniqueId,
                EnableDriverAppGps = x.EnableDriverAppGps,
                IsApportioned = x.IsApportioned,
                AlwaysShowOnSchedule = x.AlwaysShowOnSchedule,
                OfficeId = x.OfficeId == null ? 0 : x.OfficeId.Value,
                VehicleCategoryAssetType = x.VehicleCategory.AssetType,
                VehicleCategoryIsPowered = x.VehicleCategory.IsPowered,
                FuelType = x.FuelType,
                FuelCapacity = x.FuelCapacity,
                SteerTires = x.SteerTires,
                DriveAxleTires = x.DriveAxleTires,
                DropAxleTires = x.DropAxleTires,
                TrailerTires = x.TrailerTires,
                Transmission = x.Transmission,
                Engine = x.Engine,
                RearEnd = x.RearEnd,
                Notes = x.Notes,
                MobileDevices = x.MobileDevices.Select(d => new MobileDeviceEditDto
                {
                    Id = d.Id,
                    DeviceType = d.DeviceType,
                    Make = d.Make,
                    Model = d.Model,
                    Imei = d.Imei,
                    SimId = d.SimId,
                }).ToList(),
            });
        }

        [AbpAuthorize(AppPermissions.Pages_Misc_SelectLists_Trucks, AppPermissions.LeaseHaulerPortal_SelectLists_Trucks)]
        public async Task<PagedResultDto<SelectListDto>> GetTrucksSelectList(GetTrucksSelectListInput input)
        {
            var query = await _truckRepository.GetQueryAsync();

            var leaseHaulerIdFilter = await GetLeaseHaulerIdFilterAsync(AppPermissions.Pages_Misc_SelectLists_Trucks, AppPermissions.LeaseHaulerPortal_SelectLists_Trucks);

            if (input.OrderLineId.HasValue)
            {
                var orderLineTruckQuery = (await _orderLineTruckRepository.GetQueryAsync())
                    .Where(x => x.OrderLineId == input.OrderLineId);

                query = orderLineTruckQuery.Select(x => x.Truck)
                    .Union(
                        orderLineTruckQuery.Select(x => x.Trailer)
                    )
                    .Distinct();
            }

            var officeIds = await GetOfficeIds();

            query = query
                .WhereIf(leaseHaulerIdFilter.HasValue, x => x.LeaseHaulerTruck.LeaseHaulerId == leaseHaulerIdFilter)
                .WhereIf(!input.AllOffices && input.OfficeId == null && officeIds.Any(),
                    x => officeIds.Contains(x.OfficeId)
                        || input.IncludeLeaseHaulerTrucks && x.OfficeId == null)
                .WhereIf(input.OfficeId != null,
                    x => x.OfficeId == input.OfficeId
                        || input.IncludeLeaseHaulerTrucks && x.OfficeId == null)
                .WhereIf(!input.IncludeLeaseHaulerTrucks, x => x.OfficeId.HasValue)
                .WhereIf(input.LeaseHaulerId.HasValue, x => x.LeaseHaulerTruck.LeaseHaulerId == input.LeaseHaulerId)
                .WhereIf(input.ExcludeInternalTrucks, x => x.LeaseHaulerTruck != null)
                .WhereIf(input.InServiceOnly, x => !x.IsOutOfService)
                .WhereIf(input.ActiveOnly, x => x.IsActive)
                .WhereIf(input.AssetType.HasValue, x => x.VehicleCategory.AssetType == input.AssetType)
                .WhereIf(input.CanPullTrailer.HasValue, x => x.CanPullTrailer == input.CanPullTrailer)
                .WhereIf(input.ExcludeTrailers, x => x.VehicleCategory.IsPowered);

            return await query
                .Select(x => new SelectListDto
                {
                    Id = x.Id.ToString(),
                    Name = x.TruckCode,
                })
                .GetSelectListResult(input);
        }

        [AbpAuthorize(AppPermissions.Pages_Misc_SelectLists_Trucks, AppPermissions.LeaseHaulerPortal_SelectLists_Trucks)]
        public async Task<List<SelectListDto>> GetBedConstructions(GetBedConstructionsInput input)
        {
            var leaseHaulerIdFilter = await GetLeaseHaulerIdFilterAsync(AppPermissions.Pages_Misc_SelectLists_Trucks, AppPermissions.LeaseHaulerPortal_SelectLists_Trucks);

            var bedConstructions = await (await _truckRepository.GetQueryAsync())
                .WhereIf(leaseHaulerIdFilter.HasValue, x => x.LeaseHaulerTruck.LeaseHaulerId == leaseHaulerIdFilter)
                .WhereIf(input.VehicleCategoryId.HasValue, x => x.VehicleCategoryId == input.VehicleCategoryId)
                .Select(x => new
                {
                    x.BedConstruction,
                })
                .Distinct()
                .ToListAsync();

            return bedConstructions
                .Select(x => new SelectListDto
                {
                    Id = x.BedConstruction.ToIntString(),
                    Name = x.BedConstruction.GetDisplayName(),
                })
                .ToList();
        }

        [AbpAuthorize(AppPermissions.Pages_Misc_SelectLists_Trucks, AppPermissions.LeaseHaulerPortal_SelectLists_Trucks)]
        public async Task<PagedResultDto<SelectListDto>> GetMakesSelectList(GetMakesSelectListInput input)
        {
            var leaseHaulerIdFilter = await GetLeaseHaulerIdFilterAsync(AppPermissions.Pages_Misc_SelectLists_Trucks, AppPermissions.LeaseHaulerPortal_SelectLists_Trucks);

            return await (await _truckRepository.GetQueryAsync())
                .WhereIf(leaseHaulerIdFilter.HasValue, x => x.LeaseHaulerTruck.LeaseHaulerId == leaseHaulerIdFilter)
                .WhereIf(input.VehicleCategoryId.HasValue, x => x.VehicleCategoryId == input.VehicleCategoryId)
                .Select(x => new SelectListDto
                {
                    Id = x.Make,
                    Name = x.Make,
                })
                .Distinct()
                .GetSelectListResult(input);
        }

        [AbpAuthorize(AppPermissions.Pages_Misc_SelectLists_Trucks, AppPermissions.LeaseHaulerPortal_SelectLists_Trucks)]
        public async Task<PagedResultDto<SelectListDto>> GetModelsSelectList(GetModelsSelectListInput input)
        {
            var leaseHaulerIdFilter = await GetLeaseHaulerIdFilterAsync(AppPermissions.Pages_Misc_SelectLists_Trucks, AppPermissions.LeaseHaulerPortal_SelectLists_Trucks);

            return await (await _truckRepository.GetQueryAsync())
                .WhereIf(leaseHaulerIdFilter.HasValue, x => x.LeaseHaulerTruck.LeaseHaulerId == leaseHaulerIdFilter)
                .WhereIf(input.VehicleCategoryId.HasValue, x => x.VehicleCategoryId == input.VehicleCategoryId)
                .WhereIf(!string.IsNullOrEmpty(input.Make), x => x.Make == input.Make)
                .Select(x => new SelectListDto
                {
                    Id = x.Model,
                    Name = x.Model,
                })
                .Distinct()
                .GetSelectListResult(input);
        }

        [AbpAuthorize(AppPermissions.Pages_Misc_SelectLists_Trucks, AppPermissions.LeaseHaulerPortal_SelectLists_Trucks)]
        public async Task<PagedResultDto<SelectListDto>> GetVehicleCategoriesSelectList(GetVehicleCategoriesSelectListInput input)
        {
            var leaseHaulerIdFilter = await GetLeaseHaulerIdFilterAsync(AppPermissions.Pages_Misc_SelectLists_Trucks, AppPermissions.LeaseHaulerPortal_SelectLists_Trucks);
            var categoriesInUse = input.IsInUse == true
                ? await (await _truckRepository.GetQueryAsync())
                    .WhereIf(leaseHaulerIdFilter.HasValue, x => x.LeaseHaulerTruck.LeaseHaulerId == leaseHaulerIdFilter)
                    .Select(x => x.VehicleCategoryId)
                    .Distinct()
                    .ToListAsync()
                : new List<int>();

            return await (await _vehicleCategoryRepository.GetQueryAsync())
                .WhereIf(input.IsPowered.HasValue, x => x.IsPowered == input.IsPowered)
                .WhereIf(input.IsInUse == true, x => categoriesInUse.Contains(x.Id))
                .WhereIf(input.AssetType.HasValue, x => x.AssetType == input.AssetType)
                .Select(x => new SelectListDto<VehicleCategorySelectListInfoDto>
                {
                    Id = x.Id.ToString(),
                    Name = x.Name,
                    Item = new VehicleCategorySelectListInfoDto
                    {
                        AssetType = x.AssetType,
                        IsPowered = x.IsPowered,
                    },
                })
                .GetSelectListResult(input);
        }

        public async Task<List<VehicleCategoryDto>> GetVehicleCategories()
        {
            return (await _listCaches.VehicleCategory.GetList(ListCacheEmptyKey.Instance)).Items
                .Select(x => new VehicleCategoryDto(x))
                .OrderBy(x => x.SortOrder)
                .ToList();
        }

        [AbpAuthorize(AppPermissions.Pages_Misc_SelectLists_Trucks, AppPermissions.LeaseHaulerPortal_SelectLists_Trucks)]
        public async Task<PagedResultDto<SelectListDto>> GetActiveTrailersSelectList(GetActiveTrailersSelectListInput input)
        {
            var leaseHaulerIdFilter = await GetLeaseHaulerIdFilterAsync(AppPermissions.Pages_Misc_SelectLists_Trucks, AppPermissions.LeaseHaulerPortal_SelectLists_Trucks);

            return await (await _truckRepository.GetQueryAsync())
                .Where(t => t.VehicleCategory.AssetType == AssetType.Trailer && !t.IsOutOfService)
                .WhereIf(input.Id.HasValue, x => x.Id == input.Id)
                .WhereIf(!input.LeaseHaulerId.HasValue, x => x.OfficeId.HasValue && x.IsActive && x.LeaseHaulerTruck == null)
                .WhereIf(input.LeaseHaulerId.HasValue, x => x.LeaseHaulerTruck.LeaseHaulerId == input.LeaseHaulerId)
                .WhereIf(leaseHaulerIdFilter.HasValue, x => x.LeaseHaulerTruck.LeaseHaulerId == leaseHaulerIdFilter)
                .WhereIf(input.VehicleCategoryId.HasValue, x => x.VehicleCategoryId == input.VehicleCategoryId)
                .WhereIf(input.BedConstruction.HasValue, x => x.BedConstruction == input.BedConstruction)
                .WhereIf(!string.IsNullOrEmpty(input.Make), x => x.Make == input.Make)
                .WhereIf(!string.IsNullOrEmpty(input.Model), x => x.Model == input.Model)
                .Select(x => new SelectListDto<TruckSelectListInfoDto>
                {
                    Id = x.Id.ToString(),
                    Name = x.TruckCode,
                    Item = new TruckSelectListInfoDto
                    {
                        VehicleCategoryId = x.VehicleCategoryId,
                    },
                })
                .GetSelectListResult(input);
        }

        [AbpAuthorize(AppPermissions.Pages_Misc_SelectLists_Trucks)]
        public async Task<PagedResultDto<SelectListDto>> GetActiveTractorsSelectList(GetSelectListInput input)
        {
            return await (await _truckRepository.GetQueryAsync())
                .Where(x => x.OfficeId.HasValue)
                .Where(t => t.CanPullTrailer && t.IsActive && !t.IsOutOfService)
                .Select(x => new SelectListDto
                {
                    Id = x.Id.ToString(),
                    Name = x.TruckCode,
                })
                .GetSelectListResult(input);
        }

        [AbpAuthorize(AppPermissions.Pages_Trucks)]
        public async Task<TruckEditDto> GetTruckForEdit(GetTruckForEditInput input)
        {
            TruckEditDto truckEditDto;

            if (input.Id.HasValue)
            {
                truckEditDto = await (await _truckRepository.GetQueryAsync())
                    .Select(t => new TruckEditDto
                    {
                        Id = t.Id,
                        TruckCode = t.TruckCode,
                        OfficeId = t.OfficeId == null ? 0 : t.OfficeId.Value,
                        OfficeName = t.Office.Name,
                        VehicleCategoryId = t.VehicleCategoryId,
                        VehicleCategoryName = t.VehicleCategory.Name,
                        VehicleCategoryAssetType = t.VehicleCategory.AssetType,
                        VehicleCategoryIsPowered = t.VehicleCategory.IsPowered,
                        BedConstruction = t.BedConstruction,
                        CanPullTrailer = t.CanPullTrailer,
                        IsApportioned = t.IsApportioned,
                        AlwaysShowOnSchedule = t.AlwaysShowOnSchedule,
                        DefaultDriverId = t.DefaultDriverId,
                        DefaultDriverName = t.DefaultDriver != null ? t.DefaultDriver.FirstName + " " + t.DefaultDriver.LastName : "",
                        CurrentTrailerId = t.CurrentTrailerId,
                        CurrentTrailerCode = t.CurrentTrailer.TruckCode,
                        IsActive = t.IsActive,
                        InactivationDate = t.InactivationDate,
                        IsOutOfService = t.IsOutOfService,

                        CurrentMileage = t.CurrentMileage,
                        CurrentHours = t.CurrentHours,
                        Year = t.Year,
                        Make = t.Make,
                        Model = t.Model,
                        Vin = t.Vin,
                        Plate = t.Plate,
                        PlateExpiration = t.PlateExpiration,
                        CargoCapacity = t.CargoCapacity,
                        CargoCapacityCyds = t.CargoCapacityCyds,
                        FuelType = t.FuelType,
                        FuelCapacity = t.FuelCapacity,
                        SteerTires = t.SteerTires,
                        DriveAxleTires = t.DriveAxleTires,
                        DropAxleTires = t.DropAxleTires,
                        TrailerTires = t.TrailerTires,
                        Transmission = t.Transmission,
                        Engine = t.Engine,
                        RearEnd = t.RearEnd,
                        InsurancePolicyNumber = t.InsurancePolicyNumber,
                        InsuranceValidUntil = t.InsuranceValidUntil,
                        PurchaseDate = t.PurchaseDate,
                        PurchasePrice = t.PurchasePrice,
                        InServiceDate = t.InServiceDate,
                        SoldDate = t.SoldDate,
                        TruxTruckId = t.TruxTruckId,
                        SoldPrice = t.SoldPrice,
                        DtdTrackerUniqueId = t.DtdTrackerUniqueId,
                        DtdTrackerDeviceTypeId = t.DtdTrackerDeviceTypeId,
                        DtdTrackerDeviceTypeName = t.DtdTrackerDeviceTypeName,
                        DtdTrackerPassword = t.DtdTrackerPassword,
                        DtdTrackerServerAddress = t.DtdTrackerServerAddress,
                        EnableDriverAppGps = t.EnableDriverAppGps,
                        Notes = t.Notes,
                        Reason = t.IsOutOfService ?
                        t.OutOfServiceHistories
                            .OrderByDescending(oosh => oosh.OutOfServiceDate)
                            .Select(oosh => oosh.Reason).FirstOrDefault()
                        : "",

                        Files = t.Files.Select(f => new TruckFileEditDto
                        {
                            Id = f.Id,
                            TruckId = f.TruckId,
                            Title = f.Title,
                            Description = f.Description,
                            FileId = f.FileId,
                            ThumbnailId = f.ThumbnailId,
                            FileName = f.FileName,
                            FileType = f.FileType,
                        }).ToList(),

                        MobileDevices = t.MobileDevices.Select(d => new MobileDeviceEditDto
                        {
                            Id = d.Id,
                            DeviceType = d.DeviceType,
                            Make = d.Make,
                            Model = d.Model,
                            Imei = d.Imei,
                            SimId = d.SimId,
                        }).ToList(),

                    })
                    .SingleAsync(t => t.Id == input.Id.Value);
            }
            else
            {
                truckEditDto = new TruckEditDto
                {
                    TruckCode = input.Name,
                    IsActive = true,
                    AlwaysShowOnSchedule = true,
                    Files = new List<TruckFileEditDto>(),
                    MobileDevices = new List<MobileDeviceEditDto>(),
                    InServiceDate = await GetToday(),
                };

                if (input.VehicleCategoryId.HasValue)
                {
                    var vehicleCategory = (await _listCaches.VehicleCategory.GetList(ListCacheEmptyKey.Instance))
                        .Find(input.VehicleCategoryId);

                    if (vehicleCategory != null)
                    {
                        truckEditDto.VehicleCategoryId = vehicleCategory.Id;
                        truckEditDto.VehicleCategoryName = vehicleCategory.Name;
                        truckEditDto.VehicleCategoryAssetType = vehicleCategory.AssetType;
                        truckEditDto.VehicleCategoryIsPowered = vehicleCategory.IsPowered;
                    }
                }
            }

            await _singleOfficeService.FillSingleOffice(truckEditDto);

            return truckEditDto;
        }

        [AbpAuthorize(AppPermissions.Pages_Trucks)]
        public async Task<EditTruckResult> EditTruck(TruckEditDto model)
        {
            var result = new EditTruckResult();

            var entity = model.Id.HasValue ? await _truckRepository.GetAsync(model.Id.Value) : new Truck();

            var vehicleCategories = await _listCaches.VehicleCategory.GetList(ListCacheEmptyKey.Instance);

            var newVehicleCategory = vehicleCategories.Find(model.VehicleCategoryId);
            if (newVehicleCategory == null)
            {
                throw new UserFriendlyException("Category is required");
            }

            var oldVehicleCategory = entity.VehicleCategoryId == model.VehicleCategoryId
                ? newVehicleCategory
                : entity.VehicleCategoryId != 0
                    ? vehicleCategories.Find(entity.VehicleCategoryId)
                    : null;

            if ((!model.Id.HasValue || newVehicleCategory != oldVehicleCategory) && newVehicleCategory.IsPowered)
            {
                var currentNumberOfTrucks = await _truckRepository.CountAsync(t => t.OfficeId != null && t.VehicleCategory.IsPowered && t.Id != model.Id) + 1;
                var maxNumberOfTrucks = (await FeatureChecker.GetValueAsync(AppFeatures.NumberOfTrucksFeature)).To<int>();
                if (currentNumberOfTrucks > maxNumberOfTrucks)
                {
                    result.NeededBiggerNumberOfTrucks = currentNumberOfTrucks;
                    return result;
                }
            }

            if (!model.IsActive && model.InactivationDate == null)
            {
                throw new ArgumentException("The InactivationDate must be specified when IsActive=false!");
            }

            if (newVehicleCategory.AssetType == AssetType.Trailer && model.DefaultDriverId.HasValue)
            {
                throw new ArgumentException("A Trailer cannot have a default driver!");
            }
            else if (!newVehicleCategory.IsPowered && model.DefaultDriverId.HasValue)
            {
                throw new ArgumentException("An unpowered vehicle cannot have a default driver!");
            }

            var wasSyncedWithDtdTracker = await ShouldSyncTruckWithDtdTracker(entity, oldVehicleCategory);

            await CreateOrUpdateOutOfServiceHistory(entity, model.IsOutOfService, model.Reason);

            if (entity.Id > 0)
            {
                if (model.IsOutOfService)
                {
                    result.ThereWereAssociatedOrders = await RemoveTruckFromScheduleStartingOnDate(entity.Id);
                    result.ThereWereAssociatedTractors = await RemoveAssociatedTractors(entity.Id);
                }
                if (!model.IsActive)
                {
                    await RemoveTruckFromScheduleStartingOnDate(entity.Id, model.InactivationDate);
                    await RemoveTruckFromDriverAssignmentStartingOnDate(entity.Id, model.InactivationDate);
                }

                if (model.IsOutOfService || !model.IsActive)
                {
                    result.ThereWereCanceledDispatches = await CancelUnacknowledgedDispatches(entity.Id);
                    result.ThereWereNotCanceledDispatches = await ThereAreAcknowledgedOrLoadedDispatches(entity.Id);
                }
            }

            if (await UpdateDefaultDriver())
            {
                result.ThereAreOrdersInTheFuture = await ThereAreOrdersInTheFuture();
            }
            result.ThereWereAssociatedOrders |= await UpdateOffice();

            await ThrowUserFriendlyExceptionIfTruckCodeExists(model.TruckCode, entity.Id, model.OfficeId);

            if (model.Id.HasValue && newVehicleCategory.AssetType == AssetType.Trailer && model.IsActive != true)
            {
                var tractors = await (await _truckRepository.GetQueryAsync()).Where(x => x.CurrentTrailerId == model.Id).ToListAsync();
                tractors.ForEach(x => x.CurrentTrailerId = null);
            }
            await ThrowUserFriendlyExceptionIfTruckWasTrailerAndCategoryChanged();

            entity.TruckCode = model.TruckCode;
            entity.VehicleCategoryId = model.VehicleCategoryId;
            entity.IsActive = model.IsActive;
            entity.InactivationDate = model.IsActive ? null : model.InactivationDate;
            entity.IsOutOfService = model.IsOutOfService;
            entity.IsApportioned = model.IsApportioned;
            entity.AlwaysShowOnSchedule = model.AlwaysShowOnSchedule;
            entity.BedConstruction = model.BedConstruction;
            entity.CanPullTrailer = model.CanPullTrailer;

            entity.CurrentMileage = model.CurrentMileage;
            entity.CurrentHours = model.CurrentHours;
            entity.Year = model.Year;
            entity.Make = model.Make;
            entity.Model = model.Model;
            entity.Vin = model.Vin;
            entity.Plate = model.Plate;
            entity.PlateExpiration = model.PlateExpiration;
            entity.CargoCapacity = model.CargoCapacity;
            entity.CargoCapacityCyds = model.CargoCapacityCyds;
            entity.FuelType = model.FuelType;
            entity.FuelCapacity = model.FuelCapacity;
            entity.SteerTires = model.SteerTires;
            entity.DriveAxleTires = model.DriveAxleTires;
            entity.DropAxleTires = model.DropAxleTires;
            entity.TrailerTires = model.TrailerTires;
            entity.Transmission = model.Transmission;
            entity.Engine = model.Engine;
            entity.RearEnd = model.RearEnd;
            entity.InsurancePolicyNumber = model.InsurancePolicyNumber;
            entity.InsuranceValidUntil = model.InsuranceValidUntil;
            entity.PurchaseDate = model.PurchaseDate;
            entity.PurchasePrice = model.PurchasePrice;
            entity.InServiceDate = model.InServiceDate;
            entity.SoldDate = model.SoldDate;
            entity.SoldPrice = model.SoldPrice;
            entity.Notes = model.Notes;
            if (await FeatureChecker.IsEnabledAsync(AppFeatures.AllowImportingTruxEarnings))
            {
                entity.TruxTruckId = model.TruxTruckId;
            }
            if (await FeatureChecker.IsEnabledAsync(AppFeatures.GpsIntegrationFeature))
            {
                if (model.DtdTrackerUniqueId.IsNullOrEmpty())
                {
                    var tenantId = await Session.GetTenantIdAsync();
                    var tenantDetails = await (await TenantManager.GetQueryAsync())
                        .Where(x => x.Id == tenantId)
                        .Select(x => new
                        {
                            x.Name,
                        }).FirstOrDefaultAsync();

                    model.DtdTrackerUniqueId = tenantDetails.Name + model.TruckCode;
                }
                entity.DtdTrackerUniqueId = model.DtdTrackerUniqueId;
                entity.DtdTrackerDeviceTypeId = model.DtdTrackerDeviceTypeId;
                entity.DtdTrackerDeviceTypeName = model.DtdTrackerDeviceTypeName;
                entity.DtdTrackerPassword = model.DtdTrackerPassword;
                entity.DtdTrackerServerAddress = model.DtdTrackerServerAddress;
                entity.EnableDriverAppGps = model.EnableDriverAppGps;
            }

            await UpdateCurrentTrailer();

            if (model.Id.HasValue)
            {
                await UpdateTruckFiles(entity.Id, model.Files);
            }

            result.Id = await _truckRepository.InsertOrUpdateAndGetIdAsync(entity);


            var existingMobileDevices = await (await _mobileDeviceRepository.GetQueryAsync())
                .Where(d => d.TruckId == result.Id)
                .ToListAsync();
            var mobileDevicesToDelete = existingMobileDevices
                .Where(d => !model.MobileDevices.Any(i => i.Id == d.Id))
                .ToList();
            await _mobileDeviceRepository.DeleteRangeAsync(mobileDevicesToDelete);

            var mobileDevicesToAdd = model.MobileDevices
                .Where(x => !existingMobileDevices.Any(d => d.Id == x.Id))
                .ToList();
            await _mobileDeviceRepository.InsertRangeAsync(mobileDevicesToAdd.Select(i => new MobileDevice
            {
                TruckId = result.Id,
                DeviceType = i.DeviceType,
                Make = i.Make,
                Model = i.Model,
                Imei = i.Imei,
                SimId = i.SimId,
            }).ToList());

            foreach (var mobileDevice in existingMobileDevices)
            {
                var mobileDeviceModel = model.MobileDevices
                    .FirstOrDefault(x => x.Id == mobileDevice.Id);
                if (mobileDeviceModel == null)
                {
                    continue;
                }
                mobileDevice.Make = mobileDeviceModel.Make;
                mobileDevice.Model = mobileDeviceModel.Model;
                mobileDevice.Imei = mobileDeviceModel.Imei;
                mobileDevice.SimId = mobileDeviceModel.SimId;
            }
            await CurrentUnitOfWork.SaveChangesAsync();


            await _crossTenantOrderSender.SyncMaterialCompanyTrucksIfNeeded(entity.Id);

            if (!wasSyncedWithDtdTracker && await ShouldSyncTruckWithDtdTracker(entity, newVehicleCategory))
            {
                await _truckTelematicsAppService.SyncWithWialon(new SyncWithWialonInput
                {
                    LocalTruckIds = new() { result.Id },
                });
            }

            return result;

            // Local functions
            async Task<bool> UpdateDefaultDriver()
            {
                if (model.DefaultDriverId.HasValue)
                {
                    var otherTrucksWithSameDefaultDriver = await (await _truckRepository.GetQueryAsync())
                        .Where(t =>
                            t.OfficeId != null
                            && t.Id != entity.Id && t.DefaultDriverId == model.DefaultDriverId)
                        .ToListAsync();
                    otherTrucksWithSameDefaultDriver.ForEach(x => x.DefaultDriverId = null);
                }

                if (entity.DefaultDriverId == model.DefaultDriverId)
                {
                    return false;
                }

                await SetDriverIdNullInDriverAssignments(entity.Id, entity.DefaultDriverId);

                entity.DefaultDriverId = model.DefaultDriverId;

                if (entity.Id == 0 && model.DefaultDriverId.HasValue)
                {
                    //we need to create driver assignments for the existing default driver with time off records, and for that we need to have truckId
                    await CurrentUnitOfWork.SaveChangesAsync();
                }
                await AddTimeOffDriverAssignmentsIfNeeded(entity.Id, model.DefaultDriverId);

                return true;
            }
            async Task<bool> ThereAreOrdersInTheFuture()
            {
                var today = await GetToday();
                return await (await _orderLineTruckRepository.GetQueryAsync())
                    .AnyAsync(olt => olt.TruckId == entity.Id && olt.OrderLine.Order.DeliveryDate > today);
            }

            async Task<bool> UpdateOffice()
            {
                if (entity.OfficeId == model.OfficeId)
                {
                    return false;
                }
                entity.OfficeId = model.OfficeId;
                if (model.Id.HasValue)
                {
                    await RemoveTruckFromDriverAssignmentStartingOnDate(model.Id.Value, null);
                    return await RemoveTruckFromScheduleStartingOnDate(model.Id.Value);
                }
                return false;
            }

            async Task UpdateCurrentTrailer()
            {
                if (!entity.CanPullTrailer && model.CurrentTrailerId != null)
                {
                    throw new ArgumentException("The truck must be able to pull a trailer to set a CurrentTrailerId");
                }
                if (entity.CanPullTrailer && model.CurrentTrailerId != null)
                {
                    var trailer = await (await _truckRepository.GetQueryAsync())
                        .Where(t => t.Id == model.CurrentTrailerId)
                        .Select(t => new
                        {
                            t.VehicleCategory.AssetType,
                            t.IsActive,
                            t.IsOutOfService,
                        })
                        .FirstOrDefaultAsync();
                    if (trailer.AssetType != AssetType.Trailer)
                    {
                        throw new UserFriendlyException("The current trailer must be a trailer!");
                    }
                    if (!trailer.IsActive || trailer.IsOutOfService)
                    {
                        throw new UserFriendlyException("The current trailer must be active!");
                    }
                    var tractorWithCurrentTrailer = await (await _truckRepository.GetQueryAsync())
                        .WhereIf(model.Id != null, t => t.Id != model.Id)
                        .Where(t => t.CurrentTrailerId == model.CurrentTrailerId)
                        .FirstOrDefaultAsync();
                    if (tractorWithCurrentTrailer != null)
                    {
                        tractorWithCurrentTrailer.CurrentTrailerId = null;
                    }
                }

                entity.CurrentTrailerId = model.CurrentTrailerId;
            }

            async Task ThrowUserFriendlyExceptionIfTruckCodeExists(string truckCode, int currentTruckId, int officeId)
            {
                if (await (await _truckRepository.GetQueryAsync())
                        .AnyAsync(t => t.Id != currentTruckId && t.TruckCode == truckCode
                            && t.OfficeId == officeId && t.LeaseHaulerTruck.AlwaysShowOnSchedule != true))
                {
                    throw new UserFriendlyException($"A truck with truck code '{truckCode}' already exists in the same office.");
                }
            }

            async Task ThrowUserFriendlyExceptionIfTruckWasTrailerAndCategoryChanged()
            {
                if (model.Id != null
                    && oldVehicleCategory.AssetType == AssetType.Trailer
                    && newVehicleCategory.AssetType != AssetType.Trailer)
                {
                    if (model.IsActive)
                    {
                        var tractorCode = await GetTractorWithCurrentTrailer(new TrailerIsSetAsCurrentTrailerForAnotherTractorInput { TrailerId = model.Id.Value });
                        if (!string.IsNullOrEmpty(tractorCode))
                        {
                            throw new UserFriendlyException($"This trailer is the current trailer on {tractorCode} and its category can't be changed while it's assigned to a truck.");
                        }
                    }
                    if (await TruckHasHistory(model.Id.Value))
                    {
                        throw new UserFriendlyException($"This trailer already has history, so its category can't be changed.");
                    }
                }
                if (model.Id != null
                    && oldVehicleCategory.AssetType != AssetType.Trailer
                    && newVehicleCategory.AssetType == AssetType.Trailer)
                {
                    if (await TruckHasHistory(model.Id.Value))
                    {
                        throw new UserFriendlyException($"This truck already has history, so its category can't be changed.");
                    }
                }
            }
        }

        private async Task<bool> ShouldSyncTruckWithDtdTracker(Truck entity, VehicleCategoryCacheItem vehicleCategory)
        {
            return entity.EnableDriverAppGps
                && !entity.DtdTrackerUniqueId.IsNullOrEmpty()
                && entity.DtdTrackerDeviceTypeId != null
                && entity.IsActive
                && vehicleCategory?.IsPowered == true
                && await FeatureChecker.IsEnabledAsync(AppFeatures.GpsIntegrationFeature)
                && await SettingManager.GetSettingValueAsync<bool>(AppSettings.GpsIntegration.DtdTracker.EnableDriverAppGps)
                && (GpsPlatform)await SettingManager.GetSettingValueAsync<int>(AppSettings.GpsIntegration.Platform) == GpsPlatform.DtdTracker;
        }

        private async Task SetDriverIdNullInDriverAssignments(int truckId, int? driverId)
        {
            if (truckId == 0 || !driverId.HasValue)
            {
                return;
            }
            var today = await GetToday();
            var driverAssignments = await (await _driverAssignmentRepository.GetQueryAsync())
                .Where(da => da.DriverId == driverId && da.TruckId == truckId && da.Date >= today)
                .ToListAsync();
            foreach (var driverAssignment in driverAssignments)
            {
                driverAssignment.DriverId = null;
            }
        }

        private async Task AddTimeOffDriverAssignmentsIfNeeded(int truckId, int? driverId)
        {
            if (truckId == 0 || !driverId.HasValue)
            {
                return;
            }
            var today = await GetToday();

            var timeOffs = await (await _timeOffRepository.GetQueryAsync())
                .Where(x => x.DriverId == driverId && x.EndDate >= today)
                .Select(x => new { x.StartDate, x.EndDate })
                .ToListAsync();

            if (!timeOffs.Any())
            {
                return;
            }

            var allShifts = await SettingManager.GetShiftsAsync();
            var existingAssignments = await (await _driverAssignmentRepository.GetQueryAsync())
                .Where(da => da.TruckId == truckId && da.Date >= today)
                .ToListAsync();
            var truck = await _truckCache.GetTruckFromCacheOrDbOrDefault(truckId);

            foreach (var timeOff in timeOffs)
            {
                var date = timeOff.StartDate < today ? today : timeOff.StartDate;
                if (date < today)
                {
                    date = today;
                }

                while (date <= timeOff.EndDate)
                {
                    foreach (var shift in allShifts)
                    {
                        var existingAssignment = existingAssignments.FirstOrDefault(x => x.Date == date && x.Shift == shift);
                        if (existingAssignment == null)
                        {
                            var newDriverAssignment = new DriverAssignment
                            {
                                OfficeId = truck.OfficeId,
                                Date = date,
                                Shift = shift,
                                TruckId = truckId,
                                DriverId = null,
                            };
                            await _driverAssignmentRepository.InsertAsync(newDriverAssignment);
                            existingAssignments.Add(newDriverAssignment);
                        }
                    }
                    date = date.AddDays(1);
                }
            }
        }

        private async Task<bool> TruckHasHistory(int truckId)
        {
            return await (await _ticketRepository.GetQueryAsync()).AnyAsync(x => x.TruckId == truckId)
                || await (await _orderLineTruckRepository.GetQueryAsync()).AnyAsync(x => x.TruckId == truckId);
        }

        [AbpAuthorize(AppPermissions.Pages_Trucks, AppPermissions.Pages_LeaseHaulers_Edit, AppPermissions.LeaseHaulerPortal_MyCompany_Trucks)]
        public async Task<string> GetTractorWithCurrentTrailer(TrailerIsSetAsCurrentTrailerForAnotherTractorInput input)
        {
            var permissions = new
            {
                ViewTrucks = await IsGrantedAsync(AppPermissions.Pages_Trucks),
                EditLeaseHaulers = await IsGrantedAsync(AppPermissions.Pages_LeaseHaulers_Edit),
                LeaseHaulerTrucks = await IsGrantedAsync(AppPermissions.LeaseHaulerPortal_MyCompany_Trucks),
            };
            if (permissions.ViewTrucks || permissions.EditLeaseHaulers)
            {
                // do nothing
            }
            else if (permissions.LeaseHaulerTrucks)
            {
                var leaseHaulerId = Session.GetLeaseHaulerIdOrThrow(this);
                var exists = await (await _leaseHaulerTruckRepository.GetQueryAsync())
                    .Where(q => q.LeaseHaulerId == leaseHaulerId && q.TruckId == input.TrailerId)
                    .AnyAsync();
                if (!exists)
                {
                    throw new AbpAuthorizationException();
                }
            }
            else
            {
                throw new AbpAuthorizationException();
            }

            return await (await _truckRepository.GetQueryAsync())
                .WhereIf(input.TractorId != null, t => t.Id != input.TractorId)
                .Where(t => t.CurrentTrailerId == input.TrailerId)
                .Select(t => t.TruckCode)
                .FirstOrDefaultAsync();
        }

        [AbpAuthorize(AppPermissions.Pages_Trucks)]
        public async Task<string> GetTruckCodeByDefaultDriver(GetTruckCodeByDefaultDriverInput input)
        {
            return await (await _truckRepository.GetQueryAsync())
                .WhereIf(input.ExceptTruckId.HasValue, t => t.Id != input.ExceptTruckId)
                .Where(t => t.OfficeId != null
                    && t.LeaseHaulerTruck.AlwaysShowOnSchedule != true
                    && t.DefaultDriverId == input.DefaultDriverId)
                .Select(t => t.TruckCode)
                .FirstOrDefaultAsync();
        }

        [AbpAuthorize(AppPermissions.Pages_Trucks, AppPermissions.Pages_Schedule)]
        public async Task<string> GetTruckCodeByTruckId(int truckId)
        {
            return await (await _truckRepository.GetQueryAsync())
                .Where(t => t.Id == truckId)
                .Select(t => t.TruckCode)
                .FirstOrDefaultAsync();
        }

        [AbpAuthorize(AppPermissions.Pages_Trucks)]
        public async Task RemoveDefaultDriver(int truckId, int driverId)
        {
            var today = await GetToday();
            await RemoveTodayAndFutureDriverAssignmentsForTruckAndDriver(truckId, driverId, today);
            await CurrentUnitOfWork.SaveChangesAsync();
            await CompleteTodayAndRemoveFutureOrderLineTrucksWithoutDriverAssignmentForTruck(truckId, driverId, today);

        }
        [RemoteService(false)]
        public async Task RemoveTodayAndFutureDriverAssignmentsForTruckAndDriver(int truckId, int driverId, DateTime today)
        {
            await _driverAssignmentRepository.DeleteAsync(da => da.TruckId == truckId && da.DriverId == driverId && da.Date >= today);
        }

        [RemoteService(false)]
        public async Task CompleteTodayAndRemoveFutureOrderLineTrucksWithoutDriverAssignmentForTruck(int truckId, int driverId, DateTime today)
        {
            var orderLineTruckQuery = (await _orderLineTruckRepository.GetQueryAsync())
                .Where(olt => olt.TruckId == truckId && olt.DriverId == driverId && olt.OrderLine.Order.DeliveryDate >= today);
            var driverAssignmentQuery = (await _driverAssignmentRepository.GetQueryAsync())
                .Where(da => da.TruckId == truckId && da.Date >= today && da.DriverId != null);
            var orderLineTrucksToDeleteOrCompleteQuery =
                from olt in orderLineTruckQuery
                join da in driverAssignmentQuery on new { olt.TruckId, Date = olt.OrderLine.Order.DeliveryDate } equals new { da.TruckId, da.Date }
                    into gj
                from driverAssignment in gj.DefaultIfEmpty()
                where driverAssignment == null
                select new { olt.Id, olt.OrderLine.Order.DeliveryDate };

            var idsToDeleteOrComplete = await orderLineTrucksToDeleteOrCompleteQuery.ToListAsync();
            var idGroupsToDeleteOrComplete = idsToDeleteOrComplete.GroupBy(x => new { MarkComplete = x.DeliveryDate == today });
            foreach (var idGroup in idGroupsToDeleteOrComplete)
            {
                var ids = idGroup.Select(x => x.Id).ToList();
                var orderLineTrucks = await (await _orderLineTruckRepository.GetQueryAsync())
                    .Where(x => ids.Contains(x.Id) || x.ParentOrderLineTruckId.HasValue && ids.Contains(x.ParentOrderLineTruckId.Value))
                    .ToListAsync();
                if (idGroup.Key.MarkComplete)
                {
                    orderLineTrucks.ForEach(olt =>
                    {
                        olt.IsDone = true;
                        olt.Utilization = 0;
                    });
                }
                else
                {
                    await _orderLineTruckRepository.DeleteRangeAsync(orderLineTrucks);
                    var orderLineIds = orderLineTrucks.Select(x => x.OrderLineId).Distinct().ToList();
                    await CurrentUnitOfWork.SaveChangesAsync();
                    foreach (var orderLineId in orderLineIds)
                    {
                        var orderLineUpdater = _orderLineUpdaterFactory.Create(orderLineId);
                        var order = await orderLineUpdater.GetOrderAsync();
                        if (order.DeliveryDate >= await GetToday())
                        {
                            orderLineUpdater.UpdateStaggeredTimeOnTrucksOnSave();
                            await orderLineUpdater.SaveChangesAsync();
                        }
                    }
                }
            }
        }

        private async Task CreateOrUpdateOutOfServiceHistory(Truck entity, bool isOutOfService, string reason)
        {
            if (entity.IsOutOfService == isOutOfService)
            {
                if (isOutOfService)
                {
                    await UpdateOutOfServiceHistoryReason(entity.Id, reason);
                }
                return;
            }

            if (isOutOfService)
            {
                entity.OutOfServiceHistories.Add(new OutOfServiceHistory
                {
                    OutOfServiceDate = Clock.Now,
                    Reason = reason.Truncate(500),
                });
            }
            else
            {
                Debug.Assert(isOutOfService == false && entity.IsOutOfService == true);
                await _outOfServiceHistoryRepository.SetInServiceDate(entity.Id, Clock.Now);
            }
        }

        private async Task UpdateOutOfServiceHistoryReason(int truckId, string reason)
        {
            var outOfServiceHistory = await (await _outOfServiceHistoryRepository.GetQueryAsync())
                .Where(oosh => oosh.TruckId == truckId)
                .OrderByDescending(oosh => oosh.OutOfServiceDate)
                .FirstAsync();
            outOfServiceHistory.Reason = reason.Truncate(500);
        }

        private async Task UpdateTruckFiles(int truckId, List<TruckFileEditDto> truckFiles)
        {
            var truckFileEntities = await _truckFileRepository.GetAllListAsync(tf => tf.TruckId == truckId);
            truckFileEntities.ForEach(entity =>
            {
                var dto = truckFiles.First(x => x.Id == entity.Id);
                entity.Title = dto.Title;
                entity.Description = dto.Description;
            });
        }

        [AbpAuthorize(AppPermissions.Pages_Trucks)]
        public async Task<SetTruckIsOutOfServiceResult> SetTruckIsOutOfService(SetTruckIsOutOfServiceInput input)
        {
            var truck = await _truckRepository.GetAsync(input.TruckId);
            await CreateOrUpdateOutOfServiceHistory(truck, input.IsOutOfService, input.Reason);
            var result = new SetTruckIsOutOfServiceResult();
            if (input.IsOutOfService)
            {
                result.ThereWereAssociatedOrders = await RemoveTruckFromScheduleStartingOnDate(truck.Id);
                result.ThereWereCanceledDispatches = await CancelUnacknowledgedDispatches(truck.Id);
                result.ThereWereNotCanceledDispatches = await ThereAreAcknowledgedOrLoadedDispatches(truck.Id);
                result.ThereWereAssociatedTractors = await RemoveAssociatedTractors(truck.Id);
            }
            truck.IsOutOfService = input.IsOutOfService;
            return result;
        }

        private async Task<bool> RemoveTruckFromScheduleStartingOnDate(int truckId, DateTime? date = null)
        {
            var today = await GetToday();
            var startingDate = date ?? today;

            var orderLineTrucksToRemove = await (await _orderLineTruckRepository.GetQueryAsync())
                .Where(olt => olt.TruckId == truckId && olt.OrderLine.Order.DeliveryDate >= startingDate)
                .Select(olt => new
                {
                    olt.Id,
                    olt.OrderLine.Order.DeliveryDate,
                })
                .ToListAsync();

            var orderLineTruckIdsToDelete = orderLineTrucksToRemove.Where(x => x.DeliveryDate != today).Select(x => x.Id).ToList();
            var orderLineTrucksToDelete = await (await _orderLineTruckRepository.GetQueryAsync())
                .Where(x => orderLineTruckIdsToDelete.Contains(x.Id) || x.ParentOrderLineTruckId.HasValue && orderLineTruckIdsToDelete.Contains(x.ParentOrderLineTruckId.Value))
                .ToListAsync();
            await _orderLineTruckRepository.DeleteRangeAsync(orderLineTrucksToDelete);

            if (orderLineTrucksToRemove.Any(x => x.DeliveryDate == today))
            {
                var ids = orderLineTrucksToRemove.Select(x => x.Id).ToList();
                var orderLineTrucksToComplete = await (await _orderLineTruckRepository.GetQueryAsync()).Where(x => ids.Contains(x.Id) || x.ParentOrderLineTruckId.HasValue && ids.Contains(x.ParentOrderLineTruckId.Value)).ToListAsync();
                foreach (var orderLineTruck in orderLineTrucksToComplete)
                {
                    orderLineTruck.IsDone = true;
                    orderLineTruck.Utilization = 0;
                }
            }

            var orderLineIdsNeedingStaggeredTimeRecalculation = orderLineTrucksToDelete.Select(x => x.OrderLineId).Distinct().ToList();
            if (orderLineIdsNeedingStaggeredTimeRecalculation.Any())
            {
                await CurrentUnitOfWork.SaveChangesAsync();
                foreach (var orderLineId in orderLineIdsNeedingStaggeredTimeRecalculation.Distinct().ToList())
                {
                    var orderLineUpdater = _orderLineUpdaterFactory.Create(orderLineId);
                    var order = await orderLineUpdater.GetOrderAsync();
                    if (order.DeliveryDate >= today)
                    {
                        orderLineUpdater.UpdateStaggeredTimeOnTrucksOnSave();
                        await orderLineUpdater.SaveChangesAsync();
                    }
                }
            }

            return orderLineTrucksToRemove.Count > 0;
        }

        private async Task<bool> RemoveTruckFromDriverAssignmentStartingOnDate(int truckId, DateTime? date)
        {
            var today = await GetToday();
            var startingDate = date ?? today;
            var driverAssignmentIdsToRemove = await (await _driverAssignmentRepository.GetQueryAsync())
                .Where(da => da.TruckId == truckId && da.Date >= startingDate && da.Date != today)
                .Select(da => da.Id)
                .ToListAsync();
            foreach (var driverAssignmentId in driverAssignmentIdsToRemove)
            {
                await _driverAssignmentRepository.DeleteAsync(driverAssignmentId);
            }

            return driverAssignmentIdsToRemove.Count > 0;
        }

        private async Task<bool> CancelUnacknowledgedDispatches(int truckId)
        {
            var syncRequest = new SyncRequest();
            var dispatchesToCancel = await (await _dispatchRepository.GetQueryAsync())
                .Where(d => d.TruckId == truckId && (d.Status == DispatchStatus.Created || d.Status == DispatchStatus.Sent))
                .Select(d => d)
                .ToListAsync();
            foreach (var dispatch in dispatchesToCancel)
            {
                dispatch.Status = DispatchStatus.Canceled;
                dispatch.Canceled = Clock.Now;
                syncRequest.AddChange(EntityEnum.Dispatch, dispatch.ToChangedEntity(), ChangeType.Removed);
            }

            if (dispatchesToCancel.Any())
            {
                await CurrentUnitOfWork.SaveChangesAsync();

                await _syncRequestSender.SendSyncRequest(syncRequest
                        .AddLogMessage($"Inactivated truckId {truckId} canceled unacknowledged dispatch(es)"));
            }

            return dispatchesToCancel.Count > 0;
        }

        private async Task<bool> ThereAreAcknowledgedOrLoadedDispatches(int truckId)
        {
            return await (await _dispatchRepository.GetQueryAsync())
                .AnyAsync(d =>
                    d.TruckId == truckId
                    && (d.Status == DispatchStatus.Acknowledged || d.Status == DispatchStatus.Loaded));
        }

        private async Task<bool> RemoveAssociatedTractors(int trailerId)
        {
            var tractors = await (await _truckRepository.GetQueryAsync())
                .Where(x => x.CurrentTrailerId == trailerId)
                .ToListAsync();

            tractors.ForEach(x => x.CurrentTrailerId = null);

            return tractors.Count > 0;
        }

        [AbpAuthorize(AppPermissions.Pages_Trucks)]
        public async Task<bool> CanDeleteTruck(EntityDto input)
        {
            bool hasDependencies = await (await _truckRepository.GetQueryAsync())
                .Where(t => t.Id == input.Id)
                .Where(t =>
                    t.OrderLineTrucksOfTruck.Any()
                    || t.OrderLineTrucksOfTrailer.Any()
                    || t.TicketsOfTruck.Any(ticket => ticket.CarrierId == null)
                    || t.TicketsOfTrailer.Any(ticket => ticket.CarrierId == null)
                    || t.PreventiveMaintenances.Any()
                    || t.DriverAssignments.Any()
                    || t.CurrentTractors.Any()
                    || t.WorkOrders.Any()
                )
                .AnyAsync();

            return !hasDependencies;
        }

        [AbpAuthorize(AppPermissions.Pages_Trucks, AppPermissions.LeaseHaulerPortal_MyCompany_Trucks)]
        public async Task DeleteTruck(EntityDto input)
        {
            var canDelete = await CanDeleteTruck(input);
            if (!canDelete)
            {
                throw new UserFriendlyException(L("UnableToDeleteTruckWithAssociatedData"));
            }

            var truck = await (await _truckRepository.GetQueryAsync())
                .Include(t => t.LeaseHaulerTruck)
                .FirstAsync(t => t.Id == input.Id);
            await CheckEntitySpecificPermissions(
                AppPermissions.Pages_Trucks,
                AppPermissions.LeaseHaulerPortal_MyCompany_Trucks,
                Session.LeaseHaulerId,
                truck.LeaseHaulerTruck?.LeaseHaulerId);

            await _truckRepository.DeleteAsync(truck);
        }

        [AbpAuthorize(AppPermissions.Pages_Trucks)]
        public async Task<PagedResultDto<PreventiveMaintenanceByTruckDto>> GetPreventiveMaintenanceDueByTruck(int truckId)
        {
            var items = await (await _truckRepository.GetQueryAsync())
                .Where(t => t.Id == truckId)
                .SelectMany(t => t.PreventiveMaintenances)
                .Select(pm => new PreventiveMaintenanceByTruckDto
                {
                    VehicleServiceName = pm.VehicleService.Name,
                    DueDate = pm.DueDate,
                    DueMileage = pm.DueMileage,
                    DueHour = pm.DueHour,
                })
                .ToListAsync();

            return new PagedResultDto<PreventiveMaintenanceByTruckDto>(items.Count, items);
        }

        [AbpAuthorize(AppPermissions.Pages_Trucks)]
        public async Task<PagedResultDto<TruckServiceHistoryDto>> GetServiceHistoryByTruck(int truckId)
        {
            var items = await (await _truckRepository.GetQueryAsync())
                .Where(t => t.Id == truckId)
                .SelectMany(t => t.WorkOrders)
                .Where(wo => wo.CompletionDate.HasValue)
                .SelectMany(wo => wo.WorkOrderLines)
                .Select(wol => new TruckServiceHistoryDto
                {
                    VehicleServiceName = wol.VehicleService.Name,
                    CompletionDate = wol.WorkOrder.CompletionDate.Value,
                    Mileage = wol.WorkOrder.Odometer,
                })
                .ToListAsync();

            return new PagedResultDto<TruckServiceHistoryDto>(items.Count, items);
        }

        [AbpAuthorize(AppPermissions.Pages_Trucks)]
        public async Task SaveCurrentMileageAndHours(SaveCurrentMileageInput input)
        {
            Truck entity = await _truckRepository.GetAsync(input.TruckId);
            entity.CurrentMileage = input.CurrentMileage;
            entity.CurrentHours = input.CurrentHours;
        }

        [AbpAuthorize(AppPermissions.Pages_Trucks)]
        public async Task<TruckFileEditDto> SaveFile(TruckFileEditDto model)
        {
            TruckFile entity = model.Id != 0 ? await _truckFileRepository.GetAsync(model.Id) : new TruckFile();

            entity.TruckId = model.TruckId;
            entity.FileId = model.FileId;
            entity.ThumbnailId = model.ThumbnailId;
            entity.FileName = model.FileName;
            entity.Title = model.Title;
            entity.Description = model.Description;
            entity.FileType = model.FileType;

            model.Id = await _truckFileRepository.InsertOrUpdateAndGetIdAsync(entity);
            return model;
        }

        [AbpAuthorize(AppPermissions.Pages_Trucks)]
        public async Task<TruckFileEditDto> GetTruckFileEditDto(int id)
        {
            return await (await _truckFileRepository.GetQueryAsync())
                .Select(x => new TruckFileEditDto
                {
                    Id = x.Id,
                    TruckId = x.TruckId,
                    Title = x.Title,
                    Description = x.Description,
                    FileId = x.FileId,
                    ThumbnailId = x.ThumbnailId,
                    FileName = x.FileName,
                    FileType = x.FileType,
                }
                )
                .SingleAsync(x => x.Id == id);
        }

        [AbpAuthorize(AppPermissions.Pages_Trucks)]
        public async Task DeleteFile(EntityDto input)
        {
            var entity = await _truckFileRepository.GetAsync(input.Id);
            await _attachmentHelper.DeleteFromAzureBlobAsync($"{entity.TruckId}/{entity.FileId}", BlobContainerNames.TruckFiles);
            await _truckFileRepository.DeleteAsync(entity);
        }

        [AbpAuthorize(AppPermissions.Pages_Trucks, AppPermissions.Pages_LeaseHaulers_Edit)]
        public async Task UpdateMaxNumberOfTrucksFeatureAndNotifyAdmins(UpdateMaxNumberOfTrucksFeatureAndNotifyAdminsInput input)
        {
            await _truckCommonDomainService.UpdateMaxNumberOfTrucksFeatureAndNotifyAdmins(input);
        }

    }
}

