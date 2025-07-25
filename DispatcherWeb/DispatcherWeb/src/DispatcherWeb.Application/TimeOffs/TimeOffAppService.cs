using System.Collections.Generic;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Threading.Tasks;
using Abp.Application.Services.Dto;
using Abp.Authorization;
using Abp.Domain.Repositories;
using Abp.Linq.Extensions;
using Abp.Runtime.Session;
using Abp.Timing;
using Abp.UI;
using DispatcherWeb.Authorization;
using DispatcherWeb.Dispatching;
using DispatcherWeb.Drivers;
using DispatcherWeb.Dto;
using DispatcherWeb.Infrastructure.Extensions;
using DispatcherWeb.Orders;
using DispatcherWeb.SyncRequests;
using DispatcherWeb.TimeOffs.Dto;
using DispatcherWeb.TimeOffs.Exporting;
using DispatcherWeb.Trucks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DispatcherWeb.TimeOffs
{
    [AbpAuthorize]
    public class TimeOffAppService : DispatcherWebAppServiceBase, ITimeOffAppService
    {
        private readonly IRepository<TimeOff> _timeOffRepository;
        private readonly IRepository<Driver> _driverRepository;
        private readonly ITimeOffListCsvExporter _timeOffListCsvExporter;
        private readonly IRepository<Dispatch> _dispatchRepository;
        private readonly IRepository<OrderLineTruck> _orderLineTruckRepository;
        private readonly IRepository<DriverAssignment> _driverAssignmentRepository;
        private readonly IRepository<Truck> _truckRepository;
        private readonly IOrderLineUpdaterFactory _orderLineUpdaterFactory;
        private readonly ISyncRequestSender _syncRequestSender;

        public TimeOffAppService(
            IRepository<TimeOff> timeOffRepository,
            IRepository<Driver> driverRepository,
            ITimeOffListCsvExporter timeOffListCsvExporter,
            IRepository<Dispatch> dispatchRepository,
            IRepository<OrderLineTruck> orderLineTruckRepository,
            IRepository<DriverAssignment> driverAssignmentRepository,
            IRepository<Truck> truckRepository,
            IOrderLineUpdaterFactory orderLineUpdaterFactory,
            ISyncRequestSender syncRequestSender)
        {
            _timeOffRepository = timeOffRepository;
            _driverRepository = driverRepository;
            _timeOffListCsvExporter = timeOffListCsvExporter;
            _dispatchRepository = dispatchRepository;
            _orderLineTruckRepository = orderLineTruckRepository;
            _driverAssignmentRepository = driverAssignmentRepository;
            _truckRepository = truckRepository;
            _orderLineUpdaterFactory = orderLineUpdaterFactory;
            _syncRequestSender = syncRequestSender;
        }

        private async Task<IOrderedQueryable<TimeOffDto>> GetTimeOffRecordsQueryAsync(GetTimeOffRecordsInput input)
        {
            input.StartDateStart = input.StartDateStart;
            input.StartDateEnd = input.StartDateEnd?.AddDays(1);

            var query = (await _timeOffRepository.GetQueryAsync())
                .WhereIf(input.StartDateStart.HasValue, x => x.StartDate >= input.StartDateStart.Value)
                .WhereIf(input.StartDateEnd.HasValue, x => x.StartDate < input.StartDateEnd.Value)
                .WhereIf(input.DriverId.HasValue, x => x.DriverId == input.DriverId)
                .Select(x => new TimeOffDto
                {
                    Id = x.Id,
                    DriverName = x.Driver.LastName + ", " + x.Driver.FirstName,
                    StartDate = x.StartDate,
                    EndDate = x.EndDate,
                    RequestedHours = x.RequestedHours,
                    PaidHours = x.EmployeeTimes
                        .Where(x => x.PayStatementTime != null && (x.EndDateTime.HasValue || x.ManualHourAmount.HasValue))
                        .Sum(x => x.ManualHourAmount.HasValue ? x.ManualHourAmount : ((decimal)EF.Functions.DateDiffMinute(x.StartDateTime, x.EndDateTime.Value) / 60)),
                    Reason = x.Reason,
                })
                .OrderBy(input.Sorting);

            return query;
        }

        [AbpAuthorize(AppPermissions.Pages_TimeOff)]
        public async Task<PagedResultDto<TimeOffDto>> GetTimeOffRecords(GetTimeOffRecordsInput input)
        {
            var query = await GetTimeOffRecordsQueryAsync(input);

            var totalCount = await query.CountAsync();

            var items = await query
                .PageBy(input)
                .ToListAsync();

            return new PagedResultDto<TimeOffDto>(
                totalCount,
                items);
        }

        [AbpAuthorize(AppPermissions.Pages_TimeOff)]
        [HttpPost]
        public async Task<FileDto> GetTimeOffRecordsToCsv(GetTimeOffRecordsInput input)
        {
            var query = await GetTimeOffRecordsQueryAsync(input);
            var items = await query
                .ToListAsync();

            if (!items.Any())
            {
                throw new UserFriendlyException("There is no data to export!");
            }

            return await _timeOffListCsvExporter.ExportToFileAsync(items);
        }

        [AbpAuthorize(AppPermissions.Pages_TimeOff)]
        public async Task<TimeOffEditDto> GetTimeOffForEdit(NullableIdDto input)
        {
            TimeOffEditDto timeOffEditDto;

            if (input.Id.HasValue)
            {
                timeOffEditDto = await (await _timeOffRepository.GetQueryAsync())
                    .Where(x => x.Id == input.Id.Value)
                    .Select(x => new TimeOffEditDto
                    {
                        Id = x.Id,
                        DriverId = x.DriverId,
                        EmployeeId = x.Driver.UserId,
                        DriverName = x.Driver.LastName + ", " + x.Driver.FirstName,
                        StartDate = x.StartDate,
                        EndDate = x.EndDate,
                        RequestedHours = x.RequestedHours,
                        Reason = x.Reason,
                    }).FirstAsync();
            }
            else
            {
                timeOffEditDto = new TimeOffEditDto
                {
                };
            }

            return timeOffEditDto;
        }

        [AbpAuthorize(AppPermissions.Pages_TimeOff)]
        public async Task<EditTimeOffResult> EditTimeOff(TimeOffEditDto model)
        {
            var entity = model.Id.HasValue
                ? await (await _timeOffRepository.GetQueryAsync())
                    .Where(x => x.Id == model.Id.Value)
                    .FirstAsync()
                : new TimeOff();

            var timezone = await GetTimezone();
            if (!model.StartDate.HasValue)
            {
                throw new UserFriendlyException("StartDate is required");
            }
            if (!model.EndDate.HasValue)
            {
                throw new UserFriendlyException("EndDate is required");
            }

            var result = await RemoveDriverFromTrucks(new RemoveDriverFromTrucksInput
            {
                StartDate = model.StartDate.Value,
                EndDate = model.EndDate.Value,
                DriverId = model.DriverId,
            });

            entity.DriverId = model.DriverId;
            entity.StartDate = model.StartDate.Value;
            entity.EndDate = model.EndDate.Value;
            entity.RequestedHours = model.RequestedHours;
            entity.Reason = model.Reason;
            entity.TenantId = await Session.GetTenantIdAsync();

            await _timeOffRepository.InsertOrUpdateAndGetIdAsync(entity);

            return result;
        }

        private async Task<EditTimeOffResult> RemoveDriverFromTrucks(RemoveDriverFromTrucksInput input)
        {
            input.StartDate = input.StartDate.Date;
            input.EndDate = input.EndDate.Date;
            var today = await GetToday();
            var syncRequest = new SyncRequest();

            if (input.EndDate < input.StartDate)
            {
                throw new UserFriendlyException("End Date should be greater than Start Date");
            }

            if (input.EndDate < today)
            {
                return new EditTimeOffResult();
            }

            if (input.StartDate < today)
            {
                input.StartDate = today;
            }

            var defaultTruck = await (await _truckRepository.GetQueryAsync())
                .Where(t =>
                    t.DefaultDriverId == input.DriverId
                    && t.OfficeId != null
                    && t.VehicleCategory.IsPowered
                    && t.IsActive
                    && !t.IsOutOfService
                )
                .Select(x => new
                {
                    TruckId = x.Id,
                    x.OfficeId,
                }).FirstOrDefaultAsync();

            var defaultTruckId = defaultTruck?.TruckId;
            var existingAssignments = await (await _driverAssignmentRepository.GetQueryAsync())
                .Where(da => da.Date >= input.StartDate && da.Date <= input.EndDate && (da.DriverId == input.DriverId || da.TruckId == defaultTruckId))
                .ToListAsync();

            var result = new EditTimeOffResult();

            var dispatchesToCancel = await (await _dispatchRepository.GetQueryAsync())
                    .Where(d => d.DriverId == input.DriverId
                        && d.OrderLine.Order.DeliveryDate >= input.StartDate
                        && d.OrderLine.Order.DeliveryDate <= input.EndDate
                        && Dispatch.UnacknowledgedStatuses.Contains(d.Status))
                    .ToListAsync();
            dispatchesToCancel.ForEach(d =>
            {
                d.Status = DispatchStatus.Canceled;
                d.Canceled = Clock.Now;
                syncRequest.AddChange(EntityEnum.Dispatch, d.ToChangedEntity(), ChangeType.Removed);
            });

            var orderLineIdsNeedingStaggeredTimeRecalculation = new List<int>();

            var allShifts = await SettingManager.GetShiftsAsync();

            var date = input.StartDate;
            while (date <= input.EndDate)
            {
                foreach (var shift in allShifts)
                {
                    int? updatedTruckId = null;
                    var existingAssignment = existingAssignments.FirstOrDefault(x => x.Date == date && x.Shift == shift);
                    if (existingAssignment != null)
                    {
                        if (existingAssignment.DriverId == input.DriverId)
                        {
                            updatedTruckId = existingAssignment.TruckId;

                            existingAssignment.DriverId = null;
                            //existingAssignment.OfficeId = sharedTruckResult.GetLocationForDate(date, input.Shift);

                            syncRequest.AddChange(EntityEnum.DriverAssignment,
                                existingAssignment.ToChangedEntity().SetOldDriverIdToNotify(input.DriverId),
                                ChangeType.Modified);
                        }
                    }
                    else
                    {
                        if (defaultTruck != null)
                        {
                            updatedTruckId = defaultTruck.TruckId;
                            var newDriverAssignment = new DriverAssignment
                            {
                                OfficeId = defaultTruck.OfficeId,
                                Date = date,
                                Shift = shift,
                                TruckId = defaultTruck.TruckId,
                                DriverId = null,
                            };
                            await _driverAssignmentRepository.InsertAsync(newDriverAssignment);
                            existingAssignments.Add(newDriverAssignment);
                            syncRequest.AddChange(EntityEnum.DriverAssignment, newDriverAssignment.ToChangedEntity());
                        }
                    }

                    if (updatedTruckId != null)
                    {
                        var orderLineTrucksToDelete = await (await _orderLineTruckRepository.GetQueryAsync())
                            .Where(olt => olt.TruckId == updatedTruckId && olt.OrderLine.Order.DeliveryDate == date && olt.OrderLine.Order.Shift == shift)
                            .ToListAsync();

                        if (orderLineTrucksToDelete.Count > 0)
                        {
                            result.TruckWasRemovedFromOrders = true;
                        }

                        await _orderLineTruckRepository.DeleteRangeAsync(orderLineTrucksToDelete);

                        orderLineIdsNeedingStaggeredTimeRecalculation.AddRange(orderLineTrucksToDelete.Select(x => x.OrderLineId).Distinct());
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
                    var order = await orderLineUpdater.GetOrderAsync();
                    if (order.DeliveryDate >= await GetToday())
                    {
                        orderLineUpdater.UpdateStaggeredTimeOnTrucksOnSave();
                        await orderLineUpdater.SaveChangesAsync();
                    }
                }
            }

            await CurrentUnitOfWork.SaveChangesAsync();

            await _syncRequestSender.SendSyncRequest(syncRequest
                    .AddLogMessage($"Added time off request for the driver"));

            return result;
        }

        [AbpAuthorize(AppPermissions.Pages_TimeOff)]
        public async Task DeleteTimeOff(EntityDto input)
        {
            var timeOff = await (await _timeOffRepository.GetQueryAsync())
                .Where(x => x.Id == input.Id)
                .FirstAsync();

            await _timeOffRepository.DeleteAsync(timeOff);
        }

        [AbpAuthorize(AppPermissions.Pages_TimeOff)]
        public async Task<PagedResultDto<SelectListDto>> GetDriversSelectList(GetSelectListInput input)
        {
            var query = (await _driverRepository.GetQueryAsync())
                //.Where(x => x.User.IsActive && x.OfficeId != null)
                .Where(x => x.OfficeId != null && !x.IsInactive)
                .Select(x => new SelectListDto
                {
                    Id = x.Id.ToString(),
                    Name = x.LastName + ", " + x.FirstName,
                });

            //var query = UserManager.Users
            //    .Where(x => x.IsActive && x.Drivers.FirstOrDefault().OfficeId != null)
            //    .Select(x => new SelectListDto
            //    {
            //        Id = x.Id.ToString(),
            //        Name = x.Surname + ", " + x.Name
            //    });

            return await query.GetSelectListResult(input);
        }
    }
}
