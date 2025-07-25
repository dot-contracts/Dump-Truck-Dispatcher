using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using Abp.Application.Services;
using Abp.Application.Services.Dto;
using Abp.BackgroundJobs;
using Abp.Collections.Extensions;
using Abp.Domain.Repositories;
using Abp.Extensions;
using Abp.Linq.Extensions;
using Abp.Notifications;
using Abp.Runtime.Session;
using Abp.Timing;
using Abp.UI;
using DispatcherWeb.Authorization.Users;
using DispatcherWeb.BackgroundJobs;
using DispatcherWeb.Configuration;
using DispatcherWeb.Dispatching.Dto;
using DispatcherWeb.Dispatching.Dto.DispatchSender;
using DispatcherWeb.Drivers;
using DispatcherWeb.Exceptions;
using DispatcherWeb.Infrastructure;
using DispatcherWeb.Infrastructure.Extensions;
using DispatcherWeb.Infrastructure.Sms.Dto;
using DispatcherWeb.Infrastructure.Templates;
using DispatcherWeb.Notifications;
using DispatcherWeb.Orders;
using DispatcherWeb.SyncRequests;
using Microsoft.EntityFrameworkCore;

namespace DispatcherWeb.Dispatching
{
    public class DispatchSender : DispatcherWebDomainServiceBase, IDispatchSender
    {
        private readonly IRepository<OrderLine> _orderLineRepository;
        private readonly IRepository<OrderLineTruck> _orderLineTruckRepository;
        private readonly IRepository<Dispatch> _dispatchRepository;
        private readonly IRepository<Driver> _driverRepository;
        private readonly IRepository<DriverAssignment> _driverAssignmentRepository;
        private readonly IRepository<Drivers.EmployeeTime> _employeeTimeRepository;
        private readonly IAppNotifier _appNotifier;
        private readonly ISyncRequestSender _syncRequestSender;
        private readonly IBackgroundJobManager _backgroundJobManager;

        //cache
        private List<DriverAssignmentDto> _driverAssignments;
        private List<OrderLineDto> _orderLines;
        private List<OrderLineTruckDto> _orderLineTrucks;
        private List<DispatchDto> _dispatches;
        private List<Dispatch> _dispatchEntities = new();
        private List<EmployeeTimeDto> _employeeTimes;
        private List<DriverAcknowledgementDto> _driverAcknowledgements = new();
        private User _cachedCurrentUser;

        //cleanup queues
        private List<(DispatchEditDto dto, Dispatch entity)> _pendingNewDispatchQueue = new();
        private Dictionary<int, List<DispatchDto>> _orderedTruckDispatches = new();
        private List<Func<Task>> _pendingCleanupActions = new();
        private List<DeferredSendSmsOrEmailInput> _deferredSmsOrEmailMessages = new();
        private List<int> _sentDisaptchIds = new();
        private SmsSenderBackgroundJobArgs _smsSenderBackgrounJobArgs = null;
        private EmailSenderBackgroundJobArgs _emailSenderBackgrounJobArgs = null;

        public DispatchSender(
            IRepository<OrderLine> orderLineRepository,
            IRepository<OrderLineTruck> orderLineTruckRepository,
            IRepository<Dispatch> dispatchRepository,
            IRepository<Driver> driverRepository,
            IRepository<DriverAssignment> driverAssignmentRepository,
            IRepository<Drivers.EmployeeTime> employeeTimeRepository,
            IAppNotifier appNotifier,
            ISyncRequestSender syncRequestSender,
            IBackgroundJobManager backgroundJobManager
            )
        {
            _orderLineRepository = orderLineRepository;
            _orderLineTruckRepository = orderLineTruckRepository;
            _dispatchRepository = dispatchRepository;
            _driverRepository = driverRepository;
            _driverAssignmentRepository = driverAssignmentRepository;
            _employeeTimeRepository = employeeTimeRepository;
            _appNotifier = appNotifier;
            _syncRequestSender = syncRequestSender;
            _backgroundJobManager = backgroundJobManager;
        }

        public async Task CacheDataForDateShift(SendOrdersToDriversInput input)
        {
            await PopulateOrderLineTruckCache(input);

            var orderLineIds = _orderLineTrucks.Select(x => x.OrderLineId).Distinct().ToArray();
            await PopulateOrderLineCache(orderLineIds);

            await PopulateDriverAssignmentCache(input);

            var driverIds = _orderLineTrucks.Select(x => x.DriverId).Distinct().ToArray();
            await PopulateDriverAcknowledgements(driverIds);

            var truckIds = GetCachedOrderLineTrucks(input).Select(x => x.TruckId).Distinct().ToList();
            await PopulateDispatchCache(
                (await _dispatchRepository.GetQueryAsync())
                    .Where(x => orderLineIds.Contains(x.OrderLineId)
                        || Dispatch.OpenStatuses.Contains(x.Status) && truckIds.Contains(x.TruckId))
            );

            await PopulateEmployeeTimeCache(input.DeliveryDate);
        }

        private List<OrderLineTruckDto> GetCachedOrderLineTrucks(SendOrdersToDriversInput input)
        {
            if (_orderLineTrucks == null)
            {
                throw new ApplicationException("Cache hasn't been populated prior to calling GetOrderLineTrucks. Make sure you call CacheDataForDateShift before using SendOrdersToDriversInternal");
            }
            var maxTimespan = TimeSpan.FromDays(1).Subtract(TimeSpan.FromSeconds(1));
            return _orderLineTrucks
                .Where(olt => olt.OrderLine != null
                            && olt.OrderLine.DeliveryDate == input.DeliveryDate
                            && olt.OrderLine.Shift == input.Shift
                            && !olt.IsDone
                            && !olt.OrderLine.IsComplete)
                .OrderBy(olt => olt.TruckTimeOnJobUtc ?? olt.OrderLine.TimeOnJobUtc ?? DateTime.MaxValue)
                .ThenBy(olt => olt.OrderLine.LineNumber)
                .ThenBy(olt => olt.OrderLineId)
                .ToList();
        }

        private async Task PopulateDriverAssignmentCache(SendOrdersToDriversInput input)
        {
            _driverAssignments = await (await _driverAssignmentRepository.GetQueryAsync())
                .Where(da =>
                    da.Date == input.DeliveryDate
                    && da.Shift == input.Shift
                    && da.DriverId != null
                    && da.Truck.OrderLineTrucksOfTruck.Any(olt =>
                        olt.OrderLine.Order.DeliveryDate == input.DeliveryDate
                        && olt.OrderLine.Order.Shift == input.Shift
                        && (!input.OfficeIds.Any() || input.OfficeIds.Contains(olt.OrderLine.Order.OfficeId))
                        && !olt.IsDone
                        && !olt.OrderLine.IsComplete
                    )
                )
                .Select(da => new DriverAssignmentDto
                {
                    DriverId = da.DriverId.Value,
                    TruckId = da.TruckId,
                    Date = da.Date,
                    Shift = da.Shift,
                    StartTimeUtc = da.StartTime,
                })
                .ToListAsync();
        }

        private async Task PopulateOrderLineCache(params int[] orderLineIds)
        {
            _orderLines ??= new List<OrderLineDto>();

            orderLineIds = orderLineIds.Except(_orderLines.Select(x => x.Id)).ToArray();

            if (orderLineIds.Any())
            {
                var orderLines = await GetOrderLineDataForDispatchMessageQuery(
                    (await _orderLineRepository.GetQueryAsync()).Where(x => orderLineIds.Contains(x.Id))
                ).ToListAsync();

                foreach (var id in orderLineIds)
                {
                    if (!orderLines.Any(x => x.Id == id))
                    {
                        throw await GetOrderLineNotFoundExceptionAsync(new EntityDto(id));
                    }
                }

                _orderLines.AddRange(orderLines);

                if (_orderLineTrucks != null)
                {
                    foreach (var orderLine in orderLines)
                    {
                        orderLine.OrderLineTrucks = _orderLineTrucks.Where(x => x.OrderLineId == orderLine.Id).ToList();
                        orderLine.OrderLineTrucks.ForEach(x => x.OrderLine = orderLine);
                    }
                }
            }
        }

