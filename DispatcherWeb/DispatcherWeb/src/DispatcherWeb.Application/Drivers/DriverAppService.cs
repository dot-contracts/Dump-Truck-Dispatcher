using System.Collections.Generic;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Threading.Tasks;
using Abp.Application.Services.Dto;
using Abp.Authorization;
using Abp.Configuration;
using Abp.Domain.Repositories;
using Abp.Extensions;
using Abp.Linq.Extensions;
using Abp.UI;
using DispatcherWeb.Authorization;
using DispatcherWeb.Configuration;
using DispatcherWeb.Dispatching;
using DispatcherWeb.Drivers.Dto;
using DispatcherWeb.Drivers.Exporting;
using DispatcherWeb.Dto;
using DispatcherWeb.Features;
using DispatcherWeb.Infrastructure.Extensions;
using DispatcherWeb.LeaseHaulers;
using DispatcherWeb.Offices;
using DispatcherWeb.Orders;
using DispatcherWeb.SyncRequests;
using DispatcherWeb.TimeClassifications;
using DispatcherWeb.Trucks;
using Microsoft.EntityFrameworkCore;

namespace DispatcherWeb.Drivers
{
    [AbpAuthorize]
    public class DriverAppService : DispatcherWebAppServiceBase, IDriverAppService
    {
        private readonly IRepository<Driver> _driverRepository;
        private readonly IRepository<DriverAssignment> _driverAssignmentRepository;
        private readonly IRepository<Truck> _truckRepository;
        private readonly IRepository<OrderLineTruck> _orderLineTruckRepository;
        private readonly IRepository<OrderLine> _orderLineRepository;
        private readonly IRepository<Dispatch> _dispatchRepository;
        private readonly IRepository<TimeClassification> _timeClassificationRepository;
        private readonly IRepository<EmployeeTimeClassification> _employeeTimeClassificationRepository;
        private readonly IDriverListCsvExporter _driverListCsvExporter;
        private readonly IDriverUserLinkService _driverUserLinkService;
        private readonly ISingleOfficeAppService _singleOfficeService;
        private readonly IDriverInactivatorService _driverInactivatorService;
        private readonly ICrossTenantOrderSender _crossTenantOrderSender;
        private readonly ISyncRequestSender _syncRequestSender;

        public DriverAppService(
            IRepository<Driver> driverRepository,
            IRepository<DriverAssignment> driverAssignmentRepository,
            IRepository<Truck> truckRepository,
            IRepository<OrderLineTruck> orderLineTruckRepository,
            IRepository<OrderLine> orderLineRepository,
            IRepository<Dispatch> dispatchRepository,
            IRepository<TimeClassification> timeClassificationRepository,
            IRepository<EmployeeTimeClassification> employeeTimeClassificationRepository,
            IDriverListCsvExporter driverListCsvExporter,
            IDriverUserLinkService driverUserLinkService,
            ISingleOfficeAppService singleOfficeService,
            IDriverInactivatorService driverInactivatorService,
            ICrossTenantOrderSender crossTenantOrderSender,
            ISyncRequestSender syncRequestSender
            )
        {
            _driverRepository = driverRepository;
            _driverAssignmentRepository = driverAssignmentRepository;
            _truckRepository = truckRepository;
            _orderLineTruckRepository = orderLineTruckRepository;
            _orderLineRepository = orderLineRepository;
            _dispatchRepository = dispatchRepository;
            _timeClassificationRepository = timeClassificationRepository;
            _employeeTimeClassificationRepository = employeeTimeClassificationRepository;
            _driverListCsvExporter = driverListCsvExporter;
            _driverUserLinkService = driverUserLinkService;
            _singleOfficeService = singleOfficeService;
            _driverInactivatorService = driverInactivatorService;
            _crossTenantOrderSender = crossTenantOrderSender;
            _syncRequestSender = syncRequestSender;
        }

        [AbpAuthorize(AppPermissions.Pages_Drivers)]
        public async Task<PagedResultDto<DriverDto>> GetDrivers(GetDriversInput input)
        {
            var query = await GetFilteredDriverQuery(input);

            var totalCount = await query.CountAsync();

            var items = await GetDriverDtoQuery(query)
                .OrderBy(input.Sorting)
                .PageBy(input)
                .ToListAsync();

            return new PagedResultDto<DriverDto>(
                totalCount,
                items);
        }

