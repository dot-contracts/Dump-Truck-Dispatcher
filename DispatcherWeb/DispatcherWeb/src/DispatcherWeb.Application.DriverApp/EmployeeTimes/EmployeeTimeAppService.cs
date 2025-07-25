using Abp.Application.Services.Dto;
using Abp.Authorization;
using Abp.Configuration;
using Abp.Domain.Entities;
using Abp.Domain.Repositories;
using Abp.Extensions;
using Abp.Linq.Extensions;
using Abp.Notifications;
using Abp.Runtime.Session;
using Abp.Timing;
using Abp.UI;
using DispatcherWeb.Authorization;
using DispatcherWeb.Configuration;
using DispatcherWeb.DriverApp.EmployeeTimes.Dto;
using DispatcherWeb.Drivers;
using DispatcherWeb.Features;
using DispatcherWeb.Infrastructure;
using DispatcherWeb.Notifications;
using DispatcherWeb.Orders;
using DispatcherWeb.TimeClassifications;
using Microsoft.EntityFrameworkCore;

namespace DispatcherWeb.DriverApp.EmployeeTimes
{
    [AbpAuthorize(AppPermissions.Pages_DriverApplication_ReactNativeDriverApp)]
    public class EmployeeTimeAppService : DispatcherWebDriverAppAppServiceBase, IEmployeeTimeAppService
    {
        private readonly IAppNotifier _appNotifier;
        private readonly IRepository<Drivers.EmployeeTime> _employeeTimeRepository;
        private readonly IRepository<TimeClassification> _timeClassificationRepository;
        private readonly IRepository<OrderLine> _orderLineRepository;
        private readonly IRepository<Driver> _driverRepository;

        public EmployeeTimeAppService(
            IAppNotifier appNotifier,
            IRepository<Drivers.EmployeeTime> employeeTimeRepository,
            IRepository<TimeClassification> timeClassificationRepository,
            IRepository<OrderLine> orderLineRepository,
            IRepository<Driver> driverRepository
            )
        {
            _appNotifier = appNotifier;
            _employeeTimeRepository = employeeTimeRepository;
            _timeClassificationRepository = timeClassificationRepository;
            _orderLineRepository = orderLineRepository;
            _driverRepository = driverRepository;
        }

        public async Task<IPagedResult<EmployeeTimeDto>> Get(GetInput input)
        {
            var query = (await _employeeTimeRepository.GetQueryAsync())
                .Where(x => x.UserId == Session.UserId)
                .Where(x => x.ManualHourAmount == null && !x.IsImported) //these records are not handled on the RN Driver App side yet and it's faster to fix this on the backend for now.
                .WhereIf(input.Id.HasValue, x => x.Id == input.Id)
                .WhereIf(input.TruckId.HasValue, x => x.EquipmentId == input.TruckId)
                .WhereIf(input.StartDateTimeBegin.HasValue, x => x.StartDateTime >= input.StartDateTimeBegin)
                .WhereIf(input.StartDateTimeEnd.HasValue, x => x.StartDateTime <= input.StartDateTimeEnd)
                .WhereIf(input.EndDateTimeBegin.HasValue, x => x.EndDateTime >= input.EndDateTimeBegin)
                .WhereIf(input.EndDateTimeEnd.HasValue, x => x.EndDateTime <= input.EndDateTimeEnd)
                .WhereIf(input.HasEndTime == true, x => x.EndDateTime != null)
                .WhereIf(input.HasEndTime == false, x => x.EndDateTime == null)
                .WhereIf(input.IsImported.HasValue, x => x.IsImported == input.IsImported)
                .WhereIf(input.ModifiedAfterDateTime.HasValue, d => d.CreationTime > input.ModifiedAfterDateTime.Value || (d.LastModificationTime != null && d.LastModificationTime > input.ModifiedAfterDateTime.Value))
                .Select(x => new EmployeeTimeDto
                {
                    Id = x.Id,
                    StartDateTime = x.StartDateTime,
                    EndDateTime = x.EndDateTime,
                    Description = x.Description,
                    TruckId = x.EquipmentId,
                    Latitude = x.Latitude,
                    Longitude = x.Longitude,
                    OrderLineId = x.OrderLineId,
                    TimeClassificationId = x.TimeClassificationId,
                    LastModifiedDateTime = x.LastModificationTime.HasValue && x.LastModificationTime.Value > x.CreationTime ? x.LastModificationTime.Value : x.CreationTime,
                    IsEditable = x.PayStatementTime == null,
                    IsImported = x.IsImported,
                });

            var totalCount = await query.CountAsync(CancellationTokenProvider.Token);
            var items = await query
                .PageBy(input)
                .ToListAsync(CancellationTokenProvider.Token);

            return new PagedResultDto<EmployeeTimeDto>(
                totalCount,
                items);
        }