        public static IQueryable<OrderLineDto> GetOrderLineDataForDispatchMessageQuery(IQueryable<OrderLine> orderLineQuery)
        {
            return orderLineQuery.Select(ol => new OrderLineDto
            {
                Id = ol.Id,
                DeliveryDate = ol.Order.DeliveryDate,
                Shift = ol.Order.Shift,
                OrderNumber = ol.OrderId,
                Customer = ol.Order.Customer.Name,
                Directions = ol.Order.Directions,
                Note = ol.Note,
                OrderLineTimeOnJobUtc = ol.TimeOnJob,
                MaterialItemId = ol.MaterialItem.Id,
                FreightItemName = ol.FreightItem.Name,
                LoadAtName = ol.LoadAt.DisplayName,
                MaterialQuantity = ol.MaterialQuantity,
                FreightQuantity = ol.FreightQuantity,
                MaterialUom = ol.MaterialUom.Name,
                FreightUom = ol.FreightUom.Name,
                MaterialUomBaseId = (UnitOfMeasureBaseEnum)ol.MaterialUom.UnitOfMeasureBaseId,
                FreightUomBaseId = (UnitOfMeasureBaseEnum)ol.FreightUom.UnitOfMeasureBaseId,
                Designation = ol.Designation,
                DeliverToName = ol.DeliverTo.DisplayName,
                ChargeTo = ol.Order.ChargeTo,
                IsMultipleLoads = ol.IsMultipleLoads,
                IsFreightPriceOverridden = ol.IsFreightPriceOverridden,
                IsMaterialPriceOverridden = ol.IsMaterialPriceOverridden,
                TimeOnJobUtc = ol.TimeOnJob,
                OfficeId = ol.Order.OfficeId,
                IsComplete = ol.IsComplete,
            });
        }

        private async Task PopulateOrderLineTruckCache(SendOrdersToDriversInput input)
        {
            await PopulateOrderLineTruckCache(
                (await _orderLineTruckRepository.GetQueryAsync())
                    .Where(olt =>
                        olt.OrderLine.Order.DeliveryDate == input.DeliveryDate
                        && olt.OrderLine.Order.Shift == input.Shift
                        && (!input.OfficeIds.Any() || input.OfficeIds.Contains(olt.OrderLine.Order.OfficeId))
                        && !olt.IsDone
                        && !olt.OrderLine.IsComplete
                    )
            );
        }

        private async Task PopulateOrderLineTruckCache(int orderLineId)
        {
            await PopulateOrderLineTruckCache(
                (await _orderLineTruckRepository.GetQueryAsync())
                    .Where(x => x.OrderLineId == orderLineId)
            );
        }

        private async Task PopulateOrderLineTruckCache(IQueryable<OrderLineTruck> orderLineTruckQuery)
        {
            _orderLineTrucks ??= new List<OrderLineTruckDto>();

            var orderLineTrucks = await GetOrderLineTrucksForDispatchMessageAsync(
                orderLineTruckQuery
            );

            foreach (var orderLineTruck in orderLineTrucks)
            {
                if (!_orderLineTrucks.Any(x => x.OrderLineTruckId == orderLineTruck.OrderLineTruckId))
                {
                    _orderLineTrucks.Add(orderLineTruck);
                }

                if (_orderLines != null)
                {
                    var orderLine = _orderLines.FirstOrDefault(x => x.Id == orderLineTruck.OrderLineId);
                    if (orderLine != null)
                    {
                        orderLine.OrderLineTrucks ??= new List<OrderLineTruckDto>();
                        if (!orderLine.OrderLineTrucks.Any(x => x.OrderLineTruckId == orderLineTruck.OrderLineTruckId))
                        {
                            orderLine.OrderLineTrucks.Add(orderLineTruck);
                        }
                        orderLineTruck.OrderLine = orderLine;
                    }
                }
            }

            if (_orderLines != null)
            {
                foreach (var orderLine in _orderLines)
                {
                    orderLine.OrderLineTrucks ??= new List<OrderLineTruckDto>();
                }
            }
        }

        private async Task<List<OrderLineTruckDto>> GetOrderLineTrucksForDispatchMessageAsync(IQueryable<OrderLineTruck> orderLineTrucks)
        {
            var query = orderLineTrucks
                //.Where(olt => !olt.IsDone)
                .WhereIf(!await FeatureChecker.AllowLeaseHaulersFeature(), olt => olt.Truck.OfficeId != null && olt.Truck.LeaseHaulerTruck.AlwaysShowOnSchedule != true);

            var orderLineTrucksWithNoDriver = query.Where(x => x.Truck.VehicleCategory.IsPowered && x.DriverId == null);
            if (await orderLineTrucksWithNoDriver.AnyAsync())
            {
                var trucksCodes = await orderLineTrucksWithNoDriver.Select(x => x.Truck.TruckCode).ToArrayAsync();
                throw new UserFriendlyException(L("YouHaveScheduledTrucks{0}WithNoDrivers", string.Join(", ", trucksCodes)));
            }

            var result = await query
                .Where(x => x.DriverId != null)
                .Select(x => new OrderLineTruckDto
                {
                    OrderLineId = x.OrderLineId,
                    OrderLineTruckId = x.Id,
                    TruckId = x.TruckId,
                    TruckCode = x.Truck.TruckCode,
                    DriverId = x.DriverId.Value,
                    DriverName = x.Driver.FirstName + " " + x.Driver.LastName,
                    UserId = x.Driver.UserId,
                    DriverPhoneNumber = x.Driver.CellPhoneNumber,
                    DriverEmailAddress = x.Driver.EmailAddress,
                    DriverPreferredNotifyFormat = x.Driver.OrderNotifyPreferredFormat,
                    TruckTimeOnJobUtc = x.TimeOnJob,
                    IsDone = x.IsDone,
                    CargoCapacity = x.Truck.CargoCapacity,
                    CargoCapacityCyds = x.Truck.CargoCapacityCyds,
                })
                .ToListAsync();

            return result;
        }

        private async Task PopulateDispatchEntityCache(IQueryable<Dispatch> dispatchQuery)
        {
            _dispatchEntities ??= new();
            var dispatches = await dispatchQuery.ToListAsync();
            foreach (var dispatch in dispatches)
            {
                _dispatchEntities.AddIfNotContains(dispatch);
            }
        }

        private async Task PopulateDispatchCache(IQueryable<Dispatch> dispatchQuery)
        {
            _dispatches ??= new List<DispatchDto>();

            var dispatches = await GetDispatchDtoQuery(
                dispatchQuery
            ).ToListAsync();

            foreach (var dispatch in dispatches)
            {
                if (!_dispatches.Any(d => d.Id == dispatch.Id))
                {
                    _dispatches.Add(dispatch);
                }
            }
        }

        private IQueryable<DispatchDto> GetDispatchDtoQuery(IQueryable<Dispatch> dispatches)
        {
            return dispatches
                .Select(x => new DispatchDto
                {
                    Id = x.Id,
                    OrderLineId = x.OrderLineId,
                    OrderLineTruckId = x.OrderLineTruckId,
                    TruckId = x.TruckId,
                    Status = x.Status,
                    SortOrder = x.SortOrder,
                    DriverId = x.DriverId,
                    TimeOnJob = x.TimeOnJob,
                    Acknowledged = x.Acknowledged,
                    Guid = x.Guid,
                    Message = x.Message,
                });
        }

        private async Task PopulateDriverAcknowledgements(params int[] driverIds)
        {
            driverIds = driverIds.Except(_driverAcknowledgements.Select(x => x.DriverId)).Distinct().ToArray();
            if (!driverIds.Any())
            {
                return;
            }
            var timeZone = await GetTimezone();
            var today = await GetToday();
            var todayInUtc = today.ConvertTimeZoneFrom(timeZone);
            var todayAcknowledges = await (await _dispatchRepository.GetQueryAsync())
                .Where(d => driverIds.Contains(d.DriverId) && d.Acknowledged >= todayInUtc)
                .Select(x => new { x.DriverId, x.TruckId })
                .ToListAsync();

            foreach (var driverId in driverIds)
            {
                _driverAcknowledgements.Add(new DriverAcknowledgementDto
                {
                    DriverId = driverId,
                    HasAcknowledgedDispatchToday = todayAcknowledges.Any(x => x.DriverId == driverId),
                });
            }
        }

