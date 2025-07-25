using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Abp.Authorization;
using Abp.Configuration;
using Abp.Extensions;
using Abp.Linq.Extensions;
using Abp.Runtime.Session;
using Abp.Timing;
using Abp.UI;
using DispatcherWeb.Authorization;
using DispatcherWeb.BackgroundJobs;
using DispatcherWeb.Configuration;
using DispatcherWeb.Dispatching.Dto;
using DispatcherWeb.Features;
using DispatcherWeb.Orders;
using DispatcherWeb.Orders.Dto;
using DispatcherWeb.SyncRequests;
using Microsoft.EntityFrameworkCore;

namespace DispatcherWeb.Dispatching
{
    [AbpAuthorize]
    public partial class DispatchingAppService
    {
        [AbpAuthorize(AppPermissions.Pages_Dispatches)]
        public async Task<List<TruckDispatchListItemDto>> TruckDispatchList(TruckDispatchListInput input)
        {
            var dispatchQuery = (await _dispatchRepository.GetQueryAsync())
                .Where(d => d.Status != DispatchStatus.Canceled && d.Status != DispatchStatus.Completed)
                .WhereIf(input.HasLeaseHaulerId == true, d => d.Truck.LeaseHaulerTruck != null)
                .WhereIf(input.HasLeaseHaulerId == false, d => d.Truck.LeaseHaulerTruck == null)
                .WhereIf(input.OfficeId.HasValue, d => d.OrderLine.Order.OfficeId == input.OfficeId.Value)
                .WhereIf(input.DispatchIds?.Any() == true, d => input.DispatchIds.Contains(d.Id))
                .WhereIf(input.DriverIds?.Any() == true, d => input.DriverIds.Contains(d.DriverId))
                .WhereIf(input.Date.HasValue, d => d.OrderLine.Order.DeliveryDate == input.Date)
                .WhereIf(input.TruckId.HasValue, d => d.TruckId == input.TruckId)
                .WhereIf(input.DriverId.HasValue, d => d.DriverId == input.DriverId)
                .WhereIf(input.TruckIds?.Any() == true, d => input.TruckIds.Contains(d.TruckId));

            var truckDispatchPlainList = await dispatchQuery
                .Select(d => new
                {
                    SortOrder = d.SortOrder,
                    OfficeId = d.OrderLine.Order.OfficeId,
                    TruckId = d.TruckId,
                    TruckCode = d.Truck.TruckCode,
                    TrailerTruckCode = d.OrderLineTruck.Trailer.TruckCode,
                    DriverId = d.DriverId,
                    LastName = d.Driver.LastName,
                    FirstName = d.Driver.FirstName,
                    UserId = d.UserId,

                    Id = d.Id,
                    Guid = d.Guid,
                    Status = d.Status,
                    DeliveryDate = d.OrderLine.Order.DeliveryDate,
                    Shift = d.OrderLine.Order.Shift,
                    TimeOnJobUtc = d.TimeOnJob,
                    CustomerName = d.OrderLine.Order.Customer.Name,
                    LoadAtName = d.OrderLine.LoadAt.DisplayName,
                    DeliverToName = d.OrderLine.DeliverTo.DisplayName,
                    Designation = d.OrderLine.Designation,
                    Item = d.OrderLine.FreightItem.Name,
                    MaterialItemName = d.OrderLine.MaterialItem.Name,
                    MaterialUom = d.OrderLine.MaterialUom.Name,
                    FreightUom = d.OrderLine.FreightUom.Name,
                    FreightQuantity = d.FreightQuantity,
                    MaterialQuantity = d.MaterialQuantity,
                    Created = d.CreationTime,
                    Acknowledged = d.Acknowledged,
                    Sent = d.Sent,
                    Loaded = d.Loads.OrderByDescending(l => l.Id).Select(l => l.SourceDateTime).FirstOrDefault(),
                    Complete = d.Loads.OrderByDescending(l => l.Id).Select(l => l.DestinationDateTime).FirstOrDefault(),
                    d.IsMultipleLoads,
                    d.WasMultipleLoads,
                    HasTickets = d.Loads.Any(l => l.Tickets.Any()),
                    Note = d.Note,
                })
                .ToListAsync();

            var timeZone = await GetTimezone();

            Debug.Assert(AbpSession.UserId != null, "AbpSession.UserId != null");
            var truckDispatchList = (
                from d in truckDispatchPlainList
                group d by new { d.TruckId, d.TruckCode, d.DriverId, d.LastName, d.FirstName } into truckDriverGroup
                orderby truckDriverGroup.Key.TruckCode, truckDriverGroup.Key.LastName, truckDriverGroup.Key.FirstName
                select new TruckDispatchListItemDto
                {
                    OfficeIds = truckDriverGroup.Select(x => x.OfficeId).Distinct().ToArray(),
                    TruckId = truckDriverGroup.Key.TruckId,
                    TruckCode = truckDriverGroup.Key.TruckCode,
                    DriverId = truckDriverGroup.Key.DriverId,
                    LastName = truckDriverGroup.Key.LastName,
                    FirstName = truckDriverGroup.Key.FirstName,
                    Dispatches = truckDriverGroup
                        .OrderByDescending(d => d.Status == DispatchStatus.Loaded)
                        .ThenByDescending(d => d.Status == DispatchStatus.Acknowledged)
                        .ThenBy(d => d.SortOrder)
                        .Select(d => new TruckDispatchListItemDto.TruckDispatch
                        {
                            Id = d.Id,
                            SortOrder = d.SortOrder,
                            Guid = d.Guid,
                            Status = d.Status,
                            DeliveryDate = d.DeliveryDate,
                            Shift = d.Shift,
                            TimeOnJob = d.TimeOnJobUtc?.ConvertTimeZoneTo(timeZone),
                            TrailerTruckCode = d.TrailerTruckCode,
                            CustomerName = d.CustomerName,
                            LoadAtName = d.LoadAtName,
                            DeliverToName = d.DeliverToName,
                            Item = d.Item,
                            ItemDisplayName = OrderItemFormatter.GetItemWithQuantityFormatted(new OrderLineItemWithQuantityDto
                            {
                                FreightItemName = d.Item,
                                MaterialItemName = d.MaterialItemName,
                                FreightUomName = d.FreightUom,
                                MaterialUomName = d.MaterialUom,
                                FreightQuantity = d.FreightQuantity,
                                MaterialQuantity = d.MaterialQuantity,
                            }),
                            MaterialUom = d.MaterialUom,
                            FreightUom = d.FreightUom,
                            Created = d.Created.ConvertTimeZoneTo(timeZone),
                            Acknowledged = d.Acknowledged?.ConvertTimeZoneTo(timeZone),
                            Sent = d.Sent?.ConvertTimeZoneTo(timeZone),
                            Loaded = d.Loaded?.ConvertTimeZoneTo(timeZone),
                            Complete = d.Complete?.ConvertTimeZoneTo(timeZone),
                            IsMultipleLoads = d.IsMultipleLoads,
                            WasMultipleLoads = d.WasMultipleLoads,
                            HasTickets = d.HasTickets,
                            Note = d.Note,
                        }).ToList(),
                })
                .ToList();

            var nowTimeInUserTimeZone = Clock.Now.ConvertTimeZoneTo(timeZone);
            var today = nowTimeInUserTimeZone.Date;
            var todayOfUserInUtcTimezone = today.ConvertTimeZoneFrom(timeZone);

            if (input.View.IsIn(DispatchListViewEnum.DriversNotClockedIn,
                DispatchListViewEnum.TrucksWithDriversAndNoDispatches,
                DispatchListViewEnum.AllTrucks))
            {
                if (input.DispatchIds?.Any() != true)
                {
                    var driverAssignments = await (await _driverAssignmentRepository.GetQueryAsync())
                        .WhereIf(input.OfficeId.HasValue, x => x.OfficeId == input.OfficeId)
                        .WhereIf(input.DriverIds?.Any() == true, x => x.DriverId != null && input.DriverIds.Contains(x.DriverId.Value))
                        .WhereIf(input.TruckIds?.Any() == true, x => input.TruckIds.Contains(x.TruckId))
                        .WhereIf(input.HasLeaseHaulerId == true, d => d.Truck.LeaseHaulerTruck != null)
                        .WhereIf(input.HasLeaseHaulerId == false, d => d.Truck.LeaseHaulerTruck == null)
                        .Where(x => x.Date == today)
                        .Select(x => new
                        {
                            OfficeId = x.OfficeId,
                            TruckId = x.TruckId,
                            TruckCode = x.Truck.TruckCode,
                            DriverId = x.DriverId,
                            LastName = x.Driver.LastName,
                            FirstName = x.Driver.FirstName,
                            UserId = x.Driver.UserId,
                        }).ToListAsync();

                    foreach (var driverAssignment in driverAssignments.Where(driverAssignment =>
                                 !truckDispatchList.Any(x => x.TruckId == driverAssignment.TruckId && x.DriverId == driverAssignment.DriverId)))
                    {
                        truckDispatchList.Add(new TruckDispatchListItemDto
                        {
                            OfficeIds = driverAssignment.OfficeId.HasValue ? new[] { driverAssignment.OfficeId.Value } : null,
                            TruckId = driverAssignment.TruckId,
                            TruckCode = driverAssignment.TruckCode,
                            DriverId = driverAssignment.DriverId,
                            LastName = driverAssignment.LastName,
                            FirstName = driverAssignment.FirstName,
                        });
                    }
                }

                if (input.View == DispatchListViewEnum.AllTrucks && input.DispatchIds?.Any() != true)
                {
                    var leaseHaulerFeatureEnabled = await FeatureChecker.IsEnabledAsync(AppFeatures.AllowLeaseHaulersFeature);

                    var truckQuery = (await _truckRepository.GetQueryAsync())
                        .Where(x => x.IsActive && !x.IsOutOfService && x.VehicleCategory.IsPowered)
                        .WhereIf(input.TruckIds?.Any() == true, x => input.TruckIds.Contains(x.Id));

                    var trucks = await truckQuery
                        .Where(x => x.OfficeId != null)
                        .WhereIf(!leaseHaulerFeatureEnabled, x => x.OfficeId != null && x.LeaseHaulerTruck.AlwaysShowOnSchedule != true)
                        .WhereIf(input.OfficeId.HasValue, x => x.OfficeId == input.OfficeId)
                        .Select(x => new
                        {
                            OfficeId = x.OfficeId,
                            TruckId = x.Id,
                            TruckCode = x.TruckCode,
                        }).ToListAsync();

                    var leaseHaulerTrucks = await truckQuery
                        .Where(x => x.OfficeId == null)
                        .SelectMany(x => x.AvailableLeaseHaulerTrucks)
                        .WhereIf(input.OfficeId.HasValue, a => a.OfficeId == input.OfficeId)
                        .Where(a => a.Date == today)
                        .Select(x => new
                        {
                            x.Id,
                            x.TruckId,
                            x.Truck.TruckCode,
                            x.DriverId,
                            x.Driver.FirstName,
                            x.Driver.LastName,
                            x.OfficeId,
                        })
                        .OrderByDescending(x => x.Id)
                        .ToListAsync();

                    foreach (var truck in leaseHaulerTrucks.Where(truck => truckDispatchList.All(x => x.TruckId != truck.TruckId)))
                    {
                        truckDispatchList.Add(new TruckDispatchListItemDto
                        {
                            OfficeIds = new[] { truck.OfficeId },
                            TruckId = truck.TruckId,
                            TruckCode = truck.TruckCode,
                            DriverId = truck.DriverId,
                            FirstName = truck.FirstName,
                            LastName = truck.LastName,
                            IsExternal = true,
                        });
                    }

                    foreach (var truck in trucks.Where(truck => truckDispatchList.All(x => x.TruckId != truck.TruckId)))
                    {
                        truckDispatchList.Add(new TruckDispatchListItemDto
                        {
                            OfficeIds = truck.OfficeId.HasValue ? new[] { truck.OfficeId.Value } : null,
                            TruckId = truck.TruckId,
                            TruckCode = truck.TruckCode,
                        });
                    }
                }
            }

            if (truckDispatchList.Any())
            {
                var dateRange = truckDispatchPlainList.Select(x => x.DeliveryDate)
                    .Union(new[] { today }).Distinct().ToList();
                var driverIdRange = truckDispatchList.Select(x => x.DriverId).Distinct().ToList();
                //var shiftRange = truckDispatchPlainList.Select(x => x.Shift).Distinct().ToList();
                var driverStartTimes = await (await _driverAssignmentRepository.GetQueryAsync())
                    .Where(x => x.DriverId.HasValue && driverIdRange.Contains(x.DriverId.Value) && dateRange.Contains(x.Date))
                    //.WhereIf(shiftRange.Any(), x => shiftRange.Contains(x.Shift))
                    .OrderBy(x => x.StartTime == null)
                    .ThenBy(x => x.StartTime)
                    .Select(x => new
                    {
                        x.DriverId,
                        x.TruckId,
                        x.Date,
                        x.Shift,
                        StartTimeUtc = x.StartTime,
                    }).ToListAsync();

                var userIdList = await (await _driverRepository.GetQueryAsync())
                    .Where(x => driverIdRange.Contains(x.Id))
                    .Select(x => new { x.UserId, DriverId = x.Id })
                    .ToListAsync();
                var userIdRange = userIdList.Select(x => x.UserId).ToList();

                var earliestDate = dateRange.Min().ConvertTimeZoneFrom(timeZone);
                var latestDate = dateRange.Max().AddDays(1).ConvertTimeZoneFrom(timeZone);
                var driverClockIns = await (await _employeeTimeRepository.GetQueryAsync())
                    .Where(x => userIdRange.Contains(x.UserId) && x.StartDateTime >= earliestDate && x.StartDateTime < latestDate)
                    .Select(x => new
                    {
                        x.StartDateTime,
                        x.EndDateTime,
                        x.IsImported,
                        x.UserId,
                    }).ToListAsync();

                var officeSettings = await (await _officeRepository.GetQueryAsync())
                    .WhereIf(input.OfficeId.HasValue, x => x.Id == input.OfficeId)
                    .Select(x => new
                    {
                        x.Id,
                        DefaultStartTimeUtc = x.DefaultStartTime,
                    }).ToListAsync();

                var tenantDefaultStartTime = (await SettingManager.GetSettingValueAsync<DateTime>(AppSettings.DispatchingAndMessaging.DefaultStartTime)).ConvertTimeZoneTo(timeZone).TimeOfDay;

                foreach (var truck in truckDispatchList)
                {
                    foreach (var dispatch in truck.Dispatches)
                    {
                        if (truck.IsExternal)
                        {
                            if (dispatch.TimeOnJob.HasValue)
                            {
                                dispatch.StartTime = dispatch.DeliveryDate.Add(dispatch.TimeOnJob.Value.TimeOfDay);
                            }
                        }
                        else
                        {
                            var startTimeRecord = driverStartTimes
                                .FirstOrDefault(x => x.Date == dispatch.DeliveryDate
                                                    && x.Shift == dispatch.Shift
                                                    && x.DriverId == truck.DriverId
                                                    && x.TruckId == truck.TruckId);
                            if (startTimeRecord?.StartTimeUtc != null)
                            {
                                var startTime = startTimeRecord.StartTimeUtc.Value.ConvertTimeZoneTo(timeZone);
                                dispatch.StartTime = startTimeRecord.Date.Date.Add(startTime.TimeOfDay);
                            }

                            if (dispatch.StartTime == null)
                            {
                                var officeDefaultStartTime = officeSettings
                                    .Where(x => truck.OfficeIds != null && truck.OfficeIds.Contains(x.Id))
                                    .Select(x => (TimeSpan?)(x.DefaultStartTimeUtc?.ConvertTimeZoneTo(timeZone).TimeOfDay ?? tenantDefaultStartTime))
                                    .OrderBy(x => x)
                                    .FirstOrDefault();

                                dispatch.StartTime = dispatch.DeliveryDate.Add(officeDefaultStartTime ?? tenantDefaultStartTime);
                            }
                        }
                    }

                    truck.StartTime = driverStartTimes.FirstOrDefault(x => x.Date == today && x.TruckId == truck.TruckId)?.StartTimeUtc?.ConvertTimeZoneTo(timeZone) ?? today.Add(tenantDefaultStartTime);
                    truck.UserId = userIdList.FirstOrDefault(x => x.DriverId == truck.DriverId)?.UserId;
                    if (truck.UserId != null)
                    {
                        truck.IsClockedIn = driverClockIns.Any(x => x.EndDateTime == null
                                                                && !x.IsImported
                                                                && x.UserId == truck.UserId
                                                                && x.StartDateTime >= todayOfUserInUtcTimezone
                                                                && x.StartDateTime < todayOfUserInUtcTimezone.AddDays(1));

                        truck.HasClockedInToday = driverClockIns.Any(x => x.UserId == truck.UserId
                                                            && x.StartDateTime >= todayOfUserInUtcTimezone
                                                            && x.StartDateTime < todayOfUserInUtcTimezone.AddDays(1));
                    }
                }
            }

            switch (input.View)
            {
                case DispatchListViewEnum.OpenDispatches:
                    truckDispatchList.RemoveAll(x => !x.Dispatches.Any());
                    break;
                case DispatchListViewEnum.DriversNotClockedIn:
                    truckDispatchList.RemoveAll(x => x.HasClockedInToday);
                    break;
                case DispatchListViewEnum.UnacknowledgedDispatches:
                    truckDispatchList.RemoveAll(x => x.Dispatches.FirstOrDefault()?.Status.IsIn(DispatchStatus.Sent, DispatchStatus.Created) != true);
                    break;
                case DispatchListViewEnum.TrucksWithDriversAndNoDispatches:
                    truckDispatchList.RemoveAll(x => x.DriverId == null || x.Dispatches.Any());
                    break;
                case DispatchListViewEnum.AllTrucks:
                    //
                    break;
            }

            return truckDispatchList
                .OrderBy(x => x.TruckCode)
                .ThenBy(x => x.FirstName)
                .ThenBy(x => x.LastName)
                .ToList();
        }