        private async Task EnsureIsEditable(int id)
        {
            if (id == 0)
            {
                return;
            }
            if (await (await _employeeTimeRepository.GetQueryAsync()).AnyAsync(x => x.Id == id && x.PayStatementTime != null,
                    CancellationTokenProvider.Token))
            {
                throw new UserFriendlyException("This EmployeeTime was already added to a pay statement and cannot be edited");
            }
        }

        public async Task<EmployeeTimeDto> Post(EmployeeTimeEditDto model)
        {
            var employeeTime = model.Id == 0 ? new Drivers.EmployeeTime() : await _employeeTimeRepository.FirstOrDefaultAsync(model.Id);
            if (employeeTime == null)
            {
                var deletedEmployeeTime = await _employeeTimeRepository.GetDeletedEntity(new EntityDto(model.Id), CurrentUnitOfWork);
                if (deletedEmployeeTime == null)
                {
                    throw new UserFriendlyException($"EmployeeTime with id {model.Id} wasn't found");
                }
                await SendDeletedRnEntityNotificationIfNeededAsync(deletedEmployeeTime, model);
                deletedEmployeeTime.UnDelete();
                employeeTime = deletedEmployeeTime;
                await CurrentUnitOfWork.SaveChangesAsync();
            }

            await EnsureIsEditable(model.Id);

            var driver = await (await _driverRepository.GetQueryAsync())
                .Where(x => x.UserId == Session.UserId)
                .Select(x => new
                {
                    x.Id,
                    x.IsInactive,
                    x.IsExternal,
                })
                .OrderByDescending(x => !x.IsInactive)
                .FirstOrDefaultAsync(CancellationTokenProvider.Token);

            //should we limit them to editing only their own records?
            //if (model.Id != 0 && employeeTime.UserId != Session.UserId || model.UserId != Session.UserId)
            //{
            //    throw new UserFriendlyException("You can only edit your own time records");
            //}

            if (model.StartDateTime < Clock.Now.AddYears(-1)
                || model.EndDateTime < Clock.Now.AddYears(-1))
            {
                throw new UserFriendlyException(L("DateShouldBeAfterOneYearAgo"));
            }
            if (model.StartDateTime > Clock.Now.AddMonths(1)
                || model.EndDateTime > Clock.Now.AddMonths(1))
            {
                throw new UserFriendlyException(L("DateShouldBeBeforeOneMonthFromNow"));
            }

            var basePayOnHourlyJobRate = await SettingManager.GetSettingValueAsync<bool>(AppSettings.TimeAndPay.BasePayOnHourlyJobRate);
            var useDriverSpecificHourlyJobRate = await SettingManager.GetSettingValueAsync<bool>(AppSettings.TimeAndPay.UseDriverSpecificHourlyJobRate);
            var allowSubcontractorsToDriveCompanyOwnedTrucks = await SettingManager.GetSettingValueAsync<bool>(AppSettings.LeaseHaulers.AllowSubcontractorsToDriveCompanyOwnedTrucks);

            if (driver != null && driver.IsExternal && !allowSubcontractorsToDriveCompanyOwnedTrucks)
            {
                //throw new UserFriendlyException("Lease Hauler drivers are not allowed to create time records");
                Logger.Warn($"Lease Hauler driver (driverId: {driver.Id}, userId: {Session.UserId}) created or updated an Employee Time record using RN API");
            }

            var driverId = driver?.Id;
            var orderLine = model.OrderLineId == null
                ? null
                : await (await _orderLineRepository.GetQueryAsync())
                    .Where(x => x.Id == model.OrderLineId)
                    .Select(x => new
                    {
                        x.Id,
                        x.HourlyDriverPayRate,
                        x.DriverPayTimeClassificationId,
                        EmployeeTimeClassificaiton = x.DriverPayTimeClassification.EmployeeTimeClassifications
                            .Where(e => driverId != null ? e.DriverId == driverId : e.Driver.UserId == Session.UserId)
                            .Select(e => new
                            {
                                e.PayRate,
                            }).FirstOrDefault(),
                    }).FirstOrDefaultAsync(CancellationTokenProvider.Token);

            var timeClassificationId = (basePayOnHourlyJobRate ? orderLine?.DriverPayTimeClassificationId : null)
                ?? await GetValidatedTimeClassificationIdOrNullAsync(model.TimeClassificationId)
                ?? await SettingManager.GetSettingValueForTenantAsync<int>(AppSettings.TimeAndPay.TimeTrackingDefaultTimeClassificationId, await Session.GetTenantIdAsync());

            var timeClassification = await (await _timeClassificationRepository.GetQueryAsync())
                .Where(x => x.Id == timeClassificationId)
                .Select(x => new
                {
                    EmployeeTimeClassificaiton = x.EmployeeTimeClassifications
                        .Where(e => driverId != null ? e.DriverId == driverId : e.Driver.UserId == Session.UserId)
                        .Select(e => new
                        {
                            e.PayRate,
                        }).FirstOrDefault(),
                }).FirstOrDefaultAsync(CancellationTokenProvider.Token);

            var hourlyPayRate = !basePayOnHourlyJobRate
                ? null
                : orderLine == null
                ? timeClassification?.EmployeeTimeClassificaiton?.PayRate
                : useDriverSpecificHourlyJobRate
                    ? orderLine.EmployeeTimeClassificaiton?.PayRate
                    : orderLine.HourlyDriverPayRate;

            employeeTime.StartDateTime = model.StartDateTime;
            employeeTime.EndDateTime = model.EndDateTime;
            employeeTime.UserId = Session.GetUserId();
            employeeTime.DriverId = driver?.Id;
            employeeTime.Description = model.Description?.TruncateWithPostfix(EntityStringFieldLengths.EmployeeTime.Description);
            employeeTime.EquipmentId = model.TruckId;
            employeeTime.Latitude = model.Latitude;
            employeeTime.Longitude = model.Longitude;
            employeeTime.TimeClassificationId = timeClassificationId;
            employeeTime.PayRate = hourlyPayRate;
            employeeTime.OrderLineId = model.OrderLineId;

            if (model.Id == 0)
            {
                await _employeeTimeRepository.InsertAsync(employeeTime);
            }

            await CurrentUnitOfWork.SaveChangesAsync();
            model.Id = employeeTime.Id;

            return (await Get(new GetInput { Id = model.Id })).Items.FirstOrDefault();
        }