        private async Task PopulateEmployeeTimeCache(DateTime date)
        {
            _employeeTimes ??= new();

            var timezone = await GetTimezone();
            var todayInUtc = (await GetToday()).ConvertTimeZoneFrom(timezone);
            var tomorrowInUtc = todayInUtc.AddDays(1);
            var employeeTimes = await (await _employeeTimeRepository.GetQueryAsync())
                .Where(x => x.StartDateTime >= todayInUtc && x.StartDateTime < tomorrowInUtc)
                .Select(x => new EmployeeTimeDto
                {
                    Id = x.Id,
                    UserId = x.UserId,
                    StartDateTime = x.StartDateTime,
                    EndDateTime = x.EndDateTime,
                    IsImported = x.IsImported,
                }).ToListAsync();

            foreach (var employeeTime in employeeTimes)
            {
                if (!_employeeTimes.Any(d => d.Id == employeeTime.Id))
                {
                    _employeeTimes.Add(employeeTime);
                }
            }
        }






        public async Task CleanUp()
        {
            await SavePendingDispatches();
            await ReorderPendingDispatches();

            await ProcessDeferredSmsOrEmailMessages();
            await RunPendingCleanUpActions();

            if (_smsSenderBackgrounJobArgs?.SmsInputs?.Any() == true)
            {
                await _backgroundJobManager.EnqueueAsync<SmsSenderBackgroundJob, SmsSenderBackgroundJobArgs>(_smsSenderBackgrounJobArgs);
            }

            if (_emailSenderBackgrounJobArgs?.EmailInputs?.Any() == true)
            {
                await _backgroundJobManager.EnqueueAsync<EmailSenderBackgroundJob, EmailSenderBackgroundJobArgs>(_emailSenderBackgrounJobArgs);
            }
        }

        private async Task SavePendingDispatches()
        {
            if (!_pendingNewDispatchQueue.Any())
            {
                return;
            }

            foreach (var newDispatch in _pendingNewDispatchQueue)
            {
                await _dispatchRepository.InsertAsync(newDispatch.entity);
                _dispatchEntities.Add(newDispatch.entity);
                //if (newDispatch.entity.Loads?.Any() == true)
                //{
                //}
            }
            await CurrentUnitOfWork.SaveChangesAsync();

            var newDispatchSortOrders = new Queue<int>(_pendingNewDispatchQueue.Select(x => x.entity.Id).OrderBy(x => x));

            foreach (var dispatch in _pendingNewDispatchQueue)
            {
                dispatch.entity.SortOrder = newDispatchSortOrders.Dequeue();
                dispatch.dto.Id = dispatch.entity.Id;
                dispatch.dto.SortOrder = dispatch.entity.SortOrder;
            }

            await CurrentUnitOfWork.SaveChangesAsync();

            //Add new DtdTicket to fulcrum, if Fulcrum is enabled and has material
            var isFulcrumEnabled = await SettingManager.IsFulcrumIntegrationEnabledAndSetup();
            if (isFulcrumEnabled)
            {
                var pendingDispatchOrderLineIds = _pendingNewDispatchQueue.Select(x => x.entity.OrderLineId).Distinct().ToList();
                var orderLines = await (await _orderLineRepository.GetQueryAsync())
                    .Where(x => pendingDispatchOrderLineIds.Contains(x.Id))
                    .Select(x => new
                    {
                        x.Id,
                        x.MaterialItemId,
                        LoadAtSendToFulcrum = (bool?)x.LoadAt.SendToFulcrum,
                    }).ToListAsync();

                foreach (var dispatch in _pendingNewDispatchQueue)
                {
                    var orderLine = orderLines.FirstOrDefault(x => x.Id == dispatch.entity.OrderLineId);
                    if (orderLine != null && orderLine.MaterialItemId.HasValue && orderLine.LoadAtSendToFulcrum == true)
                    {
                        await _backgroundJobManager.EnqueueAsync<FulcrumDispatchDtdTicketJob, FulcrumDispatchDtdTicketJobArgs>(new FulcrumDispatchDtdTicketJobArgs()
                        {
                            RequestorUser = await Session.ToUserIdentifierAsync(),
                            DispatchId = dispatch.entity.Id,
                            Action = FulcrumDtdTicketAction.Create,
                        });
                    }
                }
            }

            _pendingNewDispatchQueue.Clear();
        }

        private static bool IsOrdered(params int[] values)
        {
            if (values.Length < 2)
            {
                return true;
            }
            var last = values[0];
            foreach (var value in values.Skip(1))
            {
                if (value < last)
                {
                    return false;
                }
                last = value;
            }
            return true;
        }

        private async Task ReorderPendingDispatches()
        {
            var outOfOrderDispatchIds = new List<int>();
            foreach (var truckId in _orderedTruckDispatches.Keys)
            {
                var dispatchDtos = GetOrderedDispatchesForTruck(truckId);
                if (IsOrdered(dispatchDtos.Select(x => x.SortOrder).ToArray()))
                {
                    continue;
                }
                outOfOrderDispatchIds.AddRange(dispatchDtos.Select(x => x.Id));
            }

            if (!outOfOrderDispatchIds.Any())
            {
                return;
            }

            var dispatchEntities = await (await _dispatchRepository.GetQueryAsync())
                .Where(x => outOfOrderDispatchIds.Contains(x.Id))
                .ToListAsync();
            dispatchEntities.ForEach(x => _dispatchEntities.AddIfNotContains(x));

            foreach (var truckId in _orderedTruckDispatches.Keys)
            {
                var truckDispatchDtos = GetOrderedDispatchesForTruck(truckId).Where(x => dispatchEntities.Any(d => d.Id == x.Id)).ToList();
                var truckDispatchEntities = dispatchEntities.Where(x => truckDispatchDtos.Any(d => d.Id == x.Id)).ToList();
                var sortOrders = new Queue<int>(truckDispatchEntities.Select(x => x.SortOrder).OrderBy(x => x));
                foreach (var truckDispatchDto in truckDispatchDtos)
                {
                    var newSortOrder = sortOrders.Dequeue();
                    truckDispatchDto.SortOrder = newSortOrder;
                    truckDispatchEntities.First(x => x.Id == truckDispatchDto.Id).SortOrder = newSortOrder;
                }
            }

            await CurrentUnitOfWork.SaveChangesAsync();
        }

        private async Task ProcessDeferredSmsOrEmailMessages()
        {
            if (!_deferredSmsOrEmailMessages.Any())
            {
                return;
            }
            await CurrentUnitOfWork.SaveChangesAsync();
            foreach (var deferredSmsOrEmailInput in _deferredSmsOrEmailMessages)
            {
                deferredSmsOrEmailInput.DispatchId = deferredSmsOrEmailInput.Dispatch.Id;
                deferredSmsOrEmailInput.NewActiveDispatch = GetFirstOpenDispatch(deferredSmsOrEmailInput.DriverId);
                //deferredSmsInput.ActiveDispatchWasChanged = deferredSmsInput.OldActiveDispatch?.Id != deferredSmsInput.NewActiveDispatch?.Id;
                //either use this (below) OR the background job that would call BatchSendSms
                await SendSmsOrEmailInternal(deferredSmsOrEmailInput);
            }
            //await _backgroundJobManager.EnqueueAsync<BatchDispatchSmsSenderBackgroundJob, BatchDispatchSmsSenderBackgroundJobArgs>(new BatchDispatchSmsSenderBackgroundJobArgs
            //{
            //    RequestorUser = Session.ToUserIdentifier(),
            //    SendSmsInputs = _deferredSmsMessages.ToArray()
            //});
        }

        private async Task RunPendingCleanUpActions()
        {
            if (!_pendingCleanupActions.Any())
            {
                return;
            }

            await CurrentUnitOfWork.SaveChangesAsync();

            foreach (var action in _pendingCleanupActions)
            {
                await action();
            }
            _pendingCleanupActions.Clear();
        }






        public async Task SendOrdersToDrivers(SendOrdersToDriversInput input)
        {
            await CacheDataForDateShift(input);

            await SendOrdersToDriversInternal(input);

            await CleanUp();
        }

        public async Task<bool> CanAddDispatchBasedOnTime(CanAddDispatchBasedOnTimeInput input)
        {
            await PopulateOrderLineCache(input.OrderLineId);
            await PopulateOrderLineTruckCache(input.OrderLineId);

            var truckIds = _orderLineTrucks
                .Where(x => x.OrderLineId == input.OrderLineId && input.OrderLineTruckIds.Contains(x.OrderLineTruckId))
                .Select(x => x.TruckId).Distinct().ToList();

            await PopulateDispatchCache(
                (await _dispatchRepository.GetQueryAsync())
                    .Where(x => input.OrderLineId == x.OrderLineId
                        || Dispatch.OpenStatuses.Contains(x.Status) && truckIds.Contains(x.TruckId))
            );

            var result = await CanAddDispatchBasedOnTimeInternal(input);

            return result;
        }