        private async Task<IQueryable<Driver>> GetFilteredDriverQuery(IGetDriverListFilter input)
        {
            var query = (await _driverRepository.GetQueryAsync())
                .Where(x => !x.IsExternal)
                .WhereIf(input.Status == FilterActiveStatus.Active, x => !x.IsInactive)
                .WhereIf(input.Status == FilterActiveStatus.Inactive, x => x.IsInactive)
                .WhereIf(input.OfficeId.HasValue, x => x.OfficeId == input.OfficeId)
                .WhereIf(input.HasUserId.HasValue, x => input.HasUserId.Value && x.UserId.HasValue || !input.HasUserId.Value && !x.UserId.HasValue);

            if (!input.Name.IsNullOrEmpty())
            {
                var lastNameFilterCount = await query.CountAsync(x => x.LastName.StartsWith(input.Name));
                if (lastNameFilterCount < 5)
                {
                    query = query
                        .Where(x => x.FirstName.StartsWith(input.Name) || x.LastName.StartsWith(input.Name));
                }
                else
                {
                    query = query
                        .Where(x => x.LastName.StartsWith(input.Name));
                }
            }

            return query;
        }

        private IQueryable<DriverDto> GetDriverDtoQuery(IQueryable<Driver> query)
        {
            return query.Select(x => new DriverDto
            {
                Id = x.Id,
                FirstName = x.FirstName,
                LastName = x.LastName,
                OfficeName = x.Office.Name,
                IsInactive = x.IsInactive,
                LicenseNumber = x.LicenseNumber,
                LicenseState = x.LicenseState,
                TypeOfLicense = x.TypeOfLicense,
                LicenseExpirationDate = x.LicenseExpirationDate,
                LastPhysicalDate = x.LastPhysicalDate,
                NextPhysicalDueDate = x.NextPhysicalDueDate,
                LastMvrDate = x.LastMvrDate,
                NextMvrDueDate = x.NextMvrDueDate,
                DateOfHire = x.DateOfHire,
                TerminationDate = x.TerminationDate,
                EmployeeId = x.EmployeeId,
            });
        }

        [AbpAuthorize(AppPermissions.Pages_Drivers)]
        public async Task<FileDto> GetDriversToCsv(GetDriversInput input)
        {
            var query = await GetFilteredDriverQuery(input);
            var items = await GetDriverDtoQuery(query)
                .OrderBy(input.Sorting)
                .ToListAsync();

            if (!items.Any())
            {
                throw new UserFriendlyException(L("ThereIsNoDataToExport"));
            }

            return await _driverListCsvExporter.ExportToFileAsync(items);
        }

        [AbpAuthorize(AppPermissions.Pages_Misc_SelectLists_Drivers, AppPermissions.LeaseHaulerPortal_SelectLists_Drivers)]
        public async Task<PagedResultDto<SelectListDto>> GetDriversSelectList(GetDriversSelectListInput input)
        {
            var query = await _driverRepository.GetQueryAsync();

            if (input.OrderLineId.HasValue)
            {
                var order = await (await _orderLineRepository.GetQueryAsync())
                    .Where(x => x.Id == input.OrderLineId)
                    .Select(x => x.Order)
                    .Select(x => new
                    {
                        x.DeliveryDate,
                        x.Shift,
                        x.OfficeId,
                    }).FirstAsync();

                query = (await _orderLineTruckRepository.GetQueryAsync())
                    .Where(x => x.OrderLine.Order.DeliveryDate == order.DeliveryDate
                        && x.OrderLine.Order.Shift == order.Shift
                        && x.OrderLine.Order.OfficeId == order.OfficeId
                        && x.DriverId != null)
                    .WhereIf(input.TruckId.HasValue, x => x.TruckId == input.TruckId)
                    .WhereIf(!string.IsNullOrEmpty(input.TruckCode), x => x.Truck.TruckCode == input.TruckCode)
                    .Select(x => x.Driver)
                    .Distinct();
            }

            var leaseHaulerIdFilter = await GetLeaseHaulerIdFilterAsync(AppPermissions.Pages_Misc_SelectLists_Drivers, AppPermissions.LeaseHaulerPortal_SelectLists_Drivers);

            return await query
                .WhereIf(leaseHaulerIdFilter.HasValue, x => x.LeaseHaulerDriver.LeaseHaulerId == leaseHaulerIdFilter)
                .WhereIf(!input.IncludeLeaseHaulerDrivers, x => !x.IsExternal)
                .WhereIf(input.OfficeId.HasValue, x => x.OfficeId == input.OfficeId)
                .WhereIf(input.LeaseHaulerId.HasValue, x => x.LeaseHaulerDriver.LeaseHaulerId == input.LeaseHaulerId)
                .WhereIf(input.OrderNotifyPreferredFormats?.Any() == true, x => input.OrderNotifyPreferredFormats.Contains(x.OrderNotifyPreferredFormat))
                .Where(x => !x.IsInactive)
                .SelectIdName()
                .GetSelectListResult(input);
        }