        [AbpAuthorize(AppPermissions.Pages_Dispatches)]
        public async Task<List<TruckDispatchClockInInfoDto>> GetTruckDispatchClockInInfo(GetTruckDispatchClockInInfoInput input)
        {
            var timeZone = await GetTimezone();
            var today = await GetToday();
            var todayOfUserInUtcTimezone = today.ConvertTimeZoneFrom(timeZone);

            var todayClockIns = await (await _employeeTimeRepository.GetQueryAsync())
                .Where(x => input.UserIds.Contains(x.UserId)
                    && x.StartDateTime >= todayOfUserInUtcTimezone
                    && x.StartDateTime < todayOfUserInUtcTimezone.AddDays(1))
                .Select(x => new
                {
                    x.StartDateTime,
                    x.EndDateTime,
                    x.IsImported,
                    x.UserId,
                }).ToListAsync();

            return input.UserIds.Select(userId => new TruckDispatchClockInInfoDto
            {
                UserId = userId,
                IsClockedIn =
                    todayClockIns.Any(x =>
                        x.EndDateTime == null
                        && !x.IsImported
                        && x.UserId == userId
                        && x.StartDateTime >= todayOfUserInUtcTimezone
                        && x.StartDateTime < todayOfUserInUtcTimezone.AddDays(1)),
                HasClockedInToday =
                    todayClockIns.Any(x =>
                        x.UserId == userId
                        && x.StartDateTime >= todayOfUserInUtcTimezone
                        && x.StartDateTime < todayOfUserInUtcTimezone.AddDays(1)),
            }).ToList();
        }