        private async Task<bool> CanAddDispatchBasedOnTimeInternal(CanAddDispatchBasedOnTimeInput input)
        {
            if (!input.OrderLineTruckIds.Any())
            {
                return false;
            }

            var orderLine = await GetOrderLineFromCacheAsync(input.OrderLineId);
            var orderLineTrucks = orderLine.OrderLineTrucks
                .Where(olt => input.OrderLineTruckIds.Contains(olt.OrderLineTruckId))
                .ToList();

            if (orderLineTrucks.Any(olt => (olt.TruckTimeOnJobUtc ?? orderLine.TimeOnJobUtc) == null))
            {
                return false;
            }

            foreach (var orderLineTruck in orderLineTrucks)
            {
                var dispatches = GetOrderedDispatchesForTruck(orderLineTruck.TruckId);

                if (dispatches.Count < 2)
                {
                    continue;
                }
                var previousTimeUtc = dispatches.First().TimeOnJob;
                if (previousTimeUtc == null)
                {
                    return false;
                }
                foreach (var dispatch in dispatches.Skip(1))
                {
                    var dispatchTimeOnJobUtc = dispatch.TimeOnJob;
                    if (dispatchTimeOnJobUtc == null || dispatchTimeOnJobUtc < previousTimeUtc)
                    {
                        return false;
                    }
                    previousTimeUtc = dispatchTimeOnJobUtc;
                }
            }

            return true;
        }

        private List<DispatchDto> GetOrderedDispatchesForTruck(int truckId)
        {
            if (!_orderedTruckDispatches.ContainsKey(truckId))
            {
                _orderedTruckDispatches.Add(truckId, _dispatches?
                    .Where(x => Dispatch.OpenStatuses.Contains(x.Status) && x.TruckId == truckId)
                    .OrderByDescending(d => d.Status == DispatchStatus.Loaded)
                    .ThenByDescending(d => d.Status == DispatchStatus.Acknowledged)
                    .ThenBy(d => d.SortOrder)
                    .ToList() ?? new());
            }
            return _orderedTruckDispatches[truckId];
        }

        private async Task SendOrdersToDriversInternal(SendOrdersToDriversInput input)
        {
            if (_orderLines == null || _orderLineTrucks == null || _driverAssignments == null)
            {
                throw new ApplicationException("Cache hasn't been populated prior to calling SendOrdersToDriversInternal. Make sure you call CacheDataForDateShift before using SendOrdersToDriversInternal");
            }

            bool ordersExistForDate = _orderLines
                    .Where(o => o.DeliveryDate == input.DeliveryDate)
                    .WhereIf(input.Shift.HasValue, o => o.Shift == input.Shift)
                    .Any();

            if (!ordersExistForDate)
            {
                throw new UserFriendlyException(L("SendOrders_NoOrders", input.DeliveryDate.ToString("d")));
            }

            var orderLineTrucks = GetCachedOrderLineTrucks(input);
            var orderLineTruckGroups = orderLineTrucks.GroupBy(x => x.DriverId);

            if (!orderLineTrucks.Any())
            {
                throw new UserFriendlyException(
                    input.Shift.HasValue ?
                        L("SendOrdersToDrivers_NoTrucks_DateShift", input.DeliveryDate.ToString("d"), await SettingManager.GetShiftName(input.Shift.Value)) :
                        L("SendOrdersToDrivers_NoTrucks_Date", input.DeliveryDate.ToString("d"))
                );
            }

            if (!input.SendOnlyFirstOrder && orderLineTrucks.Any(o => (o.TruckTimeOnJobUtc ?? o.OrderLine?.TimeOnJobUtc) == null))
            {
                throw new UserFriendlyException(L("SomeOfOrderDontHaveStartTimeSpecified"));
            }

            var sendDispatchMessageInputList = new List<SendDispatchMessageInput>();
            foreach (var orderLineTruckGroup in orderLineTruckGroups)
            {
                if (input.SendOnlyFirstOrder)
                {
                    var orderLineTruck = orderLineTruckGroup.First();
                    sendDispatchMessageInputList.Add(new SendDispatchMessageInput
                    {
                        OrderLineId = orderLineTruck.OrderLineId,
                        NumberOfDispatches = 1,
                        FirstDispatchForDay = true,
                        OrderLineTruckIds = new[] { orderLineTruck.OrderLineTruckId },
                    });
                }
                else
                {
                    sendDispatchMessageInputList.AddRange(orderLineTruckGroup.Select((orderLineTruck, i) => new SendDispatchMessageInput
                    {
                        OrderLineId = orderLineTruck.OrderLineId,
                        NumberOfDispatches = 1,
                        FirstDispatchForDay = i == 0,
                        OrderLineTruckIds = new[] { orderLineTruck.OrderLineTruckId },
                    }));
                }
            }

            //regrouping them together by OrderLineId breaks the order in which they should be sent out for some of the drivers (#10211)
            //sdmInputList = sdmInputList.GroupBy(x => new { x.OrderLineId, x.FirstDispatchForDay }).Select(g => new SendDispatchMessageInput
            //{
            //    OrderLineId = g.Key.OrderLineId,
            //    NumberOfDispatches = 1,
            //    FirstDispatchForDay = g.Key.FirstDispatchForDay,
            //    TruckDrivers = g.SelectMany(x => x.TruckDrivers).Distinct().ToArray()
            //}).ToList();

            var dispatchViaDriverApplication = await SettingManager.DispatchViaDriverApplication();
            var newDispatches = new List<DispatchesOfOrderLineTruck>();
            //bool success = true;
            foreach (var sendDispatchMessageInput in sendDispatchMessageInputList)
            {
                var orderLine = _orderLines.FirstOrDefault(x => x.Id == sendDispatchMessageInput.OrderLineId);
                if (orderLine == null)
                {
                    continue; //we can skip an already deleted order line and silently not include it in the messages, instead of throwing an exception for the whole batch operation
                    //throw await GetOrderLineNotFoundException(new EntityDto(sendDispatchMessageInput.OrderLineId));
                }
                var dto = await CreateSendDispatchMessageDtoInternal(orderLine, sendDispatchMessageInput.FirstDispatchForDay);
                sendDispatchMessageInput.Message = dto.Message;
                sendDispatchMessageInput.IsMultipleLoads = dto.IsMultipleLoads;
                bool skipSmsIfDispatchesExist = dispatchViaDriverApplication && !sendDispatchMessageInput.FirstDispatchForDay;

                var result = await SendDispatchMessageInternal(sendDispatchMessageInput, skipSmsIfDispatchesExist);

                newDispatches.AddRange(result.DispatchesOfOrderLineTruck);
                //success = success && result.Success;
            }

            _pendingCleanupActions.Add(async () =>
            {
                await SendSyncRequestsForCreatedDisaptchesAsync(newDispatches);
            });

            //return success;
        }

        private async Task SendSyncRequestsForCreatedDisaptchesAsync(List<DispatchesOfOrderLineTruck> dispatchesOfOrderLineTruck)
        {
            await _syncRequestSender.SendSyncRequest(new SyncRequest()
                    .AddChanges(EntityEnum.Dispatch,
                        dispatchesOfOrderLineTruck
                            .SelectMany(x => x.Dispatches
                                .OfType<Dispatch>()
                                .Select(dispatch => dispatch.ToChangedEntity())))
                    .AddLogMessage("Created dispatches"));
        }





        private async Task<bool> ShouldSendOrdersToDriversImmediately()
        {
            var dispatchVia = await SettingManager.GetDispatchVia();
            return dispatchVia.IsIn(DispatchVia.None, DispatchVia.SimplifiedSms);
        }

        public async Task<SendDispatchMessageDto> CreateSendDispatchMessageDto(int orderLineId, bool firstDispatchForDay = false)
        {
            await PopulateOrderLineCache(orderLineId);
            await PopulateOrderLineTruckCache(orderLineId);

            var orderLine = await GetOrderLineFromCacheAsync(orderLineId);

            return await CreateSendDispatchMessageDtoInternal(orderLine, firstDispatchForDay);
        }