        private async Task<int?> GetValidatedTimeClassificationIdOrNullAsync(int? id)
        {
            if (id == null)
            {
                return null;
            }
            var timeClassification = await (await _timeClassificationRepository.GetQueryAsync())
                .Where(x => x.Id == id)
                .Select(x => new
                {
                    x.Id,
                }).FirstOrDefaultAsync(CancellationTokenProvider.Token);

            return timeClassification?.Id;
        }

        private async Task<string> GetMeaningfulEmployeeTimeDiffAsync(Drivers.EmployeeTime deletedEmployeeTime, EmployeeTimeEditDto model)
        {
            var result = "";
            var timezone = await GetTimezone();
            if (deletedEmployeeTime.StartDateTime != model.StartDateTime)
            {
                result += $"Start Date/Time: {deletedEmployeeTime.StartDateTime.ConvertTimeZoneTo(timezone):g} ➔ {model.StartDateTime.ConvertTimeZoneTo(timezone):g}; ";
            }
            else
            {
                result += $"Start Date/Time: {deletedEmployeeTime.StartDateTime.ConvertTimeZoneTo(timezone):g}; ";
            }

            if (deletedEmployeeTime.EndDateTime != model.EndDateTime)
            {
                result += $"End Date/Time: {deletedEmployeeTime.EndDateTime?.ConvertTimeZoneTo(timezone):g} ➔ {model.EndDateTime?.ConvertTimeZoneTo(timezone):g}; ";
            }

            if (deletedEmployeeTime.Description != model.Description)
            {
                result += $"Description: {deletedEmployeeTime.Description} ➔ {model.Description}; ";
            }

            if (deletedEmployeeTime.EquipmentId != model.TruckId)
            {
                result += $"Truck: {deletedEmployeeTime.EquipmentId} ➔ {model.TruckId}; ";
            }

            if (deletedEmployeeTime.TimeClassificationId != model.TimeClassificationId)
            {
                var timeClassifications = await (await _timeClassificationRepository.GetQueryAsync())
                    .Where(x => x.Id == deletedEmployeeTime.TimeClassificationId || x.Id == model.TimeClassificationId)
                    .Select(x => new
                    {
                        x.Id,
                        x.Name,
                    }).ToListAsync();
                var deletedTimeClassification = timeClassifications.FirstOrDefault(x => x.Id == deletedEmployeeTime.Id)?.Name ?? "-";
                var modelTimeClassification = timeClassifications.FirstOrDefault(x => x.Id == model.TimeClassificationId)?.Name ?? "-";

                if (deletedTimeClassification != modelTimeClassification)
                {
                    result += $"Time Classification: {deletedTimeClassification} ➔ {modelTimeClassification}; ";
                }
            }

            return result;
        }

        private async Task SendDeletedRnEntityNotificationIfNeededAsync(Drivers.EmployeeTime deletedEmployeeTime, EmployeeTimeEditDto model)
        {
            if (!await FeatureChecker.IsEnabledAsync(AppFeatures.SendRnConflictsToUsers))
            {
                return;
            }

            var employeeTimeDiff = await GetMeaningfulEmployeeTimeDiffAsync(deletedEmployeeTime, model);

            await _appNotifier.SendNotificationAsync(
                new SendNotificationInput(
                    AppNotificationNames.SimpleMessage,
                    $"An employee time record that has been deleted in the main app was uploaded from the native driver app. Driver: {await GetCurrentUserFullName()}; {employeeTimeDiff}",
                    NotificationSeverity.Warn
                )
                {
                    IncludeLocalUsers = true,
                    PermissionFilter = AppPermissions.ReceiveRnConflicts,
                });
        }

        public async Task Delete(int id)
        {
            await EnsureIsEditable(id);
            await _employeeTimeRepository.DeleteAsync(id);
        }
    }
}