        [AbpAuthorize(AppPermissions.Pages_DriverMessages)]
        public async Task<PagedResultDto<SelectListDto>> GetDriversFromOrderLineSelectList(GetDriversFromOrderLineSelectListInput input)
        {
            return await (await _orderLineTruckRepository.GetQueryAsync())
                .Where(olt =>
                    olt.OrderLineId == input.OrderLineId
                    && !olt.IsDone
                    && olt.DriverId != null
                )
                .Select(olt => new SelectListDto
                {
                    Id = olt.DriverId.ToString(),
                    Name = olt.Truck.TruckCode + " - " + olt.Driver.FirstName + " " + olt.Driver.LastName,
                })
                .GetSelectListResult(input);
        }

        [AbpAuthorize(AppPermissions.Pages_Schedule, AppPermissions.LeaseHaulerPortal_Schedule)]
        public async Task<List<DriverNameDto>> GetDriverNames()
        {
            var leaseHaulerIdFilter = await GetLeaseHaulerIdFilterAsync(AppPermissions.Pages_Schedule, AppPermissions.LeaseHaulerPortal_Schedule);

            return await (await _driverRepository.GetQueryAsync())
                .Where(x => !x.IsInactive)
                .WhereIf(leaseHaulerIdFilter.HasValue, x => x.LeaseHaulerDriver.LeaseHaulerId == leaseHaulerIdFilter)
                .Select(x => new DriverNameDto
                {
                    Id = x.Id,
                    FirstName = x.FirstName,
                    LastName = x.LastName,
                }).ToListAsync();
        }

        [AbpAuthorize(AppPermissions.Pages_Misc_SelectLists_LeaseHaulers)]
        public async Task<List<DriverCompanyDto>> GetCompanyListForUserDrivers(GetCompanyListForUserDriversInput input)
        {
            return await _driverUserLinkService.GetCompanyListForUserDrivers(input);
        }

        [AbpAuthorize(AppPermissions.Pages_Drivers)]
        public async Task<DriverEditDto> GetDriverForEdit(NullableIdNameDto input)
        {
            DriverEditDto driverEditDto;

            if (input.Id.HasValue)
            {
                driverEditDto = await (await _driverRepository.GetQueryAsync())
                    .Where(x => x.Id == input.Id)
                    .Select(x => new DriverEditDto
                    {
                        Id = x.Id,
                        FirstName = x.FirstName,
                        LastName = x.LastName,
                        UserId = x.UserId,
                        IsUserActive = x.User.IsActive,
                        OfficeId = x.OfficeId,
                        OfficeName = x.Office.Name,
                        IsInactive = x.IsInactive,
                        EmailAddress = x.EmailAddress,
                        CellPhoneNumber = x.CellPhoneNumber,
                        OrderNotifyPreferredFormat = x.OrderNotifyPreferredFormat,
                        Address = x.Address,
                        City = x.City,
                        State = x.State,
                        ZipCode = x.ZipCode,
                        LicenseState = x.LicenseState,
                        LicenseNumber = x.LicenseNumber,
                        TypeOfLicense = x.TypeOfLicense,
                        LicenseExpirationDate = x.LicenseExpirationDate,
                        LastPhysicalDate = x.LastPhysicalDate,
                        NextPhysicalDueDate = x.NextPhysicalDueDate,
                        LastMvrDate = x.LastMvrDate,
                        NextMvrDueDate = x.NextMvrDueDate,
                        DateOfHire = x.DateOfHire,
                        TerminationDate = x.TerminationDate,
                        EmployeeId = x.EmployeeId,
                    })
                    .FirstAsync();
            }
            else
            {
                var (firstName, lastName) = Utilities.SplitFullName(input.Name);
                driverEditDto = new DriverEditDto
                {
                    FirstName = firstName,
                    LastName = lastName,
                };
            }

            await _singleOfficeService.FillSingleOffice(driverEditDto);

            return driverEditDto;
        }