        private async Task<SendDispatchMessageDto> CreateSendDispatchMessageDtoInternal(OrderLineDto orderLine, bool firstDispatchForDay)
        {
            return new SendDispatchMessageDto
            {
                OrderLineId = orderLine.Id,
                Message = await CreateDispatchMessageFromTemplate(orderLine, firstDispatchForDay),
                OrderLineTrucks = orderLine.OrderLineTrucks.Where(x => !x.IsDone).ToList(),
                IsMultipleLoads = orderLine.IsMultipleLoads,
                FreightUom = orderLine.FreightUom,
                UseTruckCapacityForLoadQuantity = true,
                LoadMaterialQuantity = orderLine.MaterialUomBaseId == null ? orderLine.FreightQuantity : orderLine.MaterialQuantity,
                MaterialItemId = orderLine.MaterialItemId,
                FreightUomBaseId = orderLine.FreightUomBaseId,
            };
        }

        public async Task<string> CreateDispatchMessageFromTemplate(OrderLineDataForDispatchMessage orderLine, bool firstDispatchForDay = false)
        {
            var today = await GetToday();
            var messageTemplate = await SettingManager.GetSettingValueAsync(AppSettings.DispatchingAndMessaging.DriverDispatchSmsTemplate);
            if (firstDispatchForDay)
            {
                var dateString = orderLine.DeliveryDate == today ? "today" : orderLine.DeliveryDate == today.AddDays(1) ? "tomorrow" : orderLine.DeliveryDate.ToShortDateString();
                messageTemplate = $"Your first dispatch for {dateString}: " + messageTemplate;
            }

            var tokenDto = new TemplateTokenDto
            {
                DeliveryDate = orderLine.DeliveryDate.ToString("d"),
                Shift = await SettingManager.GetShiftName(orderLine.Shift) ?? "",
                OrderNumber = orderLine.OrderNumber.ToString(),
                Customer = orderLine.Customer,
                Directions = orderLine.Directions,
                Note = orderLine.Note,
                //TimeOnJob = orderLine.TimeOnJob?.ToString("t"),
                //StartTime = orderLine.StartTime?.ToString("t"),
                Item = orderLine.FreightItemName,
                LoadAt = orderLine.LoadAtName,
                Designation = orderLine.Designation,
                MaterialQuantity = orderLine.MaterialQuantity?.ToString("N"),
                FreightQuantity = orderLine.FreightQuantity?.ToString("N"),
                MaterialUom = orderLine.MaterialUom,
                FreightUom = orderLine.FreightUom,
                DeliverTo = orderLine.DeliverToName,
                ChargeTo = orderLine.ChargeTo,
            };
            return messageTemplate.ReplaceTokensInTemplate(tokenDto);
        }

        public async Task<CreateDispatchesAndSendSmsToEachDriverResult> SendDispatchMessage(
            SendDispatchMessageInput input,
            bool skipSmsIfDispatchesExist = true
        )
        {
            await PopulateOrderLineCache(input.OrderLineId);
            await PopulateOrderLineTruckCache(
                (await _orderLineTruckRepository.GetQueryAsync())
                    .Where(x => x.OrderLineId == input.OrderLineId)
            );

            var orderLine = _orderLines.FirstOrDefault(x => x.Id == input.OrderLineId);
            if (orderLine != null)
            {
                var truckIds = orderLine.OrderLineTrucks.Select(x => x.TruckId).Distinct().ToList();
                await PopulateDispatchCache((await _dispatchRepository.GetQueryAsync())
                    .Where(x => input.OrderLineId == x.OrderLineId
                        || Dispatch.OpenStatuses.Contains(x.Status) && truckIds.Contains(x.TruckId)));

                await PopulateDriverAssignmentCache(new SendOrdersToDriversInput
                {
                    DeliveryDate = orderLine.DeliveryDate,
                    OfficeIds = new[] { orderLine.OfficeId },
                    Shift = orderLine.Shift,
                });

                var driverIds = _orderLineTrucks
                    .Where(x => x.OrderLineId == input.OrderLineId)
                    .Select(x => x.DriverId).Distinct().ToArray();
                await PopulateDriverAcknowledgements(driverIds);
            }
            await PopulateEmployeeTimeCache(await GetToday());

            var result = await SendDispatchMessageInternal(input, skipSmsIfDispatchesExist);

            _pendingCleanupActions.Add(async () =>
            {
                await SendSyncRequestsForCreatedDisaptchesAsync(result.DispatchesOfOrderLineTruck);
            });

            await CleanUp();

            return result;
        }