        [AbpAuthorize(AppPermissions.Pages_Dispatches)]
        public async Task<List<TruckDispatchDriverAssignmentInfoDto>> GetTruckDispatchDriverAssignmentInfo(GetTruckDispatchDriverAssignmentInfoInput input)
        {
            var today = await GetToday();
            var driverStartTimes = await (await _driverAssignmentRepository.GetQueryAsync())
                    .WhereIf(input.TruckIds?.Any() == true, x => input.TruckIds.Contains(x.TruckId) && x.Date == today)
                    .OrderBy(x => x.StartTime == null)
                    .ThenBy(x => x.StartTime)
                    .Select(x => new
                    {
                        x.TruckId,
                        x.DriverId,
                        //x.Shift,
                        StartTimeUtc = x.StartTime,
                    }).ToListAsync();

            var timezone = await GetTimezone();
            return driverStartTimes.GroupBy(x => x.TruckId).Select(x => new TruckDispatchDriverAssignmentInfoDto
            {
                TruckId = x.Key,
                StartTime = x.First().StartTimeUtc?.ConvertTimeZoneTo(timezone),
            }).ToList();
        }

        [AbpAuthorize(AppPermissions.Pages_Dispatches)]
        public async Task<ViewDispatchDto> ViewDispatch(int dispatchId)
        {
            var item = await (await _dispatchRepository.GetQueryAsync())
                .Where(d => d.Id == dispatchId)
                .Select(d => new ViewDispatchDto
                {
                    Id = d.Id,
                    TruckCode = d.Truck.TruckCode,
                    CustomerName = d.OrderLine.Order.Customer.Name,
                    Item = d.OrderLine.FreightItem.Name,
                    Tickets = d.Loads.OrderByDescending(l => l.Id).Select(l => l.Tickets.Select(t => new ViewDispatchTicketDto
                    {
                        MaterialQuantity = t.MaterialQuantity,
                        FreightQuantity = t.FreightQuantity,
                        Designation = d.OrderLine.Designation,
                        TicketUomId = t.FreightUomId,
                        OrderLineMaterialUomId = d.OrderLine.MaterialUomId,
                        OrderLineFreightUomId = d.OrderLine.FreightUomId,
                    }).ToList()).FirstOrDefault(),
                    MaterialQuantity = d.MaterialQuantity,
                    FreightQuantity = d.FreightQuantity,
                    MaterialUomId = d.OrderLine.MaterialUomId,
                    FreightUomId = d.OrderLine.FreightUomId,
                    TimeOnJob = d.TimeOnJob,
                    Status = d.Status,
                    Sent = d.Sent,
                    Loaded = d.Loads.OrderByDescending(l => l.Id).Select(l => l.SourceDateTime).FirstOrDefault(),
                    Delivered = d.Loads.OrderByDescending(l => l.Id).Select(l => l.DestinationDateTime).FirstOrDefault(),
                }).FirstAsync();

            var timezone = await GetTimezone();
            item.TimeOnJob = item.TimeOnJob?.ConvertTimeZoneTo(timezone);
            item.Sent = item.Sent?.ConvertTimeZoneTo(timezone);
            item.Loaded = item.Loaded?.ConvertTimeZoneTo(timezone);
            item.Delivered = item.Delivered?.ConvertTimeZoneTo(timezone);

            return item;
        }

