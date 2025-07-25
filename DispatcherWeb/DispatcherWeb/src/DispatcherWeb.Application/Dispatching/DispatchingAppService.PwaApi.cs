using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Abp.Application.Services;
using Abp.Authorization;
using Abp.Configuration;
using Abp.Domain.Uow;
using Abp.Extensions;
using Abp.Linq.Extensions;
using Abp.Notifications;
using Abp.Runtime.Session;
using Abp.Timing;
using DispatcherWeb.Authorization.Roles;
using DispatcherWeb.Common.Dto;
using DispatcherWeb.Configuration;
using DispatcherWeb.Dispatching.Dto;
using DispatcherWeb.DriverApplication.Dto;
using DispatcherWeb.Drivers;
using DispatcherWeb.Features;
using DispatcherWeb.Infrastructure.Extensions;
using DispatcherWeb.Notifications;
using DispatcherWeb.Orders;
using DispatcherWeb.Storage;
using DispatcherWeb.SyncRequests;
using DispatcherWeb.Tickets;
using DispatcherWeb.Tickets.Dto;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace DispatcherWeb.Dispatching
{
    [AbpAuthorize]
    public partial class DispatchingAppService
    {

        [AbpAllowAnonymous]
        [UnitOfWork(IsDisabled = true)]
        public async Task<bool> ExecuteDriverApplicationAction(ExecuteDriverApplicationActionInput input)
        {
            return await UnitOfWorkManager.WithUnitOfWorkAsync(async () =>
            {
                var authInfo = await _driverApplicationAuthProvider.AuthDriverByDriverGuid(input.DriverGuid);
                using (Session.Use(authInfo.TenantId, authInfo.UserId))
                {
                    var info = new DriverApplicationActionInfo(input, authInfo);
                    info.TimeZone = await SettingManager.GetSettingValueForUserAsync(TimingSettingNames.TimeZone, info.TenantId, info.UserId);
                    switch (input.Action)
                    {
                        case DriverApplicationAction.ClockIn:
                            await DriverClockIn(info, input.ClockInData);
                            break;
                        case DriverApplicationAction.ClockOut:
                            await DriverClockOut(info);
                            break;
                        case DriverApplicationAction.AcknowledgeDispatch:
                            input.AcknowledgeDispatchData.Info = info;
                            await AcknowledgeDispatchInternal(input.AcknowledgeDispatchData);
                            break;
                        case DriverApplicationAction.LoadDispatch:
                            input.LoadDispatchData.Info = info;
                            await LoadDispatchInternal(input.LoadDispatchData);
                            break;
                        case DriverApplicationAction.CancelDispatch:
                            input.CancelDispatchData.Info = info;
                            await CancelDispatchForDriver(input.CancelDispatchData);
                            break;
                        case DriverApplicationAction.MarkDispatchComplete:
                            input.MarkDispatchCompleteData.Info = info;
                            await MarkDispatchCompleteInternal(input.MarkDispatchCompleteData);
                            break;
                        case DriverApplicationAction.ModifyDispatchTicket:
                            input.LoadDispatchData.Info = info;
                            await ModifyDispatchTicketInternal(input.LoadDispatchData);
                            break;
                        case DriverApplicationAction.CompleteDispatch:
                            input.CompleteDispatchData.Info = info;
                            await CompleteDispatchInternal(input.CompleteDispatchData);
                            break;
                        case DriverApplicationAction.AddSignature:
                            await AddSignature(input.AddSignatureData);
                            break;
                        case DriverApplicationAction.SaveDriverPushSubscription:
                            await SaveDriverPushSubscription(info, input.PushSubscriptionData);
                            break;
                        case DriverApplicationAction.RemoveDriverPushSubscription:
                            await RemoveDriverPushSubscription(info, input.PushSubscriptionData);
                            break;
                        case DriverApplicationAction.UploadDeferredBinaryObject:
                            await UploadDeferredBinaryObject(info, input.UploadDeferredData);
                            break;
                        case DriverApplicationAction.UploadLogs:
                            await UploadLogs(info, input.UploadLogsData);
                            break;
                        case DriverApplicationAction.ModifyEmployeeTime:
                            //todo need to implement for new flutter app.
                            throw new NotImplementedException();
                        //break;
                        case DriverApplicationAction.RemoveEmployeeTime:
                            //todo need to implement for new flutter app.
                            throw new NotImplementedException();
                        //break;
                        case DriverApplicationAction.AddDriverNote:
                            await AddDriverNote(input.AddDriverNoteData);
                            break;
                        default:
                            throw new ApplicationException("Received unexpected DriverApplicationAction");
                    }

                    return true;
                }
            });
        }

        [AbpAllowAnonymous]
        public async Task<List<DispatchCompleteInfoDto>> GetDriverDispatchesForCurrentUser(GetDriverDispatchesForCurrentUserInput input)
        {
            var authInfo = await _driverApplicationAuthProvider.AuthDriverByDriverGuidIfNeeded(input.DriverGuid);
            using (Session.Use(authInfo.TenantId, authInfo.UserId))
            {
                var allowProductionPay = await SettingManager.GetSettingValueAsync<bool>(AppSettings.TimeAndPay.AllowProductionPay);
                var requiredTicketEntry = await SettingManager.GetRequiredTicketEntry();

                var items = await (await _dispatchRepository.GetQueryAsync())
                        .WhereIf(input.UpdatedAfterDateTime.HasValue, d => d.CreationTime > input.UpdatedAfterDateTime.Value || (d.LastModificationTime != null && d.LastModificationTime > input.UpdatedAfterDateTime.Value))
                        .WhereIf(input.UpdatedAfterDateTime == null, d => d.Status != DispatchStatus.Canceled && d.Status != DispatchStatus.Completed)
                        .Where(d => d.DriverId == authInfo.DriverId && (Dispatch.OpenStatuses.Contains(d.Status) || d.Status == DispatchStatus.Canceled || d.Status == DispatchStatus.Completed))
                        .OrderByDescending(d => d.Status == DispatchStatus.Loaded)
                        .ThenByDescending(d => d.Status == DispatchStatus.Acknowledged)
                        .ThenByDescending(d => d.Status == DispatchStatus.Sent)
                        .ThenBy(d => d.SortOrder)
                .Select(di => new DispatchCompleteInfoDto
                {
                    CustomerName = di.OrderLine.Order.Customer.Name,
                    ContactName = di.OrderLine.RequiresCustomerNotification ? di.OrderLine.CustomerNotificationContactName : di.OrderLine.Order.CustomerContact.Name,
                    ContactPhoneNumber = di.OrderLine.RequiresCustomerNotification ? di.OrderLine.CustomerNotificationPhoneNumber : di.OrderLine.Order.CustomerContact.PhoneNumber,
                    Date = di.OrderLine.Order.DeliveryDate,
                    Shift = di.OrderLine.Order.Shift,
                    FreightItemName = di.OrderLine.FreightItem.Name,
                    MaterialItemName = di.OrderLine.MaterialItem.Name,
                    Designation = di.OrderLine.Designation,
                    TimeOnJobUtc = di.TimeOnJob,
                    TruckCode = di.Truck.TruckCode,
                    TrailerTruckCode = di.OrderLineTruck.Trailer.TruckCode,
                    LoadAtName = di.OrderLine.LoadAt.Name,
                    LoadAt = di.OrderLine.LoadAt == null ? null : new LocationAddressDto
                    {
                        StreetAddress = di.OrderLine.LoadAt.StreetAddress,
                        City = di.OrderLine.LoadAt.City,
                        State = di.OrderLine.LoadAt.State,
                        ZipCode = di.OrderLine.LoadAt.ZipCode,
                        CountryCode = di.OrderLine.LoadAt.CountryCode,
                    },
                    LoadAtLatitude = di.OrderLine.LoadAt.Latitude,
                    LoadAtLongitude = di.OrderLine.LoadAt.Longitude,
                    ChargeTo = di.OrderLine.Order.ChargeTo,
                    //LoadId = withTicket ? di.Load != null ? di.Load.Id : (int?)null : null,
                    //TicketNumber = withTicket ? di.Load != null ? di.Load.TicketNumber : null : null,
                    //MaterialAmount = withTicket ? di.Load != null ? di.Load.TicketMaterialQuantity : 0 : 0,
                    //FreightAmount = withTicket ? di.Load != null ? di.Load.TicketFreightQuantity : 0 : 0,
                    MaterialUomName = di.OrderLine.MaterialUom.Name,
                    FreightUomName = di.OrderLine.FreightUom.Name,
                    MaterialQuantity = di.MaterialQuantity,
                    FreightQuantity = di.FreightQuantity,
                    JobNumber = di.OrderLine.JobNumber,
                    Note = di.Note,
                    IsMultipleLoads = di.IsMultipleLoads,
                    WasMultipleLoads = di.WasMultipleLoads,
                    AcknowledgedDateTimeUtc = di.Acknowledged,
                    LastLoad = di.Loads
                        .OrderByDescending(l => l.Id)
                        .Select(l => new DispatchCompleteInfoLoadDto
                        {
                            Id = l.Id,
                            SignatureId = l.SignatureId,
                            SourceDateTime = l.SourceDateTime,
                            LastTicket = l.Tickets
                                .OrderByDescending(t => t.Id)
                                .Select(t => new DispatchCompleteInfoTicketDto
                                {
                                    TicketNumber = t.TicketNumber,
                                    FreightQuantity = t.FreightQuantity,
                                    MaterialQuantity = t.MaterialQuantity,
                                    MaterialItemId = t.MaterialItemId,
                                    MaterialItemName = t.MaterialItem.Name,
                                    FreightItemId = t.FreightItemId,
                                    FreightItemName = t.FreightItem.Name,
                                    LoadCount = t.LoadCount,
                                    TicketPhotoId = t.TicketPhotoId,
                                })
                                .FirstOrDefault(),
                        })
                        .FirstOrDefault(),
                    HasTickets = di.Loads.Any(l => l.Tickets.Any()),
                    DeliverToName = di.OrderLine.DeliverTo.Name,
                    DeliverTo = di.OrderLine.DeliverTo == null ? null : new LocationAddressDto
                    {
                        StreetAddress = di.OrderLine.DeliverTo.StreetAddress,
                        City = di.OrderLine.DeliverTo.City,
                        State = di.OrderLine.DeliverTo.State,
                        ZipCode = di.OrderLine.DeliverTo.ZipCode,
                        CountryCode = di.OrderLine.DeliverTo.CountryCode,
                    },
                    DeliverToLatitude = di.OrderLine.DeliverTo.Latitude,
                    DeliverToLongitude = di.OrderLine.DeliverTo.Longitude,
                    DispatchStatus = di.Status,
                    DispatchId = di.Id,
                    Guid = di.Guid,
                    TenantId = di.TenantId,
                    LastUpdateDateTime = di.LastModificationTime.HasValue && di.LastModificationTime.Value > di.CreationTime ? di.LastModificationTime.Value : di.CreationTime,
                    Id = di.Id,
                    SortOrder = di.SortOrder,
                    NumberOfAddedLoads = di.NumberOfAddedLoads,
                    NumberOfLoadsToFinish = di.NumberOfLoadsToFinish,
                    ProductionPay = allowProductionPay && di.OrderLine.ProductionPay,
                    RequireTicket = di.OrderLine.RequireTicket,
                    OrderLineTruckId = di.OrderLineTruckId,
                    MaterialItemId = di.OrderLine.MaterialItemId,
                    FreightItemId = di.OrderLine.FreightItemId,
                    MaterialUomId = di.OrderLine.MaterialUomId,
                    FreightUomId = di.OrderLine.FreightUomId,
                }).ToListAsync(CancellationTokenProvider.Token);

                var separateItems = await FeatureChecker.IsEnabledAsync(AppFeatures.SeparateMaterialAndFreightItems);
                foreach (var dispatch in items)
                {
                    dispatch.QuantityWithItem = OrderItemFormatter.GetItemWithQuantityFormatted(dispatch);
                    dispatch.VisibleTicketControls = TicketQuantityHelper.GetVisibleTicketControls(separateItems, new TicketOrderLineDetailsDto
                    {
                        Designation = dispatch.Designation,
                        FreightQuantity = dispatch.FreightQuantity,
                        MaterialQuantity = dispatch.MaterialQuantity,
                        FreightUomId = dispatch.FreightUomId,
                        FreightUomBaseId = null, //not used for this method
                        MaterialUomId = dispatch.MaterialUomId,
                        MaterialItemId = dispatch.MaterialItemId,
                        FreightItemId = dispatch.FreightItemId,
                        CalculateMinimumFreightAmount = false, //not used for this method
                        MinimumFreightAmount = 0, //not used for this method
                    });
                    dispatch.RequireTicket = IsTicketRequired(dispatch.RequireTicket, requiredTicketEntry);
                }

                return items;
            }
        }

        [AbpAllowAnonymous]
        public async Task<List<OrderLineTruckInfoDto>> GetOrderLineTrucksForCurrentUser(GetOrderLineTrucksForCurrentUserInput input)
        {
            var authInfo = await _driverApplicationAuthProvider.AuthDriverByDriverGuidIfNeeded(input.DriverGuid);
            using (Session.Use(authInfo.TenantId, authInfo.UserId))
            {
                var ids = !input.UpdatedAfterDateTime.HasValue
                    ? (await GetDriverDispatchesForCurrentUser(new GetDriverDispatchesForCurrentUserInput
                    {
                        DriverGuid = input.DriverGuid,
                    })).Select(x => x.OrderLineTruckId).Distinct().ToList()
                    : new();

                var items = await (await _orderLineTruckRepository.GetQueryAsync())
                    .WhereIf(input.UpdatedAfterDateTime.HasValue, olt =>
                        olt.CreationTime > input.UpdatedAfterDateTime.Value
                        || (olt.LastModificationTime != null && olt.LastModificationTime > input.UpdatedAfterDateTime.Value))
                    .WhereIf(!input.UpdatedAfterDateTime.HasValue, olt => ids.Contains(olt.Id))
                    .Where(d => d.DriverId == authInfo.DriverId)
                    .Select(x => new OrderLineTruckInfoDto
                    {
                        Id = x.Id,
                        DriverNote = x.DriverNote,
                        LastUpdateDateTime = x.LastModificationTime.HasValue && x.LastModificationTime.Value > x.CreationTime ? x.LastModificationTime.Value : x.CreationTime,
                    }).ToListAsync(CancellationTokenProvider.Token);

                return items;
            }
        }

        private async Task LoadDispatchInternal(LoadDispatchInput input)
        {
            if (input.Id == null && input.Guid == null)
            {
                throw new ArgumentException("Id or Guid must be provided", nameof(input));
            }

            if (input.Guid != null)
            {
                Logger.Warn("LoadDispatchInternal received Guid in input. If this message still gets logged, it is too early to remove Guid support");
            }

            var dispatchQuery = (await _dispatchRepository.GetQueryAsync())
                .WhereIf(input.Guid.HasValue, d => d.Guid == input.Guid) //deprecated, temporarily kept for backwards compatibility
                .WhereIf(input.Id.HasValue, d => d.Id == input.Id);

            var dispatchEntity = await dispatchQuery
                .FirstOrDefaultAsync(CancellationTokenProvider.Token);

            var dispatchData = await dispatchQuery
                .Select(d => new
                {
                    d.OrderLine.OrderId,
                }).FirstOrDefaultAsync(CancellationTokenProvider.Token);

            if (dispatchEntity == null || dispatchData == null)
            {
                return;
            }

            if (dispatchEntity.Status == DispatchStatus.Canceled)
            {
                if (input.Info == null)
                {
                    //request didn't come from driver application
                    return;
                }
            }

            Load loadEntity;
            if (input.IsEdit)
            {
                loadEntity = await (await _loadRepository.GetQueryAsync())
                    .Include(l => l.Tickets)
                    .Where(l => l.DispatchId == dispatchEntity.Id)
                    .OrderByDescending(l => l.Id)
                    .FirstOrDefaultAsync(CancellationTokenProvider.Token);
            }
            else if (input.LoadId != null)
            {
                loadEntity = await (await _loadRepository.GetQueryAsync())
                    .Include(l => l.Tickets)
                    .Where(l => l.Id == input.LoadId && l.DispatchId == dispatchEntity.Id)
                    .OrderByDescending(l => l.Id)
                    .FirstOrDefaultAsync(CancellationTokenProvider.Token);
            }
            else
            {
                loadEntity = await (await _loadRepository.GetQueryAsync())
                    .Include(l => l.Tickets)
                    .Where(l => l.DispatchId == dispatchEntity.Id && l.SourceDateTime == null)
                    .OrderByDescending(l => l.Id)
                    .FirstOrDefaultAsync(CancellationTokenProvider.Token) ?? new Load { DispatchId = dispatchEntity.Id };
            }

            loadEntity.SourceLatitude = input.SourceLatitude;
            loadEntity.SourceLongitude = input.SourceLongitude;

            var ticket = loadEntity.Tickets.LastOrDefault();

            if (dispatchEntity.Status == DispatchStatus.Acknowledged)
            {
                await ChangeDispatchStatusToLoadedAsync(dispatchEntity, loadEntity, input.Info);
            }
            else
            {
                Logger.Warn($"[Dispatching] DriverId {input.Info?.DriverId} ({AbpSession.UserId}) tried to load dispatchId {dispatchEntity.Id} but it is in status {dispatchEntity.Status}");
                loadEntity.SourceDateTime ??= input.Info?.ActionTimeInUtc ?? Clock.Now;
                if (ticket != null)
                {
                    ticket.TicketDateTime ??= input.Info?.ActionTimeInUtc ?? Clock.Now;
                }
            }

            await _loadRepository.InsertOrUpdateAsync(loadEntity);
            await CurrentUnitOfWork.SaveChangesAsync();

            await _orderTaxCalculator.CalculateTotalsAsync(dispatchData.OrderId);
            if (ticket != null)
            {
                await _fuelSurchargeCalculator.RecalculateTicket(ticket.Id);
            }

            await CurrentUnitOfWork.SaveChangesAsync();

            await _syncRequestSender.SendSyncRequest(new SyncRequest()
                .AddChange(EntityEnum.Dispatch, dispatchEntity.ToChangedEntity())
                .SetIgnoreForDeviceId(input.Info?.DeviceId)
                .AddLogMessage("Loaded dispatch"));
        }

        private async Task ModifyDispatchTicketInternal(LoadDispatchInput input)
        {
            if (input.Id == null && input.Guid == null)
            {
                throw new ArgumentException("Id or Guid must be provided", nameof(input));
            }

            if (input.Guid != null)
            {
                Logger.Warn("ModifyDispatchTicketInternal received Guid in input. If this message still gets logged, it is too early to remove Guid support");
            }

            await UploadTicketPhotoIfNeeded(input);

            var dispatchQuery = (await _dispatchRepository.GetQueryAsync())
                .WhereIf(input.Guid.HasValue, d => d.Guid == input.Guid) //deprecated, temporarily kept for backwards compatibility
                .WhereIf(input.Id.HasValue, d => d.Id == input.Id);

            var dispatchEntity = await dispatchQuery
                .FirstOrDefaultAsync(CancellationTokenProvider.Token);

            var dispatchData = await dispatchQuery
                .Select(d => new
                {
                    d.OrderLine.OrderId,
                    d.OrderLine.Order.OfficeId,
                    d.OrderLine.LoadAtId,
                    d.OrderLine.DeliverToId,
                    d.OrderLine.FreightItemId,
                    d.OrderLine.MaterialItemId,
                    d.OrderLine.Designation,
                    d.OrderLine.MaterialUomId,
                    d.OrderLine.FreightUomId,
                    d.Truck.TruckCode,
                    d.OrderLineTruck.TrailerId,
                    d.OrderLine.Order.CustomerId,
                }).FirstOrDefaultAsync(CancellationTokenProvider.Token);

            if (dispatchEntity == null || dispatchData == null)
            {
                return;
            }

            if (dispatchEntity.Status == DispatchStatus.Canceled)
            {
                Logger.Warn($"[Dispatching] DriverId {input.Info?.DriverId} ({AbpSession.UserId}) modified ticket for dispatchGuid {input.Guid}/{input.Id} ({dispatchEntity.Id}) but it is canceled");
                if (input.Info == null)
                {
                    //request didn't come from driver application
                    return;
                }
            }

            using (CurrentUnitOfWork.SetTenantId(dispatchEntity.TenantId))
            using (await UseExistingSessionOrFallbackToDispatchUserAsync(dispatchEntity))
            {
                Load loadEntity;
                var loadQuery = (await _loadRepository.GetQueryAsync())
                    .Include(l => l.Tickets)
                    .Where(l => l.DispatchId == dispatchEntity.Id)
                    .OrderByDescending(l => l.Id);

                if (input.IsEdit || dispatchEntity.Status == DispatchStatus.Loaded || input.DispatchStatus == DispatchStatus.Loaded)
                {
                    loadEntity = await loadQuery
                        .FirstOrDefaultAsync(CancellationTokenProvider.Token);
                }
                else if (input.LoadId != null)
                {
                    loadEntity = await loadQuery
                        .Where(l => l.Id == input.LoadId)
                        .FirstOrDefaultAsync(CancellationTokenProvider.Token);
                }
                else
                {
                    loadEntity = await loadQuery
                        .Where(l => l.SourceDateTime == null)
                        .FirstOrDefaultAsync(CancellationTokenProvider.Token)
                            ?? new Load { DispatchId = dispatchEntity.Id };
                }

                var orderTotalsBeforeUpdate = await GetOrderTotalsAsync(dispatchEntity.OrderLineId);

                var ticket = loadEntity.Tickets.LastOrDefault();
                if (ticket == null)
                {
                    ticket = new Ticket
                    {
                        OrderLineId = dispatchEntity.OrderLineId,
                        OfficeId = dispatchData.OfficeId,
                        LoadAtId = dispatchData.LoadAtId,
                        DeliverToId = dispatchData.DeliverToId,
                        TruckId = dispatchEntity.TruckId,
                        TruckCode = dispatchData.TruckCode,
                        TrailerId = dispatchData.TrailerId,
                        CustomerId = dispatchData.CustomerId,
                        DriverId = dispatchEntity.DriverId,
                        TicketDateTime = input.Info?.ActionTimeInUtc ?? Clock.Now,
                        NonbillableFreight = !dispatchData.Designation.HasFreight(),
                        NonbillableMaterial = !dispatchData.Designation.HasMaterial(),
                        TenantId = await AbpSession.GetTenantIdAsync(),
                    };
                    loadEntity.Tickets.Add(ticket);

                    var truckDetails = await (await _truckRepository.GetQueryAsync())
                        .Where(x => x.Id == ticket.TruckId)
                        .Select(x => new
                        {
                            LeaseHaulerId = (int?)x.LeaseHaulerTruck.LeaseHaulerId,
                        }).FirstOrDefaultAsync(CancellationTokenProvider.Token);
                    ticket.CarrierId = truckDetails?.LeaseHaulerId;
                }
                if (!input.CreateNewTicket)
                {
                    ticket.TicketNumber = input.TicketNumber;
                }

                if (input.Amount > 0)
                {
                    input.Quantity ??= input.MaterialQuantity ?? input.Amount;
                    input.MaterialQuantity ??= input.Amount;
                }
                else if (input.MaterialQuantity == null && input.Quantity > 0)
                {
                    input.MaterialQuantity = input.Quantity;
                }
                await _ticketQuantityHelper.SetTicketQuantity(ticket, input);

                ticket.LoadCount = input.LoadCount;

                if (input.TicketPhotoId.HasValue)
                {
                    ticket.TicketPhotoId = input.TicketPhotoId;
                    ticket.TicketPhotoFilename = input.TicketPhotoFilename;
                }
                if (input.DeferredPhotoId.HasValue)
                {
                    var existingDeferredDataId = await PopDataIdFromExistingDeferred(input.DeferredPhotoId.Value);
                    if (existingDeferredDataId != null)
                    {
                        ticket.TicketPhotoId = existingDeferredDataId;
                    }
                    else
                    {
                        ticket.DeferredTicketPhotoId = input.DeferredPhotoId;
                    }
                    ticket.TicketPhotoFilename = input.TicketPhotoFilename;
                }

                await _loadRepository.InsertOrUpdateAsync(loadEntity);
                await CurrentUnitOfWork.SaveChangesAsync();

                if (input.CreateNewTicket)
                {
                    ticket.TicketNumber = "G-" + ticket.Id;
                }

                ticket.IsInternal = input.CreateNewTicket || input.TicketNumber == "G-" + ticket.Id;

                await CurrentUnitOfWork.SaveChangesAsync();
                await _orderTaxCalculator.CalculateTotalsAsync(dispatchData.OrderId);
                await _fuelSurchargeCalculator.RecalculateTicket(ticket.Id);

                await CurrentUnitOfWork.SaveChangesAsync();

                await _syncRequestSender.SendSyncRequest(new SyncRequest()
                    .AddChange(EntityEnum.Dispatch, dispatchEntity.ToChangedEntity())
                    .SetIgnoreForDeviceId(input.Info?.DeviceId)
                    .AddLogMessage("Modified dispatch ticket"));

                await NotifyDispatchersAfterTicketUpdateIfNeeded(dispatchEntity.OrderLineId, orderTotalsBeforeUpdate);
            }
        }

        [RemoteService(false)]
        public async Task NotifyDispatchersAfterTicketUpdateIfNeeded(int orderLineId, GetOrderTotalsResult orderTotalsBeforeUpdate)
        {
            var orderTotalsAfterUpdate = await GetOrderTotalsAsync(orderLineId);

            if (!orderTotalsBeforeUpdate.ActualAmountReachedOrderedQuantity && orderTotalsAfterUpdate.ActualAmountReachedOrderedQuantity)
            {
                var orderDetails = await (await _orderLineRepository.GetQueryAsync())
                    .Where(x => x.Id == orderLineId)
                    .Select(x => new
                    {
                        OfficeId = x.Order.OfficeId,
                        CustomerName = x.Order.Customer.Name,
                    })
                    .FirstOrDefaultAsync(CancellationTokenProvider.Token);

                await _appNotifier.SendPriorityNotification(
                    new SendPriorityNotificationInput(
                        L("OrderHasReachedRequestedAmount").Replace("{CustomerName}", orderDetails.CustomerName),
                        NotificationSeverity.Warn,
                        [orderDetails.OfficeId]
                    )
                    {
                        OnlineFilter = true,
                        RoleFilter = new[] { StaticRoleNames.Tenants.Dispatching },
                    });
            }
        }

        [RemoteService(false)]
        public async Task<GetOrderTotalsResult> GetOrderTotalsAsync(int orderLineId)
        {
            var data = await (await _orderLineRepository.GetQueryAsync())
                    .Where(x => x.Id == orderLineId)
                    .Select(ol => new
                    {
                        ol.FreightQuantity,
                        ol.MaterialQuantity,
                        ol.Designation,
                        ol.Order.DeliveryDate,
                        Tickets = ol.Tickets.Select(x => new
                        {
                            x.FreightQuantity,
                            x.MaterialQuantity,
                            x.TicketDateTime,
                        }).ToList(),
                    }).FirstOrDefaultAsync(CancellationTokenProvider.Token);

            if (data == null)
            {
                return null;
            }

            var timezone = await GetTimezone();
            data.Tickets.RemoveAll(ticket => ticket.TicketDateTime?.ConvertTimeZoneTo(timezone).Date != data.DeliveryDate);

            var result = new GetOrderTotalsResult();

            if (data.Designation.HasFreight())
            {
                result.FreightQuantity = data.FreightQuantity;
                result.FreightActualAmount = data.Tickets.Sum(t => t.FreightQuantity);
            }

            if (data.Designation.HasMaterial())
            {
                result.MaterialQuantity = data.MaterialQuantity;
                result.MaterialActualAmount = data.Tickets.Sum(t => t.MaterialQuantity);
            }

            return result;
        }

        private async Task AddSignature(AddSignatureInput input)
        {
            if (input.DispatchId == null && input.Guid == null)
            {
                throw new ArgumentException("Id or Guid must be provided", nameof(input));
            }

            var dispatchEntity = await (await _dispatchRepository.GetQueryAsync())
                .WhereIf(input.Guid.HasValue, d => d.Guid == input.Guid) //deprecated, temporarily kept for backwards compatibility
                .WhereIf(input.DispatchId.HasValue, d => d.Id == input.DispatchId)
                .FirstOrDefaultAsync(CancellationTokenProvider.Token);

            if (input.Guid != null)
            {
                Logger.Warn("AddSignature received Guid in input. If this message still gets logged, it is too early to remove Guid support");
            }

            if (dispatchEntity == null)
            {
                //throw new UserFriendlyException("Dispatch wasn't found");
                return;
            }

            var loadEntity = await (await _loadRepository.GetQueryAsync())
                .Where(l => l.DispatchId == dispatchEntity.Id && l.DestinationDateTime == null)
                .OrderByDescending(l => l.Id)
                .FirstOrDefaultAsync(CancellationTokenProvider.Token);

            if (loadEntity == null)
            {
                return;
            }

            if (!input.Signature.IsNullOrEmpty())
            {
                loadEntity.SignatureName = input.SignatureName;
                loadEntity.SignatureId = await _binaryObjectManager.UploadDataUriStringAsync(input.Signature, await AbpSession.GetTenantIdOrNullAsync());
            }

            if (!input.SignatureName.IsNullOrEmpty())
            {
                loadEntity.SignatureName = input.SignatureName;
            }

            if (input.DeferredSignatureId != null)
            {
                var existingDeferredDataId = await PopDataIdFromExistingDeferred(input.DeferredSignatureId.Value);
                if (existingDeferredDataId != null)
                {
                    loadEntity.SignatureId = existingDeferredDataId;
                }
                else
                {
                    loadEntity.DeferredSignatureId = input.DeferredSignatureId;
                }
            }
        }

        private async Task AddDriverNote(AddDriverNoteInput input)
        {
            var orderLineTruck = await (await _orderLineTruckRepository.GetQueryAsync())
                .Where(olt => olt.Id == input.OrderLineTruckId)
                .FirstOrDefaultAsync(CancellationTokenProvider.Token);

            if (orderLineTruck != null)
            {
                orderLineTruck.DriverNote = input.DriverNote;
            }
            else
            {
                Logger.Warn($"Unable to add driver note '${input.DriverNote}' to OrderLineTruck ${input.OrderLineTruckId} because OLT wasn't found");
            }
        }

        private async Task<Guid?> PopDataIdFromExistingDeferred(Guid deferredId)
        {
            var existingDeferred = await (await _deferredBinaryObjectRepository.GetQueryAsync())
                .Where(x => x.Id == deferredId)
                .FirstOrDefaultAsync(CancellationTokenProvider.Token);

            if (existingDeferred == null)
            {
                return null;
            }

            var dataId = existingDeferred.BinaryObjectId;

            await _deferredBinaryObjectRepository.DeleteAsync(existingDeferred);

            return dataId;
        }



        private async Task<CompleteDispatchResult> CompleteDispatchInternal(CompleteDispatchDto input)
        {
            if (input.Id == null && input.Guid == null)
            {
                throw new ArgumentException("Id or Guid must be provided", nameof(input));
            }

            if (input.Guid != null)
            {
                Logger.Warn("CompleteDispatchInternal received Guid in input. If this message still gets logged, it is too early to remove Guid support");
            }

            var dispatchEntity = await (await _dispatchRepository.GetQueryAsync())
                .WhereIf(input.Guid.HasValue, d => d.Guid == input.Guid) //deprecated, temporarily kept for backwards compatibility
                .WhereIf(input.Id.HasValue, d => d.Id == input.Id)
                .FirstOrDefaultAsync(CancellationTokenProvider.Token);

            if (dispatchEntity == null)
            {
                return new CompleteDispatchResult { NotFound = true };
            }

            if (dispatchEntity.Status == DispatchStatus.Canceled)
            {
                Logger.Warn(
                    $"[Dispatching] DriverId {input.Info?.DriverId} ({AbpSession.UserId}) completed dispatch {dispatchEntity.Id} but it was canceled");
                if (input.Info == null)
                {
                    //request didn't come from driver application
                    return new CompleteDispatchResult
                    {
                        IsCanceled = true,
                    };
                }
            }
            else if (dispatchEntity.Status == DispatchStatus.Completed)
            {
                return new CompleteDispatchResult
                {
                    IsCompleted = true,
                };
            }

            var loadEntity = await (await _loadRepository.GetQueryAsync())
                .Where(l => l.DispatchId == dispatchEntity.Id && l.DestinationDateTime == null)
                .OrderByDescending(l => l.Id)
                .FirstOrDefaultAsync(CancellationTokenProvider.Token);

            if (loadEntity == null)
            {
                Logger.Warn($"[Dispatching] Dispatch {dispatchEntity.Id}: Load with null DestinationDateTime wasn't found.");
                if (dispatchEntity.Status == DispatchStatus.Canceled)
                {
                    loadEntity = await (await _loadRepository.GetQueryAsync())
                        .Where(l => l.DispatchId == dispatchEntity.Id)
                        .OrderByDescending(x => x.Id)
                        .FirstOrDefaultAsync(CancellationTokenProvider.Token);

                    if (loadEntity == null)
                    {
                        Logger.Warn($"[Dispatching] Dispatch {dispatchEntity.Id}: No loads exist for dispatch. Exiting CompleteDispatch");
                        return new CompleteDispatchResult { IsCanceled = true };
                    }
                    //otherwise we continue with the last completed load and overwrite its values with the values from the driver
                }
                else
                {
                    Logger.Error($"[Dispatching] Dispatch {dispatchEntity.Id}: No loads with null DestinationDateTime exist for dispatch. Exiting CompleteDispatch");
                    return new CompleteDispatchResult { NotFound = true };
                }
            }

            loadEntity.DestinationLatitude = input.DestinationLatitude;
            loadEntity.DestinationLongitude = input.DestinationLongitude;
            CompleteDispatchResult result;
            if (!(input.IsMultipleLoads ?? dispatchEntity.IsMultipleLoads) || input.ContinueMultiload != true || dispatchEntity.Status == DispatchStatus.Canceled)
            {
                if (dispatchEntity.IsMultipleLoads && input.ContinueMultiload == false)
                {
                    UncheckMultipleLoads(dispatchEntity);
                }

                await ChangeDispatchStatusToCompleted(dispatchEntity, loadEntity, input.Info);

                result = new CompleteDispatchResult();
            }
            else
            {
                ChangeMultipleLoadsDispatchStatusToAcknowledged(dispatchEntity, loadEntity, input.Info);
                dispatchEntity.Loads.Add(new Load());
                await CurrentUnitOfWork.SaveChangesAsync();
                result = new CompleteDispatchResult
                {
                    NextDispatchId = dispatchEntity.Id,
                };
            }

            await CurrentUnitOfWork.SaveChangesAsync();

            await _syncRequestSender.SendSyncRequest(new SyncRequest()
                .AddChange(EntityEnum.Dispatch, dispatchEntity.ToChangedEntity(), input.ContinueMultiload == true ? ChangeType.Modified : ChangeType.Removed)
                .SetIgnoreForDeviceId(input.Info?.DeviceId)
                .AddLogMessage("Completed dispatch"));

            return result;
        }








        private async Task CancelDispatchForDriver(CancelDispatchForDriverInput input)
        {
            var dispatchEntity = await (await _dispatchRepository.GetQueryAsync())
                .Include(x => x.Loads)
                    .ThenInclude(x => x.Tickets)
                .Where(x => x.Id == input.DispatchId)
                .FirstOrDefaultAsync(CancellationTokenProvider.Token);

            if (dispatchEntity == null || dispatchEntity.Status == DispatchStatus.Completed || dispatchEntity.Status == DispatchStatus.Canceled)
            {
                return;
            }

            try
            {
                if (!await IsAllowedToCancelDispatch(input.DispatchId))
                {
                    dispatchEntity.Status = DispatchStatus.Completed;
                    return;
                }

                dispatchEntity.Status = DispatchStatus.Canceled;
                dispatchEntity.Canceled = input.Info.ActionTimeInUtc;

                if (!dispatchEntity.WasMultipleLoads)
                {
                    if (dispatchEntity.Loads != null)
                    {
                        foreach (var load in dispatchEntity.Loads)
                        {
                            if (!load.Tickets.Any())
                            {
                                await _loadRepository.DeleteAsync(load);
                            }
                        }
                    }

                }
            }
            finally
            {
                await CurrentUnitOfWork.SaveChangesAsync();

                //await _driverApplicationPushSender.SendPushMessageToDrivers(new SendPushMessageToDriversInput(dispatchEntity.DriverId)
                //{
                //    LogMessage = $"Canceled dispatch {dispatchEntity.Id}"
                //});
                await _syncRequestSender.SendSyncRequest(new SyncRequest()
                    .AddChange(EntityEnum.Dispatch, dispatchEntity.ToChangedEntity(), ChangeType.Removed)
                    .SetIgnoreForDeviceId(input.Info?.DeviceId)
                    .AddLogMessage("Canceled dispatch"));
            }
        }

        private async Task MarkDispatchCompleteInternal(MarkDispatchCompleteInput input)
        {
            var dispatchEntity = await (await _dispatchRepository.GetQueryAsync())
                .Where(x => x.Id == input.DispatchId)
                .FirstOrDefaultAsync(CancellationTokenProvider.Token);

            if (dispatchEntity == null || dispatchEntity.Status == DispatchStatus.Completed || dispatchEntity.Status == DispatchStatus.Canceled)
            {
                return;
            }

            dispatchEntity.Status = DispatchStatus.Completed;

            await CurrentUnitOfWork.SaveChangesAsync();

            await _syncRequestSender.SendSyncRequest(new SyncRequest()
                .AddChange(EntityEnum.Dispatch, dispatchEntity.ToChangedEntity(), ChangeType.Removed)
                .SetIgnoreForDeviceId(input.Info?.DeviceId)
                .AddLogMessage("Marked dispatch complete"));
        }

        public static bool IsTicketRequired(bool requireTicket, RequiredTicketEntryEnum requiredTicketEntry)
        {
            switch (requiredTicketEntry)
            {
                case RequiredTicketEntryEnum.None:
                    return false;
                case RequiredTicketEntryEnum.Always:
                    return true;
                default:
                    return requireTicket;
            }
        }

        private async Task SaveDriverPushSubscription(DriverApplicationActionInfo info, ModifyDriverPushSubscriptionInput input)
        {
            await _pushSubscriptionManager.AddDriverPushSubscription(new AddDriverPushSubscriptionInput
            {
                PushSubscription = input?.PushSubscription,
                DriverId = info.DriverId,
                DeviceId = info.DeviceId,
            });
        }

        private async Task RemoveDriverPushSubscription(DriverApplicationActionInfo info, ModifyDriverPushSubscriptionInput input)
        {
            await _pushSubscriptionManager.RemoveDriverPushSubscription(new RemoveDriverPushSubscriptionInput
            {
                PushSubscription = input?.PushSubscription,
                DriverId = info.DriverId,
                DeviceId = info.DeviceId,
            });
        }

        private static LogLevel? GetLogLevelEnum(string logLevel)
        {
            switch (logLevel?.ToLower())
            {
                case "info": return LogLevel.Information;
                case "warn": return LogLevel.Warning;
                case "error": return LogLevel.Error;
                case "debug": return LogLevel.Debug;
                case "critical": return LogLevel.Critical;
                case "trace": return LogLevel.Trace;
            }
            return null;
        }

        private async Task UploadLogs(DriverApplicationActionInfo info, List<UploadLogsInput> inputList)
        {
            var i = 0;
            foreach (var input in inputList)
            {
                await _driverApplicationLogRepository.InsertAsync(new DriverApplicationLog
                {
                    OriginalLogId = input.Id,
                    ServiceWorker = input.Sw,
                    BatchOrder = i++, //in case the records in a batch get their ids out of order when saved, we can then filter by DriverId and order by (DateTime, OrderInBatch) to get the original order of log records
                    DateTime = input.DateTime, //.ConvertTimeZoneFrom(info.TimeZone),
                    DriverId = info.DriverId,
                    Level = input.LogLevel ?? GetLogLevelEnum(input.Level) ?? LogLevel.None,
                    Message = input.Message,
                    AppVersion = input.AppVersion,
                    TenantId = info.TenantId,
                    UserId = info.UserId,
                    DeviceId = info.DeviceId,
                    DeviceGuid = info.DeviceGuid,
                });
            }
            await CurrentUnitOfWork.SaveChangesAsync();
        }

        [AbpAllowAnonymous]
        public async Task UploadAnonymousLogs(UploadAnonymousLogsInput input)
        {
            var i = 0;
            foreach (var log in input.UploadLogsData)
            {
                var logDateTime = log.DateTime;
                if (input.TimezoneOffset.HasValue)
                {
                    logDateTime = logDateTime.AddMinutes(input.TimezoneOffset.Value);
                }

                await _driverApplicationLogRepository.InsertAsync(new DriverApplicationLog
                {
                    OriginalLogId = log.Id,
                    ServiceWorker = log.Sw,
                    BatchOrder = i++, //in case the records in a batch get their ids out of order when saved, we can then filter by DriverId and order by (DateTime, OrderInBatch) to get the original order of log records
                    DateTime = logDateTime,
                    DriverId = null,
                    Level = log.LogLevel ?? GetLogLevelEnum(log.Level) ?? LogLevel.None,
                    Message = log.Message,
                    AppVersion = log.AppVersion,
                    TenantId = null,
                    UserId = null,
                    DeviceId = input.DeviceId,
                    DeviceGuid = input.DeviceGuid,
                });
            }
            await CurrentUnitOfWork.SaveChangesAsync();
        }

        private async Task UploadDeferredBinaryObject(DriverApplicationActionInfo info, UploadDeferredBinaryObjectInput input)
        {
            var dataId = await _binaryObjectManager.UploadDataUriStringAsync(input.BytesString, info.TenantId);
            if (dataId == null)
            {
                return;
            }
            switch (input.Destination)
            {
                case DeferredBinaryObjectDestination.TicketPhoto:
                    var existingTickets = await (await _ticketRepository.GetQueryAsync()).Where(x => x.DeferredTicketPhotoId == input.DeferredId).ToListAsync(CancellationTokenProvider.Token);
                    if (existingTickets.Any())
                    {
                        existingTickets.ForEach(x =>
                        {
                            x.TicketPhotoId = dataId;
                            x.DeferredTicketPhotoId = null;
                        });
                        return;
                    }
                    break;
                case DeferredBinaryObjectDestination.LoadSignature:
                    var existingLoads = await (await _loadRepository.GetQueryAsync()).Where(x => x.DeferredSignatureId == input.DeferredId).ToListAsync(CancellationTokenProvider.Token);
                    if (existingLoads.Any())
                    {
                        existingLoads.ForEach(x =>
                        {
                            x.SignatureId = dataId;
                            x.DeferredSignatureId = null;
                        });
                        return;
                    }
                    break;
                default:
                    Logger.Error($"UploadDeferredBinaryObject received and unexpected Destination (${input.Destination.ToIntString()}) from the DriverApplication. DriverId: {info.DriverId}; DeviceId: {info.DeviceId}");
                    return;
            }

            var existingDeferred = await (await _deferredBinaryObjectRepository.GetQueryAsync()).FirstOrDefaultAsync(x => x.Id == input.DeferredId,
                                           CancellationTokenProvider.Token);
            if (existingDeferred != null)
            {
                var message = $"UploadDeferredBinaryObject: overwriting DeferredBinaryObject.Id {input.DeferredId} (BinaryObjectId, Destination, TenantId): \nwas ({existingDeferred.BinaryObjectId}, {existingDeferred.Destination}, {existingDeferred.TenantId}), \nnew ({dataId.Value}, {input.Destination}, {info.TenantId}); \nDriverId: {info.DriverId}; DeviceId: {info.DeviceId}";
                Logger.Warn(message);
                await _driverApplicationLogger.LogWarn(info.DriverId, message);
                existingDeferred.BinaryObjectId = dataId.Value;
                existingDeferred.Destination = input.Destination;
                existingDeferred.TenantId = info.TenantId;
            }
            else
            {
                await _deferredBinaryObjectRepository.InsertAsync(new DeferredBinaryObject
                {
                    Id = input.DeferredId,
                    BinaryObjectId = dataId.Value,
                    Destination = input.Destination,
                    TenantId = info.TenantId,
                });
            }
        }

        private async Task DriverClockIn(DriverApplicationActionInfo info, DriverClockInInput input)
        {
            var userDate = info.ActionTimeInUtc.ConvertTimeZoneTo(info.TimeZone).Date;
            var userDayBeginningInUtc = userDate.ConvertTimeZoneFrom(info.TimeZone);

            //NotEndedTodayEmployeeTimeExists
            if (await (await _employeeTimeRepository.GetQueryAsync())
                    .AnyAsync(et => et.UserId == info.UserId && et.StartDateTime >= userDayBeginningInUtc && et.EndDateTime == null && !et.IsImported,
                         CancellationTokenProvider.Token))
            {
                return;
            }

            var driver = await (await _driverRepository.GetQueryAsync())
                .Where(x => x.Id == info.DriverId)
                .Select(x => new
                {
                    x.IsExternal,
                }).FirstOrDefaultAsync(CancellationTokenProvider.Token);

            var allowSubcontractorsToDriveCompanyOwnedTrucks = await SettingManager.GetSettingValueAsync<bool>(AppSettings.LeaseHaulers.AllowSubcontractorsToDriveCompanyOwnedTrucks);

            if (driver != null && driver.IsExternal && !allowSubcontractorsToDriveCompanyOwnedTrucks)
            {
                return;
            }

            //SetPreviousEmployeeTimeEndDateTimeIfNull
            var notEndedEmployeeTime = await GetNotEndedEmployeeTime(info.UserId);
            if (notEndedEmployeeTime != null)
            {
                notEndedEmployeeTime.EndDateTime = notEndedEmployeeTime.StartDateTime.ConvertTimeZoneTo(info.TimeZone).EndOfDay().ConvertTimeZoneFrom(info.TimeZone);
            }

            var truckId = await (await _driverAssignmentRepository.GetQueryAsync())
                    .Where(da => da.DriverId == info.DriverId && da.Date == userDate)
                    .Select(da => (int?)da.TruckId)
                    .FirstOrDefaultAsync(CancellationTokenProvider.Token);

            var timeClassificationId = await GetValidatedTimeClassificationIdOrNullAsync(input.TimeClassificationId)
                ?? await SettingManager.GetSettingValueForTenantAsync<int>(AppSettings.TimeAndPay.TimeTrackingDefaultTimeClassificationId, info.TenantId);

            var employeeTime = new Drivers.EmployeeTime
            {
                UserId = info.UserId,
                StartDateTime = info.ActionTimeInUtc,
                TimeClassificationId = timeClassificationId,
                EquipmentId = truckId,
                Latitude = input.Latitude,
                Longitude = input.Longitude,
                Description = input.Description,
                TenantId = await AbpSession.GetTenantIdAsync(),
                DriverId = info.DriverId,
            };
            await _employeeTimeRepository.InsertAsync(employeeTime);

            await CurrentUnitOfWork.SaveChangesAsync();

            await _syncRequestSender.SendSyncRequest(new SyncRequest()
                .AddChange(EntityEnum.EmployeeTime, employeeTime.ToChangedEntity())
                .SetIgnoreForDeviceId(info.DeviceId));
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

        private async Task DriverClockOut(DriverApplicationActionInfo info)
        {
            var notEndedEmployeeTime = await GetNotEndedEmployeeTime(info.UserId);
            if (notEndedEmployeeTime == null)
            {
                return;
            }

            notEndedEmployeeTime.EndDateTime = info.ActionTimeInUtc;
        }

        private async Task<Drivers.EmployeeTime> GetNotEndedEmployeeTime(long userId)
        {
            return await (await _employeeTimeRepository.GetQueryAsync())
                .FirstOrDefaultAsync(et => et.UserId == userId && et.EndDateTime == null && !et.IsImported,
                  CancellationTokenProvider.Token);
        }

        private async Task UploadTicketPhotoIfNeeded(LoadDispatchInput input)
        {
            if (input.TicketPhotoId != null || string.IsNullOrEmpty(input.TicketPhotoBase64))
            {
                return;
            }

            var tenantId = await AbpSession.GetTenantIdOrNullAsync();
            input.TicketPhotoId = await _binaryObjectManager.UploadDataUriStringAsync(input.TicketPhotoBase64, tenantId, 12000000);
        }

        private static void ChangeMultipleLoadsDispatchStatusToAcknowledged(Dispatch dispatch, Load load, DriverApplicationActionInfo info)
        {
            dispatch.Status = DispatchStatus.Acknowledged;
            load.DestinationDateTime = info?.ActionTimeInUtc ?? Clock.Now;
        }

        private async Task<IDisposable> UseExistingSessionOrFallbackToDispatchUserAsync(Dispatch dispatchEntity)
        {
            if (Session.UserId == null)
            {
                Logger.Warn("UseExistingSessionOrFallbackToDispatchUser: Session.UserId is null, falling back to " + (dispatchEntity.UserId?.ToString() ?? "null"));
            }

            var tenantId = await Session.GetTenantIdOrNullAsync();
            return Session.Use(tenantId ?? dispatchEntity.TenantId, Session.UserId ?? dispatchEntity.UserId);
        }

        private static Task ChangeDispatchStatusToLoadedAsync(Dispatch dispatch, Load load, DriverApplicationActionInfo info)
        {
            if (dispatch.Status == DispatchStatus.Acknowledged)
            {
                dispatch.NumberOfAddedLoads++;
            }
            dispatch.Status = DispatchStatus.Loaded;
            load.SourceDateTime = info?.ActionTimeInUtc ?? Clock.Now;

            return Task.CompletedTask;
        }

        private async Task ChangeDispatchStatusToCompleted(Dispatch dispatch, Load load, DriverApplicationActionInfo info)
        {
            var oldActiveDispatch = await GetFirstOpenDispatch(dispatch.DriverId);

            if (dispatch.NumberOfLoadsToFinish > 0 && dispatch.NumberOfLoadsToFinish < dispatch.NumberOfAddedLoads)
            {
                dispatch.Status = DispatchStatus.Canceled;
                //might already be canceled
                dispatch.Canceled ??= info?.ActionTimeInUtc ?? Clock.Now;
            }
            else
            {
                if (dispatch.Status != DispatchStatus.Canceled || info?.ActionTimeInUtc < dispatch.Canceled)
                {
                    dispatch.Status = DispatchStatus.Completed;
                }
            }
            load.DestinationDateTime = info?.ActionTimeInUtc ?? Clock.Now;

            await CurrentUnitOfWork.SaveChangesAsync();

            await RunPostDispatchCompletionLogic(dispatch);

            var newActiveDispatch = await GetFirstOpenDispatch(dispatch.DriverId);

            await _dispatchSender.SendSmsOrEmail(new SendSmsOrEmailInput
            {
                TruckId = dispatch.TruckId,
                DriverId = dispatch.DriverId,
                UserId = dispatch.UserId,
                PhoneNumber = dispatch.PhoneNumber,
                EmailAddress = dispatch.EmailAddress,
                OrderNotifyPreferredFormat = dispatch.OrderNotifyPreferredFormat,
                SendOrdersToDriversImmediately = false,
                AfterCompleted = true,
                ActiveDispatchWasChanged = oldActiveDispatch?.Id != newActiveDispatch?.Id,
            });
        }

        [RemoteService(false)]
        public async Task RunPostDispatchCompletionLogic(int dispatchId)
        {
            await CurrentUnitOfWork.SaveChangesAsync();
            var dispatch = await _dispatchRepository.GetAsync(dispatchId);
            await RunPostDispatchCompletionLogic(dispatch);
        }

        private async Task RunPostDispatchCompletionLogic(Dispatch dispatch)
        {
            await CurrentUnitOfWork.SaveChangesAsync();
            await CompleteOrderLineTrucksOfHourlyDispatchIfNeeded(dispatch);
            await SendCompletedDispatchNotificationIfNeeded(dispatch);
        }

        private async Task SendCompletedDispatchNotificationIfNeeded(Dispatch dispatch)
        {
            await CurrentUnitOfWork.SaveChangesAsync();

            var hasMoreDispatches = await (await _dispatchRepository.GetQueryAsync())
                    .Where(d => d.DriverId == dispatch.DriverId && Dispatch.OpenStatuses.Contains(d.Status) && d.Id != dispatch.Id)
                    .AnyAsync(CancellationTokenProvider.Token);

            if (!hasMoreDispatches && !await ShouldSendOrdersToDriversImmediately())
            {
                var orderDetails = await (await _orderLineRepository.GetQueryAsync())
                    .Where(x => x.Id == dispatch.OrderLineId)
                    .Select(x => new
                    {
                        OfficeId = x.Order.OfficeId,
                    }).FirstOrDefaultAsync(CancellationTokenProvider.Token);

                var driverDetails = await (await _driverRepository.GetQueryAsync())
                    .Where(x => x.Id == dispatch.DriverId)
                    .Select(x => new
                    {
                        FullName = x.FirstName + " " + x.LastName,
                    }).FirstOrDefaultAsync(CancellationTokenProvider.Token);

                var truckDetails = await (await _truckRepository.GetQueryAsync())
                    .Where(x => x.Id == dispatch.TruckId)
                    .Select(x => new
                    {
                        x.TruckCode,
                        LeaseHaulerName = x.LeaseHaulerTruck.LeaseHauler.Name,
                    }).FirstOrDefaultAsync(CancellationTokenProvider.Token);

                if (orderDetails != null && driverDetails != null && truckDetails != null)
                {
                    var truckCode =
                        (truckDetails.LeaseHaulerName.IsNullOrEmpty() ? "" : truckDetails.LeaseHaulerName + " ")
                        + truckDetails.TruckCode;
                    await _appNotifier.SendPriorityNotification(
                        new SendPriorityNotificationInput(
                            L("DriverNameHasFinishedDispatches")
                                .Replace("{DriverName}", driverDetails.FullName)
                                .Replace("{TruckCode}", truckCode),
                            NotificationSeverity.Warn,
                            [orderDetails.OfficeId]
                        )
                        {
                            OnlineFilter = true,
                            RoleFilter = new[] { StaticRoleNames.Tenants.Dispatching },
                        });
                }
            }
        }

        private async Task<bool> ShouldSendOrdersToDriversImmediately()
        {
            var dispatchVia = (DispatchVia)await SettingManager.GetSettingValueAsync<int>(AppSettings.DispatchingAndMessaging.DispatchVia);
            return dispatchVia == DispatchVia.None || dispatchVia == DispatchVia.SimplifiedSms;
        }

        private async Task AcknowledgeDispatchInternal(AcknowledgeDispatchInput input)
        {
            var dispatch = await _dispatchRepository.GetAsync(input.DispatchId);
            if (!dispatch.Status.IsIn(DispatchStatus.Sent, DispatchStatus.Created))
            {
                dispatch.Acknowledged ??= input.Info?.ActionTimeInUtc ?? Clock.Now;
                return;
            }
            ChangeDispatchStatusToAcknowledged(dispatch, input.Info);

            await CurrentUnitOfWork.SaveChangesAsync();

            await _syncRequestSender.SendSyncRequest(new SyncRequest()
                .AddChange(EntityEnum.Dispatch, dispatch.ToChangedEntity())
                .SetIgnoreForDeviceId(input.Info?.DeviceId)
                .AddLogMessage("Acknowledged dispatch"));
        }

        private void ChangeDispatchStatusToAcknowledged(Dispatch dispatch, DriverApplicationActionInfo info)
        {
            dispatch.Status = DispatchStatus.Acknowledged;
            dispatch.Acknowledged = info?.ActionTimeInUtc ?? Clock.Now;
        }

        private async Task CompleteOrderLineTrucksOfHourlyDispatchIfNeeded(Dispatch dispatch)
        {
            var dispatchData = await (await _dispatchRepository.GetQueryAsync())
                .Where(x => x.Id == dispatch.Id)
                .Select(x => new
                {
                    x.OrderLine.Designation,
                    FreightUomName = x.OrderLine.FreightUom.Name,
                    OrderLineTrucks = x.OrderLine.OrderLineTrucks.Select(t => new
                    {
                        t.Id,
                        t.IsDone,
                    }).ToList(),
                    RelatedDispatches = x.OrderLineTruck.Dispatches.Select(d => new
                    {
                        d.Id,
                        d.Status,
                    }).ToList(),
                }).FirstAsync(CancellationTokenProvider.Token);

            if (dispatchData.Designation.MaterialOnly()
                || !dispatchData.FreightUomName.ToLower().StartsWith("hour")
                || dispatch.Status != DispatchStatus.Completed
                || dispatch.OrderLineTruckId == null)
            {
                return;
            }

            if (!dispatchData.RelatedDispatches.Where(x => x.Id != dispatch.Id).Any(x => Dispatch.OpenStatuses.Contains(x.Status)))
            {
                var orderLineTruck = await _orderLineTruckRepository.FirstOrDefaultAsync(x => x.Id == dispatch.OrderLineTruckId.Value);
                if (orderLineTruck != null) //in some cases an old OLT was already deleted in production, and we don't want to throw an exception in this case and we want the driver app to continue working
                {
                    orderLineTruck.IsDone = true;
                    orderLineTruck.Utilization = 0;
                }

                if (dispatchData.OrderLineTrucks.Where(x => x.Id != orderLineTruck?.Id).All(x => x.IsDone))
                {
                    var orderLine = await _orderLineRepository.FirstOrDefaultAsync(x => x.Id == dispatch.OrderLineId);
                    if (orderLine != null)
                    {
                        orderLine.IsComplete = true;
                    }
                }
            }
        }
    }
}