        private async Task<CreateDispatchesAndSendSmsToEachDriverResult> SendDispatchMessageInternal(
            SendDispatchMessageInput input,
            bool skipSmsIfDispatchesExist = true
        )
        {
            var sendOrdersToDriversImmediately = await ShouldSendOrdersToDriversImmediately();
            ValidateNumberOfDispatches(input.NumberOfDispatches);
            await EnsureCanCreateDispatchInternalAsync(input.OrderLineId, input.OrderLineTruckIds.Length, input.NumberOfDispatches, input.IsMultipleLoads);

            var canAddDispatchBasedOnTimeInternal = await CanAddDispatchBasedOnTimeInternal(new CanAddDispatchBasedOnTimeInput
            {
                OrderLineId = input.OrderLineId,
                OrderLineTruckIds = input.OrderLineTruckIds.ToList(),
            });

            if (input.AddDispatchBasedOnTime && !canAddDispatchBasedOnTimeInternal)
            {
                input.AddDispatchBasedOnTime = false;
            }

            var orderLine = await GetOrderLineFromCacheAsync(input.OrderLineId);

            if (orderLine.IsComplete)
            {
                throw new UserFriendlyException("Cannot create dispatches because the OrderLine is complete!");
            }

            var today = await GetToday();
            if (today > orderLine.DeliveryDate)
            {
                throw new UserFriendlyException("Cannot create Dispatch for the past date!");
            }

            var orderLineTrucks = input.OrderLineTruckIds
                .Select(x => orderLine.OrderLineTrucks.FirstOrDefault(d => d.OrderLineTruckId == x))
                .Where(x => x != null)
                .ToList();

            if (orderLineTrucks.Any(x => x.IsDone))
            {
                throw new UserFriendlyException("Cannot create dispatches because one or more trucks are done!");
            }

            var dispatchVia = await SettingManager.GetDispatchVia();
            var dispatchViaDriverApp = dispatchVia == DispatchVia.DriverApplication;

            var result = new CreateDispatchesAndSendSmsToEachDriverResult
            {
                //Success = true,
                DispatchesOfOrderLineTruck = new List<DispatchesOfOrderLineTruck>(),
            };

            var timezone = await GetTimezone();

            foreach (var orderLineTruck in orderLineTrucks)
            {
                var timeOnJobUtc = orderLineTruck?.TruckTimeOnJobUtc ?? orderLine.TimeOnJobUtc;

                if (!orderLineTruck.HasContactInfo
                    && (await SettingManager.SendSmsOnDispatching() != SendSmsOnDispatchingEnum.DontSend || !dispatchViaDriverApp))
                {
                    Logger.Warn($"The Driver with DriverId={orderLineTruck.DriverId} doesn't have a cell phone number or an email.");
                    if (!dispatchViaDriverApp)
                    {
                        _pendingCleanupActions.Add(async () =>
                        {
                            await _appNotifier.SendMessageAsync(
                                await Session.ToUserIdentifierAsync(),
                                $"Driver {orderLineTruck.DriverName} doesn't have a valid SMS number or Email Address.",
                                NotificationSeverity.Error
                            );
                        });
                        //result.Success = false;
                    }
                    if (!dispatchViaDriverApp)
                    {
                        continue;
                    }
                }

                if (dispatchViaDriverApp && !orderLineTruck.UserId.HasValue)
                {
                    _pendingCleanupActions.Add(async () =>
                    {
                        await _appNotifier.SendMessageAsync(
                            await Session.ToUserIdentifierAsync(),
                            $"{orderLineTruck.DriverName} has been assigned a dispatch, but is not set up as a user.",
                            NotificationSeverity.Warn
                        );
                    });
                }

                var oldActiveDispatch = GetFirstOpenDispatch(orderLineTruck.DriverId);

                var numberOfDispatchesToCreate = dispatchVia == DispatchVia.SimplifiedSms ? 1 : input.NumberOfDispatches;
                var affectedDispatches = new List<Dispatch>();
                DispatchDto firstNewDispatch = null;
                for (int i = 0; i < numberOfDispatchesToCreate; i++)
                {
                    Guid acknowledgementGuid = Guid.NewGuid();

                    var driverAssignment = _driverAssignments.FirstOrDefault(x => x.DriverId == orderLineTruck.DriverId && x.TruckId == orderLineTruck.TruckId);
                    string dispatchMessage = FormatDispatchMessage(new FormatDispatchMessageInput
                    {
                        Message = input.Message,
                        AcknowledgementGuid = acknowledgementGuid,
                        TruckCode = orderLineTruck.TruckCode,
                        TimeOnJob = timeOnJobUtc?.ConvertTimeZoneTo(timezone),
                        DispatchVia = dispatchVia,
                        StartTime = driverAssignment?.StartTimeUtc?.ConvertTimeZoneTo(timezone),
                        MultipleLoads = input.IsMultipleLoads,
                        NumberOfDispatches = input.NumberOfDispatches,
                    });

                    var newDispatch = new DispatchEditDto
                    {
                        TruckId = orderLineTruck.TruckId,
                        DriverId = orderLineTruck.DriverId,
                        OrderLineId = orderLine.Id,
                        OrderLineTruckId = orderLineTruck.OrderLineTruckId,
                        PhoneNumber = orderLineTruck.DriverPhoneNumber,
                        EmailAddress = orderLineTruck.DriverEmailAddress,
                        UserId = orderLineTruck.UserId,
                        OrderNotifyPreferredFormat = orderLineTruck.DriverPreferredNotifyFormat,
                        Message = dispatchMessage,
                        Note = orderLine.Note,
                        IsMultipleLoads = input.IsMultipleLoads,
                        WasMultipleLoads = input.IsMultipleLoads,
                        Guid = acknowledgementGuid,
                        Status = DispatchStatus.Created,
                        TimeOnJob = orderLineTruck.TruckTimeOnJobUtc ?? orderLine.TimeOnJobUtc,
                        MaterialQuantity = orderLine.MaterialQuantity,
                        FreightQuantity = orderLine.FreightQuantity,
                    };

                    UnitOfMeasureBaseEnum? uomBaseId;
                    decimal? orderQuantity;
                    if (orderLine.MaterialUomBaseId != null)
                    {
                        uomBaseId = orderLine.MaterialUomBaseId;
                        orderQuantity = orderLine.MaterialQuantity;
                    }
                    else
                    {
                        uomBaseId = orderLine.FreightUomBaseId;
                        orderQuantity = orderLine.FreightQuantity;
                    }

                    var manualQuantity = !input.UseTruckCapacityForLoadQuantity && input.LoadMaterialQuantity.HasValue
                        ? input.LoadMaterialQuantity
                        : null;

                    var truckCapacity = uomBaseId switch
                    {
                        UnitOfMeasureBaseEnum.Tons => orderLineTruck.CargoCapacity,
                        UnitOfMeasureBaseEnum.CubicYards => orderLineTruck.CargoCapacityCyds,
                        _ => null,
                    };

                    var dispatchQuantity = manualQuantity.HasValue
                        ? manualQuantity
                        : orderQuantity >= truckCapacity
                            ? truckCapacity
                            : orderQuantity;

                    if (orderLine.MaterialUomBaseId == null)
                    {
                        newDispatch.FreightQuantity = dispatchQuantity;
                    }
                    else
                    {
                        newDispatch.MaterialQuantity = dispatchQuantity;
                    }


                    //commented out to keep Dispatch TimeOnJob matching exactly with OrderLineTruck or OrderLine's TimeOnJob
                    //newDispatch.TimeOnJob = newDispatch.TimeOnJob == null ? null
                    //    : (orderLine.DeliveryDate ?? await GetToday()).Date.Add(newDispatch.TimeOnJob.Value.ConvertTimeZoneTo(timezone).TimeOfDay).ConvertTimeZoneFrom(timezone);

                    var orderedTruckDispatches = GetOrderedDispatchesForTruck(orderLineTruck.TruckId);
                    affectedDispatches.Add(AddDispatch(newDispatch));
                    firstNewDispatch ??= newDispatch;

                    if (input.AddDispatchBasedOnTime && timeOnJobUtc.HasValue)
                    {
                        var insertIndex = orderedTruckDispatches.FindIndex(x => x.TimeOnJob > timeOnJobUtc);
                        if (insertIndex != -1)
                        {
                            orderedTruckDispatches.RemoveAll(x => x == newDispatch);
                            orderedTruckDispatches.Insert(insertIndex, newDispatch);
                        }
                    }
                }
                //await CurrentUnitOfWork.SaveChangesAsync();
                //var newActiveDispatch = GetFirstOpenDispatch(orderLineTruck.DriverId);

                if (firstNewDispatch == null)
                {
                    continue;
                }

                result.DispatchesOfOrderLineTruck.Add(new DispatchesOfOrderLineTruck
                {
                    OrderLineTruck = orderLineTruck,
                    Dispatches = affectedDispatches,
                });

                DeferredSendSmsOrEmail(new DeferredSendSmsOrEmailInput
                {
                    TruckId = orderLineTruck.TruckId,
                    DriverId = orderLineTruck.DriverId,
                    UserId = orderLineTruck.UserId,
                    PhoneNumber = orderLineTruck.DriverPhoneNumber,
                    EmailAddress = orderLineTruck.DriverEmailAddress,
                    OrderNotifyPreferredFormat = orderLineTruck.DriverPreferredNotifyFormat,
                    SendOrdersToDriversImmediately = sendOrdersToDriversImmediately,
                    SkipIfDispatchesExist = skipSmsIfDispatchesExist,
                    OldActiveDispatch = oldActiveDispatch,
                    Dispatch = firstNewDispatch, //the following sends a "first" dispatch each time. GetNextDispatch(orderLineTruck.TruckId, orderLineTruck.DriverId),
                    //we need to wait until we save the dispatches and update their SortOrder during cleanup, and only then we can get the next active dispatch.
                    //var newActiveDispatch = GetFirstOpenDispatch(orderLineTruck.DriverId);
                    //ActiveDispatchWasChanged = oldActiveDispatch?.Id != newActiveDispatch?.Id
                });

                //switch (sendSmsResult)
                //{
                //    case SendSmsResultError e:
                //        result.Success = false;
                //        break;
                //}
            }

            //if (await SettingManager.DispatchViaDriverApplication())
            //{
            //    result.Success = true;
            //}

            return result;
        }

        public static void ValidateNumberOfDispatches(int numberOfDispatches)
        {
            if (numberOfDispatches < 1 || numberOfDispatches > DispatchingAppService.MaxNumberOfDispatches)
            {
                throw new ArgumentException($"The number of dispatches per truck must be between 1 and {DispatchingAppService.MaxNumberOfDispatches}!");
            }
        }

        public async Task EnsureCanCreateDispatchAsync(int orderLineId, int newTruckCount, int newDispatchCount, bool multipleLoads)
        {
            //no need to use a cached approach for a single external call, it won't have a better performance
            //for internal use when the data is already cached, please use EnsureCanCreateDispatchInternalAsync instead
            //await PopulateOrderLineCache(orderLineId);
            //await PopulateDispatchCache((await _dispatchRepository.GetQueryAsync())
            //    .Where(x => x.OrderLineId == orderLineId));
            //await EnsureCanCreateDispatchInternalAsync(orderLineId, newTruckCount, newDispatchCount, multipleLoads);

            var orderLine = await (await _orderLineRepository.GetQueryAsync())
                .Where(x => x.Id == orderLineId)
                .Select(x => new
                {
                    x.IsFreightPriceOverridden,
                    x.IsMaterialPriceOverridden,
                }).FirstAsync();

            if (orderLine.IsFreightPriceOverridden || orderLine.IsMaterialPriceOverridden)
            {
                if (newTruckCount > 1
                    || newDispatchCount > 1
                    || multipleLoads
                    || await (await _dispatchRepository.GetQueryAsync()).AnyAsync(x => x.OrderLineId == orderLineId && x.Status != DispatchStatus.Canceled))
                {
                    throw new UserFriendlyException(L("OrderLineWithOverriddenTotalCanOnlyHaveSingleTicketError"));
                }
            }
        }

        private async Task EnsureCanCreateDispatchInternalAsync(int orderLineId, int newTruckCount, int newDispatchCount, bool multipleLoads)
        {
            var orderLine = await GetOrderLineFromCacheAsync(orderLineId);
            if (_dispatches == null)
            {
                throw new ApplicationException("Dispatcher cache wasn't populated");
            }

            if (orderLine.IsFreightPriceOverridden || orderLine.IsMaterialPriceOverridden)
            {
                if (newTruckCount > 1
                    || newDispatchCount > 1
                    || multipleLoads
                    || _dispatches.Any(x => x.OrderLineId == orderLineId && x.Status != DispatchStatus.Canceled))
                {
                    throw new UserFriendlyException(L("OrderLineWithOverriddenTotalCanOnlyHaveSingleTicketError"));
                }
            }
        }