        [AbpAuthorize(AppPermissions.Pages_Dispatches_Edit)]
        public async Task EditDispatch(EditDispatchDto input)
        {
            var dispatch = await (await _dispatchRepository.GetQueryAsync())
                .Where(d => d.Id == input.Id)
                .FirstAsync();

            var orderLine = await (await _orderLineRepository.GetQueryAsync())
                .Where(ol => ol.Id == dispatch.OrderLineId)
                .Select(ol => new
                {
                    ol.Order.DeliveryDate,
                })
                .FirstAsync();

            input.TimeOnJob = input.TimeOnJob == null ? null
                : orderLine.DeliveryDate.Date.Add(input.TimeOnJob.Value.TimeOfDay);

            dispatch.TimeOnJob = input.TimeOnJob?.ConvertTimeZoneFrom(await GetTimezone());

            dispatch.MaterialQuantity = input.MaterialQuantity;
            dispatch.FreightQuantity = input.FreightQuantity;

            await CurrentUnitOfWork.SaveChangesAsync();

            await _syncRequestSender.SendSyncRequest(new SyncRequest()
                .AddChange(EntityEnum.Dispatch, dispatch.ToChangedEntity())
                .AddLogMessage("Edited dispatch"));
        }


        [AbpAuthorize(AppPermissions.Pages_Dispatches)]
        public async Task<SetDispatchTimeOnJobDto> GetDispatchTimeOnJob(int dispatchId)
        {
            var dispatch = await (await _dispatchRepository.GetQueryAsync())
                .Where(d => d.Id == dispatchId)
                .Select(d => new SetDispatchTimeOnJobDto
                {
                    Id = d.Id,
                    TimeOnJob = d.TimeOnJob,
                })
                .FirstAsync();

            dispatch.TimeOnJob = dispatch.TimeOnJob?.ConvertTimeZoneTo(await GetTimezone());

            return dispatch;
        }