        [AbpAuthorize(AppPermissions.Pages_Drivers)]
        public async Task<DriverEmployeeTimeClassificationsDto> GetDriverEmployeeTimeClassifications(NullableIdDto input)
        {
            var allowProductionPay = await SettingManager.GetSettingValueAsync<bool>(AppSettings.TimeAndPay.AllowProductionPay) && await FeatureChecker.IsEnabledAsync(AppFeatures.DriverProductionPayFeature);

            var result = new DriverEmployeeTimeClassificationsDto
            {
                AllTimeClassifications = await (await _timeClassificationRepository.GetQueryAsync())
                    .WhereIf(!allowProductionPay, x => !x.IsProductionBased)
                    .Select(x => new TimeClassificationDto
                    {
                        Id = x.Id,
                        Name = x.Name,
                        IsProductionBased = x.IsProductionBased,
                        DefaultRate = x.DefaultRate,
                    })
                    .OrderBy(x => x.Name)
                    .ToListAsync(),
            };

            if (input.Id.HasValue)
            {
                result.EmployeeTimeClassifications = await (await _employeeTimeClassificationRepository.GetQueryAsync())
                    .Where(x => x.DriverId == input.Id)
                    .WhereIf(!allowProductionPay, x => !x.TimeClassification.IsProductionBased)
                    .OrderByDescending(e => e.IsDefault)
                    .ThenBy(e => e.TimeClassification.Name)
                    .Select(e => new EmployeeTimeClassificationEditDto
                    {
                        Id = e.Id,
                        TimeClassificationId = e.TimeClassificationId,
                        IsDefault = e.IsDefault,
                        AllowForManualTime = e.AllowForManualTime,
                        PayRate = e.PayRate,
                    }).ToListAsync();
            }
            else
            {
                result.EmployeeTimeClassifications = await _driverUserLinkService.GetDefaultTimeClassifications();
            }

            return result;
        }

        [AbpAuthorize(AppPermissions.Pages_Drivers)]
        public async Task<EmployeeTimeClassificationEditDto> GetEmployeeTimeClassificationOrNull(GetEmployeeTimeClassificationOrNullInput input)
        {
            if (!input.EmployeeId.HasValue)
            {
                return null;
            }

            var driverId = await _driverRepository.GetDriverIdByUserIdOrDefault(input.EmployeeId.Value);
            if (driverId == 0)
            {
                return null;
            }

            var employeeTimeClassification = await (await _employeeTimeClassificationRepository.GetQueryAsync())
                    .Where(x => x.DriverId == driverId && x.TimeClassificationId == input.TimeClassificationId)
                    .Select(e => new EmployeeTimeClassificationEditDto
                    {
                        Id = e.Id,
                        TimeClassificationId = e.TimeClassificationId,
                        IsDefault = e.IsDefault,
                        AllowForManualTime = e.AllowForManualTime,
                        PayRate = e.PayRate,
                    }).FirstOrDefaultAsync();

            return employeeTimeClassification;
        }

        [AbpAuthorize(AppPermissions.Pages_Drivers, AppPermissions.Pages_LeaseHauler, AppPermissions.LeaseHaulerPortal_MyCompany_Drivers)]
        public async Task<bool> ThereAreActiveDriversWithSameEmail(ThereAreActiveDriversWithSameEmailInput input)
        {
            return await (await _driverRepository.GetQueryAsync())
                .WhereIf(input.ExceptDriverId.HasValue, x => x.Id != input.ExceptDriverId)
                .Where(x => x.EmailAddress == input.Email && !x.IsInactive)
                .AnyAsync();
        }