        private async Task<OrderLineDto> GetOrderLineFromCacheAsync(int id)
        {
            if (_orderLines == null)
            {
                throw new ApplicationException("OrderLine cache wasn't populated");
            }

            var orderLine = _orderLines.FirstOrDefault(x => x.Id == id);
            if (orderLine != null)
            {
                return orderLine;
            }

            throw await GetOrderLineNotFoundExceptionAsync(new EntityDto(id));
        }

        private async Task<Exception> GetOrderLineNotFoundExceptionAsync(EntityDto input)
        {
            if (await _orderLineRepository.IsEntityDeleted(input, CurrentUnitOfWork))
            {
                return new EntityDeletedException("Order Line", "This order line has been deleted and can’t be edited");
            }

            return new Exception($"Order Line with id {input.Id} wasn't found and is not deleted");
        }

        protected override async Task<User> GetCurrentUserAsync()
        {
            return _cachedCurrentUser ??= await base.GetCurrentUserAsync();
        }

        [RemoteService(false)]
        public async Task SendSmsOrEmail(SendSmsOrEmailInput input)
        {
            await BatchSendSmsOrEmail(input);
        }

        [RemoteService(false)]
        public async Task BatchSendSmsOrEmail(params SendSmsOrEmailInput[] inputs)
        {
            var dispatchIds = inputs.Where(x => x.DispatchId.HasValue).Select(x => x.DispatchId.Value).Distinct().ToList();
            if (dispatchIds.Any())
            {
                var truckIds = inputs.Select(x => x.TruckId).Distinct().ToList();
                var driverIds = inputs.Select(x => x.DriverId).Distinct().ToList();
                var dispatchQuery = (await _dispatchRepository.GetQueryAsync())
                    .Where(x => dispatchIds.Contains(x.Id)
                        || Dispatch.OpenStatuses.Contains(x.Status) && (truckIds.Contains(x.TruckId)
                        || driverIds.Contains(x.DriverId))
                    );
                await PopulateDispatchCache(dispatchQuery);
                await PopulateDispatchEntityCache(dispatchQuery);
            }

            foreach (var input in inputs)
            {
                await SendSmsOrEmailInternal(input);
            }

            await CleanUp();
        }

        private void DeferredSendSmsOrEmail(DeferredSendSmsOrEmailInput input)
        {
            _deferredSmsOrEmailMessages.Add(input);
        }

        private async Task SendSmsOrEmailInternal(SendSmsOrEmailInput input)
        {
            if (await SettingManager.DispatchViaDriverApplication())
            {
                var sendSmsOnDispatching = await SettingManager.SendSmsOnDispatching();
                if (sendSmsOnDispatching == SendSmsOnDispatchingEnum.DontSend
                    || (sendSmsOnDispatching == SendSmsOnDispatchingEnum.SendWhenUserNotClockedIn
                        && input.UserId.HasValue && await IsUserClockedIn(input.UserId.Value)))
                {
                    Logger.Info($"Skipping sms to DriverId {input.DriverId} ({input.PhoneNumber}/{input.EmailAddress}) because SendSmsOnDispatching is {sendSmsOnDispatching}");
                    return;
                }
            }

            if (input.ActiveDispatchWasChanged == false && !input.SendOrdersToDriversImmediately && input.SkipIfDispatchesExist)
            {
                Logger.Info($"Skipping sms to DriverId {input.DriverId} ({input.PhoneNumber}/{input.EmailAddress}) because ActiveDispatchWasChanged=false, SendOrdersToDriversImmediately=false, SkipIfDispatchesExist=true");
                return;
            }

            var dispatch = input.DispatchId.HasValue ? await GetDispatchFromCacheOrDb(input.DispatchId.Value) : await GetNextDispatchFromDb(input.TruckId, input.DriverId);
            if (dispatch == null)
            {
                Logger.Info($"Skipping sms to DriverId {input.DriverId} ({input.PhoneNumber}/{input.EmailAddress}) because no dispatch was found (using DispatchId '{input.DispatchId}')");
                return;
            }

            if (input.AfterCompleted && await DriverAlreadyAcknowledgedDispatchTodayFromCacheOrDb(input.TruckId, input.DriverId))
            {
                Logger.Info($"Skipping sms to DriverId {input.DriverId} ({input.PhoneNumber}/{input.EmailAddress}) because AfterCompleted=true and  DriverAlreadyAcknowledgedDispatchToday");
                return;
            }

            var messageSendingErrors = new List<string>();

            if (input.OrderNotifyPreferredFormat.IsIn(OrderNotifyPreferredFormat.Neither))
            {
                messageSendingErrors.Add("Preferred notify format is set to " + input.OrderNotifyPreferredFormat);
            }
            else
            {
                if (input.OrderNotifyPreferredFormat.HasFlag(OrderNotifyPreferredFormat.Sms))
                {
                    if (input.PhoneNumber.IsNullOrEmpty())
                    {
                        messageSendingErrors.Add("Driver doesn't have a valid SMS number.");
                    }
                    else
                    {
                        _smsSenderBackgrounJobArgs ??= new SmsSenderBackgroundJobArgs
                        {
                            RequestorUser = await Session.ToUserIdentifierAsync(),
                            SmsInputs = new List<SmsSendInput>(),
                        };
                        _smsSenderBackgrounJobArgs.SmsInputs.Add(new SmsSendInput
                        {
                            //todo add contact name later, this wasn't implemented earlier. Make sure the cache is populated if we're using the below method.
                            //ContactName = await GetDriverNameFromCacheOrDb(dispatch.DriverId),
                            Body = dispatch.Message?.TruncateWithPostfix(EntityStringFieldLengths.Dispatch.SmsMessageLimit),
                            ToPhoneNumber = input.PhoneNumber,
                            TrackStatus = false,
                        });
                        Logger.Info($"Scheduled SMS sending for dispatchId {dispatch.Id}, driverId {input.DriverId}, phone {input.PhoneNumber}");
                    }
                }
                if (input.OrderNotifyPreferredFormat.HasFlag(OrderNotifyPreferredFormat.Email))
                {
                    if (input.EmailAddress.IsNullOrEmpty())
                    {
                        messageSendingErrors.Add("Driver doesn't have a valid Email address.");
                    }
                    else
                    {
                        _emailSenderBackgrounJobArgs ??= new EmailSenderBackgroundJobArgs
                        {
                            RequestorUser = await Session.ToUserIdentifierAsync(),
                            EmailInputs = new List<EmailSenderBackgroundJobArgsEmail>(),
                        };
                        _emailSenderBackgrounJobArgs.EmailInputs.Add(new EmailSenderBackgroundJobArgsEmail
                        {
                            Body = dispatch.Message,
                            ToEmailAddress = input.EmailAddress,
                        });
                        Logger.Info($"Scheduled Email sending for dispatchId {dispatch.Id}, driverId {input.DriverId}, email {input.EmailAddress}");
                    }
                }
            }

            _sentDisaptchIds.Add(dispatch.Id);
            if (messageSendingErrors.Any())
            {
                string driverName = await GetDriverNameFromCacheOrDb(input.DriverId);
                var messageSendingErrorsFormatted = string.Join(" \r\n", messageSendingErrors);
                Logger.Error($"There was an error while sending the sms/email to DriverId: {input.DriverId}, TruckId: {input.TruckId}. {messageSendingErrorsFormatted}");
                string detailedErrorMessage = $"Unable to send the dispatch for {driverName} with phone number '{input.PhoneNumber}' and email address '{input.EmailAddress}'. {messageSendingErrorsFormatted}";
                if (!await SettingManager.DispatchViaDriverApplication())
                {
                    _pendingCleanupActions.Add(async () =>
                    {
                        await _appNotifier.SendMessageAsync(
                            await Session.ToUserIdentifierAsync(),
                            detailedErrorMessage,
                            NotificationSeverity.Error
                        );
                    });
                }
            }

            if (input.SendOrdersToDriversImmediately)
            {
                ChangeDispatchStatusToCompleted(dispatch);
            }
            else
            {
                ChangeDispatchStatusToSent(dispatch);
            }
        }