        [AbpAuthorize(AppPermissions.Pages_Dispatches)]
        public async Task<SetDispatchTimeOnJobDto> SetDispatchTimeOnJob(SetDispatchTimeOnJobDto input)
        {
            var dispatch = await (await _dispatchRepository.GetQueryAsync())
                .Where(d => d.Id == input.Id)
                .FirstAsync();

            var orderLine = await (await _orderLineRepository.GetQueryAsync())
                .Where(ol => ol.Id == dispatch.OrderLineId)
                .Select(ol => new
                {
                    ol.Order.DeliveryDate,
                })
                .FirstAsync();

            input.TimeOnJob = input.TimeOnJob == null ? null
                : orderLine.DeliveryDate.Date.Add(input.TimeOnJob.Value.TimeOfDay);

            dispatch.TimeOnJob = input.TimeOnJob?.ConvertTimeZoneFrom(await GetTimezone());

            await CurrentUnitOfWork.SaveChangesAsync();

            await _syncRequestSender.SendSyncRequest(new SyncRequest()
                .AddChange(EntityEnum.Dispatch, dispatch.ToChangedEntity())
                .AddLogMessage("Changed TimeOnJob for dispatch"));

            return input;
        }

        [AbpAuthorize(AppPermissions.Pages_Dispatches_Edit)]
        public async Task CancelDispatch(CancelDispatchDto cancelDispatch)
        {
            var dispatchEntity = await (await _dispatchRepository.GetQueryAsync())
                .FirstAsync(x => x.Id == cancelDispatch.DispatchId);
            if (dispatchEntity == null)
            {
                throw new ApplicationException($"Dispatch with Id={cancelDispatch.DispatchId} not found!");
            }

            if (dispatchEntity.Status == DispatchStatus.Completed)
            {
                throw new UserFriendlyException(L("CannotChangeDispatchStatus"));
            }

            if (!await IsAllowedToCancelDispatch(cancelDispatch.DispatchId))
            {
                throw new UserFriendlyException(L("CannotCancelDispatchesWithTickets"));
            }

            var oldActiveDispatch = await GetFirstOpenDispatch(dispatchEntity.DriverId);

            SetDispatchEntityStatusToCanceled(dispatchEntity);
            await CurrentUnitOfWork.SaveChangesAsync();

            var isFulcrumEnabled = await SettingManager.GetSettingValueAsync<bool>(AppSettings.FulcrumIntegration.IsEnabled);
            if (isFulcrumEnabled)
            {
                await _backgroundJobManager.EnqueueAsync<FulcrumDispatchDtdTicketJob, FulcrumDispatchDtdTicketJobArgs>(new FulcrumDispatchDtdTicketJobArgs()
                {
                    RequestorUser = await Session.ToUserIdentifierAsync(),
                    DispatchId = cancelDispatch.DispatchId,
                    Action = FulcrumDtdTicketAction.Delete,
                });
            }


            var newActiveDispatch = await GetFirstOpenDispatch(dispatchEntity.DriverId);
            var syncRequest = new SyncRequest()
                .AddChange(EntityEnum.Dispatch, dispatchEntity.ToChangedEntity(), ChangeType.Removed);

            if (cancelDispatch.CancelAllDispatchesForDriver)
            {
                var dispatchesToCancel = await (await _dispatchRepository.GetQueryAsync())
                    .Where(d => d.DriverId == dispatchEntity.DriverId && d.Status == DispatchStatus.Created)
                    .ToListAsync();
                dispatchesToCancel.ForEach(SetDispatchEntityStatusToCanceled);
                await CurrentUnitOfWork.SaveChangesAsync();

                syncRequest
                    .AddChanges(EntityEnum.Dispatch, dispatchesToCancel.Select(x => x.ToChangedEntity()), ChangeType.Removed)
                    .AddLogMessage("Canceled all dispatches for driver");
            }
            else
            {
                syncRequest.AddLogMessage("Canceled dispatch");
            }

            await _syncRequestSender.SendSyncRequest(syncRequest);

            if (!cancelDispatch.CancelAllDispatchesForDriver)
            {
                await _dispatchSender.SendSmsOrEmail(new SendSmsOrEmailInput
                {
                    TruckId = dispatchEntity.TruckId,
                    DriverId = dispatchEntity.DriverId,
                    UserId = dispatchEntity.UserId,
                    PhoneNumber = dispatchEntity.PhoneNumber,
                    EmailAddress = dispatchEntity.EmailAddress,
                    OrderNotifyPreferredFormat = dispatchEntity.OrderNotifyPreferredFormat,
                    ActiveDispatchWasChanged = oldActiveDispatch?.Id != newActiveDispatch?.Id,
                });
            }
        }