        [AbpAuthorize(AppPermissions.Pages_Drivers)]
        public async Task<EditDriverResult> EditDriver(DriverEditDto model)
        {
            var result = new EditDriverResult();

            if (model.OrderNotifyPreferredFormat.HasFlag(OrderNotifyPreferredFormat.Email)
                    && string.IsNullOrEmpty(model.EmailAddress)
                    || model.OrderNotifyPreferredFormat.HasFlag(OrderNotifyPreferredFormat.Sms)
                    && string.IsNullOrEmpty(model.CellPhoneNumber))
            {
                throw new UserFriendlyException("Email address and/or cell phone number are required");
            }

            if (!await FeatureChecker.IsEnabledAsync(AppFeatures.AllowMultiOfficeFeature))
            {
                if (model.OfficeId == null)
                {
                    await _singleOfficeService.FillSingleOffice(model);
                }
            }

            var driver = model.Id.HasValue ? await _driverRepository.GetAsync(model.Id.Value) : new Driver();

            driver.FirstName = model.FirstName;
            driver.LastName = model.LastName;
            driver.OfficeId = model.OfficeId;
            driver.Address = model.Address;
            driver.City = model.City;
            driver.State = model.State;
            driver.ZipCode = model.ZipCode;
            driver.LicenseState = model.LicenseState;
            driver.LicenseNumber = model.LicenseNumber;
            driver.TypeOfLicense = model.TypeOfLicense;
            driver.LicenseExpirationDate = model.LicenseExpirationDate;
            driver.LastPhysicalDate = model.LastPhysicalDate;
            driver.NextPhysicalDueDate = model.NextPhysicalDueDate;
            driver.LastMvrDate = model.LastMvrDate;
            driver.NextMvrDueDate = model.NextMvrDueDate;
            driver.DateOfHire = model.DateOfHire;
            driver.TerminationDate = model.TerminationDate;
            driver.EmployeeId = model.EmployeeId;

            driver.EmailAddress = model.EmailAddress;

            driver.CellPhoneNumber = model.CellPhoneNumber;
            driver.OrderNotifyPreferredFormat = model.OrderNotifyPreferredFormat;

            if (driver.IsInactive != model.IsInactive)
            {
                driver.IsInactive = model.IsInactive;
                if (model.IsInactive && driver.Id != 0)
                {
                    await _driverInactivatorService.InactivateDriverAsync(driver, leaseHaulerId: null);
                }
            }

            var tenantId = await AbpSession.GetTenantIdOrNullAsync();
            if (driver.Id == 0 && tenantId.HasValue)
            {
                driver.TenantId = tenantId.Value;
            }
            await _driverUserLinkService.UpdateUser(driver, inactivateUser: model.InactivateUser);

            if (model.Id.HasValue)
            {
                result.Id = model.Id.Value;
            }
            else
            {
                result.Id = await _driverRepository.InsertAndGetIdAsync(driver);
            }

            await UpdateEmployeeTimeClassifications(driver, model.EmployeeTimeClassifications);

            await _crossTenantOrderSender.SyncMaterialCompanyDriversIfNeeded(driver.Id);

            result.FirstName = driver.FirstName;
            result.LastName = driver.LastName;

            return result;
        }