        //this implementation is different from DispatchingAppService.ChangeDispatchStatusToCompleted, this one is not expected to be an entry point
        private void ChangeDispatchStatusToCompleted(Dispatch dispatch)
        {
            if (dispatch.NumberOfLoadsToFinish > 0 && dispatch.NumberOfLoadsToFinish < dispatch.NumberOfAddedLoads)
            {
                dispatch.Status = DispatchStatus.Canceled;
                dispatch.Canceled ??= Clock.Now;
            }
            else
            {
                if (dispatch.Status != DispatchStatus.Canceled)
                {
                    dispatch.Status = DispatchStatus.Completed;
                }
            }

            if (_dispatches != null)
            {
                _dispatches.Where(x => x.Id == dispatch.Id).ToList().ForEach(x => x.Status = dispatch.Status);
            }
        }

        private void ChangeDispatchStatusToSent(Dispatch dispatch)
        {
            if (dispatch.Status != DispatchStatus.Created)
            {
                throw new ApplicationException("Dispatch must be in 'Created' status!");
            }
            dispatch.Status = DispatchStatus.Sent;
            dispatch.Sent = Clock.Now;

            if (_dispatches != null)
            {
                _dispatches.Where(x => x.Id == dispatch.Id).ToList().ForEach(x => x.Status = dispatch.Status);
            }
        }

        private async Task<bool> IsUserClockedIn(long userId)
        {
            if (_employeeTimes == null)
            {
                await PopulateEmployeeTimeCache(await GetToday());
            }

            var timezone = await GetTimezone();
            var todayInUtc = (await GetToday()).ConvertTimeZoneFrom(timezone);
            var tomorrowInUtc = todayInUtc.AddDays(1);
            return _employeeTimes.Any(x => x.UserId == userId && x.StartDateTime >= todayInUtc && x.StartDateTime < tomorrowInUtc && x.EndDateTime == null && !x.IsImported);
        }

        private DispatchDto GetNextDispatch(int truckId, int driverId)
        {
            return GetOrderedDispatchesForTruck(truckId).FirstOrDefault(x => x.DriverId == driverId && x.Status == DispatchStatus.Created);
            //return _dispatches
            //    .Where(d => d.TruckId == truckId && d.DriverId == driverId && d.Status == DispatchStatus.Created)
            //    .OrderBy(d => d.SortOrder)
            //    .FirstOrDefault();
        }

        private async Task<Dispatch> GetNextDispatchFromDb(int truckId, int driverId)
        {
            var dispatch = await (await _dispatchRepository.GetQueryAsync())
                .Where(d => d.TruckId == truckId && d.DriverId == driverId && d.Status == DispatchStatus.Created && !_sentDisaptchIds.Contains(d.Id))
                .OrderBy(d => d.SortOrder)
                .FirstOrDefaultAsync();

            if (dispatch != null)
            {
                _dispatchEntities.AddIfNotContains(dispatch);
            }

            return dispatch;
        }

        private async Task<bool> DriverAlreadyAcknowledgedDispatchTodayFromCacheOrDb(int truckId, int driverId)
        {
            if (_driverAcknowledgements != null)
            {
                var driverAcknowledgement = _driverAcknowledgements.FirstOrDefault(x => x.DriverId == driverId);
                if (driverAcknowledgement != null)
                {
                    return driverAcknowledgement.HasAcknowledgedDispatchToday;
                }
            }

            Logger.Warn($"DriverAcknowledgements are not populated, using the DB instead of cache to get DriverAlreadyAcknowledgedDispatchTodayFromCacheOrDb for truckId {truckId} driverId {driverId}");
            var timeZone = await GetTimezone();
            var today = await GetToday();
            var todayInUtc = today.ConvertTimeZoneFrom(timeZone);
            //this won't be in the cache if it's not in the open state anymore, and if we download all recently acknowledged dispatches to the cache each time then we probably won't get better performance.
            //return _dispatches
            //    .Any(d => d.TruckId == truckId && d.DriverId == driverId && d.Acknowledged?.ConvertTimeZoneTo(timeZone) >= today);
            return await (await _dispatchRepository.GetQueryAsync())
                .AnyAsync(d => d.TruckId == truckId && d.DriverId == driverId && d.Acknowledged >= todayInUtc);
        }

        public Dispatch AddDispatch(DispatchEditDto dispatchDto)
        {
            var newDispatch = new Dispatch
            {
                TruckId = dispatchDto.TruckId,
                DriverId = dispatchDto.DriverId,
                OrderLineId = dispatchDto.OrderLineId,
                OrderLineTruckId = dispatchDto.OrderLineTruckId,
                PhoneNumber = dispatchDto.PhoneNumber,
                EmailAddress = dispatchDto.EmailAddress,
                UserId = dispatchDto.UserId,
                OrderNotifyPreferredFormat = dispatchDto.OrderNotifyPreferredFormat,
                Message = dispatchDto.Message?.TruncateWithPostfix(EntityStringFieldLengths.Dispatch.Message),
                Note = dispatchDto.Note,
                IsMultipleLoads = dispatchDto.IsMultipleLoads,
                WasMultipleLoads = dispatchDto.IsMultipleLoads,
                Guid = dispatchDto.Guid,
                Status = dispatchDto.Status,
                TimeOnJob = dispatchDto.TimeOnJob,
                MaterialQuantity = dispatchDto.MaterialQuantity,
                FreightQuantity = dispatchDto.FreightQuantity,
            };
            _pendingNewDispatchQueue.Add((dispatchDto, newDispatch)); //on cleanup, save all dispatches at once, but since the order they get saved at could be different from the order they were added in, instead of doing Order=Id we'll get all new ids, sort them, and assign them to Dispatches in order in which they appear in _pendingNewDispatchQueue, thus preserving the expected order of new dispatches
            GetOrderedDispatchesForTruck(dispatchDto.TruckId).Add(dispatchDto);
            _dispatches ??= new();
            _dispatches.Add(dispatchDto);
            return newDispatch;
        }

        private DispatchDto GetFirstOpenDispatch(int driverId)
        {
            return _dispatches
                .Where(d => d.DriverId == driverId && Dispatch.OpenStatuses.Contains(d.Status))
                .OrderByDescending(d => d.Status == DispatchStatus.Loaded)
                .ThenByDescending(d => d.Status == DispatchStatus.Acknowledged)
                .ThenByDescending(d => d.Status == DispatchStatus.Sent)
                .ThenByDescending(d => d.Id != 0)
                .ThenBy(d => d.SortOrder)
                .FirstOrDefault();
        }

        private string FormatDispatchMessage(FormatDispatchMessageInput input)
        {
            string multipleLoadsString = input.MultipleLoads ? L("RunUntilStopped") + ". " : "";
            string numberOfLoadsString = input.DispatchVia == DispatchVia.SimplifiedSms && input.NumberOfDispatches > 1 ? $"{input.NumberOfDispatches} loads. " : "";
            var driverMessage = input.Message
                .Replace(TemplateTokens.TruckCode, input.TruckCode, false, CultureInfo.InvariantCulture)
                .Replace(TemplateTokens.TimeOnJob, input.TimeOnJob?.ToString("t"), false, CultureInfo.InvariantCulture)
                .Replace(TemplateTokens.StartTime, input.StartTime?.ToString("t"), false, CultureInfo.InvariantCulture);

            return $"{multipleLoadsString}{numberOfLoadsString}{driverMessage}";
        }

        private async Task<Dispatch> GetDispatchFromCacheOrDb(int dispatchId)
        {
            var dispatch = _dispatchEntities?.FirstOrDefault(x => x.Id == dispatchId);
            if (dispatch == null)
            {
                Logger.Warn("Didn't find dispatchId " + dispatchId + " in the cache, getting an entity from the DB");
                dispatch = await _dispatchRepository.GetAsync(dispatchId);
                if (dispatch != null)
                {
                    _dispatchEntities.Add(dispatch);
                }
            }
            return dispatch;
        }

        private async Task<string> GetDriverNameFromCacheOrDb(int driverId)
        {
            if (_orderLineTrucks != null)
            {
                var orderLineTruck = _orderLineTrucks.FirstOrDefault(x => x.DriverId == driverId);
                if (orderLineTruck != null)
                {
                    return orderLineTruck.DriverName;
                }
            }

            return await (await _driverRepository.GetQueryAsync()).Where(d => d.Id == driverId).Select(d => d.FirstName + " " + d.LastName).FirstAsync();
        }

    }
}