        [AbpAuthorize(AppPermissions.Pages_Dispatches_Edit)]
        public async Task MarkDispatchComplete(MarkDispatchCompleteInput input)
        {
            await MarkDispatchCompleteInternal(input);
        }

        [AbpAuthorize(AppPermissions.Pages_Dispatches_Edit)]
        public async Task ReorderDispatches(ReorderDispatchesInput input)
        {
            var dispatches = await (await _dispatchRepository.GetQueryAsync())
                .Where(x => input.OrderedDispatchIds.Contains(x.Id))
                .ToListAsync();

            if (dispatches.Count != input.OrderedDispatchIds.Count)
            {
                throw new ApplicationException("At least one of the dispatches weren't found to ReorderDispatches");
            }

            var sortOrders = new Queue<int>(dispatches.Select(x => x.SortOrder).OrderBy(x => x));
            foreach (var dispatchId in input.OrderedDispatchIds)
            {
                dispatches.First(d => d.Id == dispatchId).SortOrder = sortOrders.Dequeue();
            }

            await CurrentUnitOfWork.SaveChangesAsync();

            await _syncRequestSender.SendSyncRequest(new SyncRequest()
                .AddChanges(EntityEnum.Dispatch, dispatches.Select(x => x.ToChangedEntity()))
                //.SetIgnoreForCurrentUser(true)
                .AddLogMessage("Reordered dispatch(es)"));
        }

        [AbpAuthorize(AppPermissions.Pages_Dispatches_Edit)]
        public async Task EndMultipleLoadsDispatch(int dispatchId)
        {
            await EndMultipleLoadsDispatches(new[] { dispatchId });
        }
    }
}