        private async Task UpdateEmployeeTimeClassifications(Driver driver, List<EmployeeTimeClassificationEditDto> modelEmployeeTimeClassifications)
        {
            if (modelEmployeeTimeClassifications == null || !modelEmployeeTimeClassifications.Any(x => x.TimeClassificationId > 0 && x.IsDefault))
            {
                throw new UserFriendlyException("At least one time classification is required and needs to be marked as default");
            }

            modelEmployeeTimeClassifications.RemoveAll(x => x.TimeClassificationId == 0);

            if (modelEmployeeTimeClassifications.Count(x => x.IsDefault) > 1)
            {
                throw new UserFriendlyException("Only one time classification can be marked as default");
            }

            if (modelEmployeeTimeClassifications.Select(x => x.TimeClassificationId).Distinct().Count() != modelEmployeeTimeClassifications.Count)
            {
                throw new UserFriendlyException("You cannot select the same time classification more than once");
            }

            var existingEmployeeTimeClassifications = driver.Id > 0 ? await (await _employeeTimeClassificationRepository.GetQueryAsync())
                .Where(x => x.DriverId == driver.Id)
                .ToListAsync() : new List<EmployeeTimeClassification>();

            var syncRequest = new SyncRequest();

            foreach (var existingToDelete in existingEmployeeTimeClassifications.Where(x => !modelEmployeeTimeClassifications.Any(e => e.TimeClassificationId == x.TimeClassificationId)))
            {
                await _employeeTimeClassificationRepository.DeleteAsync(existingToDelete);
                syncRequest
                    .AddChange(EntityEnum.EmployeeTimeClassification, existingToDelete.ToChangedEntity(), ChangeType.Removed);
            }

            foreach (var classification in modelEmployeeTimeClassifications)
            {
                var existingToUpdate = existingEmployeeTimeClassifications.FirstOrDefault(x => x.TimeClassificationId == classification.TimeClassificationId);
                if (existingToUpdate != null)
                {
                    existingToUpdate.TimeClassificationId = classification.TimeClassificationId;
                    existingToUpdate.PayRate = classification.PayRate ?? 0;
                    existingToUpdate.IsDefault = classification.IsDefault;
                    existingToUpdate.AllowForManualTime = classification.AllowForManualTime;

                    syncRequest
                        .AddChange(EntityEnum.EmployeeTimeClassification, existingToUpdate.ToChangedEntity(), ChangeType.Modified);
                }
                else
                {
                    var employeeTimeClassificationEntity = new EmployeeTimeClassification
                    {
                        Driver = driver,
                        TimeClassificationId = classification.TimeClassificationId,
                        PayRate = classification.PayRate ?? 0,
                        IsDefault = classification.IsDefault,
                        AllowForManualTime = classification.AllowForManualTime,
                    };

                    await _employeeTimeClassificationRepository.InsertAsync(employeeTimeClassificationEntity);
                    syncRequest
                        .AddChange(EntityEnum.EmployeeTimeClassification, employeeTimeClassificationEntity.ToChangedEntity(), ChangeType.Modified);
                }
            }

            await CurrentUnitOfWork.SaveChangesAsync();
            await _syncRequestSender.SendSyncRequest(syncRequest
                .AddLogMessage("Updated EmployeeTimeClassification records for driver"));
        }

        [AbpAuthorize(AppPermissions.Pages_Drivers)]
        public async Task<DriverInactivationInfo> GetDriverInactivationInfo(EntityDto input)
        {
            var today = await GetToday();
            return new DriverInactivationInfo
            {
                HasOpenDispatches = await (await _dispatchRepository.GetQueryAsync())
                    .AnyAsync(x => x.DriverId == input.Id && !Dispatch.ClosedDispatchStatuses.Contains(x.Status)),
                HasDriverAssignments = await (await _driverAssignmentRepository.GetQueryAsync())
                    .AnyAsync(x => x.DriverId == input.Id && x.Date >= today),
            };
        }

        private async Task<bool> IsDriverAssociatedWithTruck(EntityDto input)
        {
            return await (await _truckRepository.GetQueryAsync()).Where(x => x.DefaultDriverId == input.Id).AnyAsync();
        }

        private async Task<bool> CanDeleteDriver(EntityDto input)
        {
            var hasTrucks = await IsDriverAssociatedWithTruck(input);
            if (hasTrucks)
            {
                return false;
            }

            var hasDriverAssignments = await (await _driverAssignmentRepository.GetQueryAsync()).Where(x => x.DriverId == input.Id).AnyAsync();
            if (hasDriverAssignments)
            {
                return false;
            }

            return true;
        }

        [AbpAuthorize(AppPermissions.Pages_Drivers, AppPermissions.LeaseHaulerPortal_MyCompany_Drivers)]
        public async Task DeleteDriver(EntityDto input)
        {
            var canDelete = await CanDeleteDriver(input);
            if (!canDelete)
            {
                throw new UserFriendlyException(L("UnableToDeleteDriverWithAssociatedData"));
            }

            var driver = await (await _driverRepository.GetQueryAsync())
                .Include(d => d.LeaseHaulerDriver)
                .FirstAsync(d => d.Id == input.Id);
            await CheckEntitySpecificPermissions(
                AppPermissions.Pages_Drivers,
                AppPermissions.LeaseHaulerPortal_MyCompany_Drivers,
                Session.LeaseHaulerId,
                driver.LeaseHaulerDriver?.LeaseHaulerId);
            await _driverUserLinkService.EnsureCanDeleteDriver(driver);
            await _employeeTimeClassificationRepository.DeleteAsync(d => d.DriverId == input.Id);
            await _driverRepository.DeleteAsync(driver);
        }
    }
}
