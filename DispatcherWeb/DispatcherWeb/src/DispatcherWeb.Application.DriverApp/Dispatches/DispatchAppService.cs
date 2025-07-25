using Abp.Application.Services.Dto;
using Abp.Authorization;
using Abp.Configuration;
using Abp.Domain.Repositories;
using Abp.Linq.Extensions;
using Abp.Runtime.Session;
using Abp.UI;
using DispatcherWeb.Authorization;
using DispatcherWeb.Common.Dto;
using DispatcherWeb.Configuration;
using DispatcherWeb.Dispatching;
using DispatcherWeb.DriverApp.Dispatches.Dto;
using DispatcherWeb.DriverApp.Loads.Dto;
using DispatcherWeb.DriverApp.Locations.Dto;
using DispatcherWeb.DriverApp.Tickets.Dto;
using DispatcherWeb.Features;
using DispatcherWeb.Infrastructure.Extensions;
using DispatcherWeb.Orders;
using DispatcherWeb.SyncRequests;
using DispatcherWeb.Tickets;
using DispatcherWeb.Tickets.Dto;
using Microsoft.EntityFrameworkCore;

namespace DispatcherWeb.DriverApp.Dispatches
{
    [AbpAuthorize(AppPermissions.Pages_DriverApplication_ReactNativeDriverApp)]
    public class DispatchAppService : DispatcherWebDriverAppAppServiceBase, IDispatchAppService
    {
        private readonly IRepository<Dispatch> _dispatchRepository;
        private readonly ISyncRequestSender _syncRequestSender;
        private readonly IDispatchingAppService _dispatchingAppService;

        public DispatchAppService(
            IRepository<Dispatch> dispatchRepository,
            ISyncRequestSender syncRequestSender,
            DispatcherWeb.Dispatching.IDispatchingAppService dispatchingAppService
            )
        {
            _dispatchRepository = dispatchRepository;
            _syncRequestSender = syncRequestSender;
            _dispatchingAppService = dispatchingAppService;
        }

        public async Task<IPagedResult<DispatchDto>> Get(GetInput input)
        {
            var allowProductionPay = await SettingManager.GetSettingValueForTenantAsync<bool>(AppSettings.TimeAndPay.AllowProductionPay, await Session.GetTenantIdAsync());
            var enableDriverAppGps = await FeatureChecker.IsEnabledAsync(AppFeatures.AllowGpsTracking)
                && await SettingManager.GetSettingValueAsync<bool>(AppSettings.GpsIntegration.DtdTracker.EnableDriverAppGps)
                && (GpsPlatform)await SettingManager.GetSettingValueAsync<int>(AppSettings.GpsIntegration.Platform) == GpsPlatform.DtdTracker;
            var separateItems = await FeatureChecker.IsEnabledAsync(AppFeatures.SeparateMaterialAndFreightItems);
            var requiredTicketEntry = await SettingManager.GetRequiredTicketEntry();

            var query = (await _dispatchRepository.GetQueryAsync())
                .Where(x => x.Driver.UserId == Session.UserId)
                .WhereIf(input.Id.HasValue, x => x.Id == input.Id)
                //.WhereIf(input.Id == null, x => Dispatch.OpenStatuses.Contains(x.Status) || x.Status == DispatchStatus.Completed)
                .WhereIf(input.DispatchStatuses?.Any() == true, x => input.DispatchStatuses.Contains(x.Status))
                .WhereIf(input.TruckId.HasValue, x => x.TruckId == input.TruckId)
                .WhereIf(input.OrderDateBegin.HasValue, x => x.OrderLine.Order.DeliveryDate >= input.OrderDateBegin)
                .WhereIf(input.OrderDateEnd.HasValue, x => x.OrderLine.Order.DeliveryDate <= input.OrderDateEnd)
                .WhereIf(input.HasLoads == true, x => x.Loads.Any())
                .WhereIf(input.HasLoads == false, x => !x.Loads.Any())
                .WhereIf(input.ModifiedAfterDateTime.HasValue, d => d.CreationTime > input.ModifiedAfterDateTime.Value || (d.LastModificationTime != null && d.LastModificationTime > input.ModifiedAfterDateTime.Value))
                .OrderByDescending(d => d.Status == DispatchStatus.Loaded)
                    .ThenByDescending(d => d.Status == DispatchStatus.Acknowledged)
                    .ThenByDescending(d => d.Status == DispatchStatus.Sent)
                    .ThenBy(d => d.SortOrder)
                .Select(di => new DispatchDto
                {
                    Id = di.Id,
                    TenantId = di.TenantId,
                    CustomerName = di.OrderLine.Order.Customer.Name,
                    CustomerContact = di.OrderLine.Order.CustomerContact == null ? null : new CustomerContactDto
                    {
                        Name = di.OrderLine.Order.CustomerContact.Name,
                        PhoneNumber = di.OrderLine.Order.CustomerContact.PhoneNumber,
                    },
                    OrderDate = di.OrderLine.Order.DeliveryDate,
                    Shift = di.OrderLine.Order.Shift,
                    Status = di.Status,
                    Designation = di.OrderLine.Designation,
                    FreightItemId = di.OrderLine.FreightItemId,
                    FreightItem = di.OrderLine.FreightItem.Name,
                    MaterialItemId = di.OrderLine.MaterialItemId,
                    MaterialItem = di.OrderLine.MaterialItem.Name,
                    LoadAt = di.OrderLine.LoadAt == null ? null : new LocationDto
                    {
                        Name = di.OrderLine.LoadAt.Name,
                        Latitude = di.OrderLine.LoadAt.Latitude,
                        Longitude = di.OrderLine.LoadAt.Longitude,
                        AddressObject = new LocationAddressDto
                        {
                            StreetAddress = di.OrderLine.LoadAt.StreetAddress,
                            City = di.OrderLine.LoadAt.City,
                            State = di.OrderLine.LoadAt.State,
                            ZipCode = di.OrderLine.LoadAt.ZipCode,
                            CountryCode = di.OrderLine.LoadAt.CountryCode,
                        },
                    },
                    DeliverTo = di.OrderLine.DeliverTo == null ? null : new LocationDto
                    {
                        Name = di.OrderLine.DeliverTo.Name,
                        Latitude = di.OrderLine.DeliverTo.Latitude,
                        Longitude = di.OrderLine.DeliverTo.Longitude,
                        AddressObject = new LocationAddressDto
                        {
                            StreetAddress = di.OrderLine.DeliverTo.StreetAddress,
                            City = di.OrderLine.DeliverTo.City,
                            State = di.OrderLine.DeliverTo.State,
                            ZipCode = di.OrderLine.DeliverTo.ZipCode,
                            CountryCode = di.OrderLine.DeliverTo.CountryCode,
                        },
                    },
                    CustomerNotification = di.OrderLine.RequiresCustomerNotification ? new CustomerNotificationDto
                    {
                        ContactName = di.OrderLine.CustomerNotificationContactName,
                        PhoneNumber = di.OrderLine.CustomerNotificationPhoneNumber,
                    } : null,
                    MaterialQuantity = di.MaterialQuantity,
                    FreightQuantity = di.FreightQuantity,
                    TravelTime = di.OrderLine.TravelTime,
                    JobNumber = di.OrderLine.JobNumber,
                    Note = di.Note,
                    IsCOD = di.OrderLine.Order.Customer.IsCod,
                    ChargeTo = di.OrderLine.Order.ChargeTo,
                    MaterialUomId = di.OrderLine.MaterialUomId,
                    MaterialUOM = di.OrderLine.MaterialUom.Name,
                    FreightUomId = di.OrderLine.FreightUomId,
                    FreightUOM = di.OrderLine.FreightUom.Name,
                    LastModifiedDateTime = di.LastModificationTime.HasValue && di.LastModificationTime.Value > di.CreationTime ? di.LastModificationTime.Value : di.CreationTime,
                    ProductionPay = allowProductionPay && di.OrderLine.ProductionPay,
                    RequireTicket = di.OrderLine.RequireTicket,
                    TimeOnJob = di.TimeOnJob,
                    IsMultipleLoads = di.IsMultipleLoads,
                    Loads = di.Loads.Select(l => new LoadDto
                    {
                        Id = l.Id,
                        DispatchId = l.DispatchId,
                        Guid = l.Guid,
                        SourceDateTime = l.SourceDateTime,
                        SourceLatitude = l.SourceLatitude,
                        SourceLongitude = l.SourceLongitude,
                        DestinationDateTime = l.DestinationDateTime,
                        DestinationLatitude = l.DestinationLatitude,
                        DestinationLongitude = l.DestinationLongitude,
                        SignatureId = l.SignatureId,
                        SignatureName = l.SignatureName,
                        TravelTime = l.TravelTime,
                    }).ToList(),
                    Tickets = di.Loads.SelectMany(l => l.Tickets).Select(t => new TicketDto
                    {
                        Id = t.Id,
                        LoadId = t.LoadId,
                        DispatchId = di.Id,
                        Quantity = t.MaterialQuantity ?? 0, //kept for backwards compatibility
                        FreightQuantity = t.FreightQuantity,
                        MaterialQuantity = t.MaterialQuantity,
                        ItemId = t.FreightItemId,
                        ItemName = t.FreightItem.Name,
                        FreightItemId = t.FreightItemId,
                        FreightItemName = t.FreightItem.Name,
                        MaterialItemId = t.MaterialItemId,
                        MaterialItemName = t.MaterialItem.Name,
                        UnitOfMeasureId = t.FreightUomId,
                        FreightUomId = t.FreightUomId,
                        MaterialUomId = t.MaterialUomId,
                        TicketDateTime = t.TicketDateTime,
                        TicketNumber = t.TicketNumber,
                        TicketPhotoId = t.TicketPhotoId,
                        TicketPhotoFilename = t.TicketPhotoFilename,
                        LoadCount = t.LoadCount,
                        Nonbillable = t.NonbillableMaterial && t.NonbillableFreight,
                    }).ToList(),
                    TruckId = di.TruckId,
                    TruckCode = di.Truck.TruckCode,
                    TrailerId = di.OrderLineTruck.TrailerId,
                    TrailerTruckCode = di.OrderLineTruck.Trailer.TruckCode,
                    DriverId = di.DriverId,
                    TimeClassificationId = di.OrderLine.DriverPayTimeClassificationId,
                    EnableDriverAppGps = enableDriverAppGps && di.Truck.EnableDriverAppGps,
                    OrderLineId = di.OrderLineId,
                    OrderLineTruckId = di.OrderLineTruckId,
                    AcknowledgedDateTime = di.Acknowledged,
                    SortOrder = di.SortOrder,
                    //NumberOfAddedLoads = di.NumberOfAddedLoads,
                    //NumberOfLoadsToFinish = di.NumberOfLoadsToFinish,
                });

            var totalCount = await query.CountAsync(CancellationTokenProvider.Token);
            var items = await query
                .PageBy(input)
                .ToListAsync(CancellationTokenProvider.Token);

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
                dispatch.RequireTicket = DispatchingAppService.IsTicketRequired(dispatch.RequireTicket, requiredTicketEntry);
            }

            return new PagedResultDto<DispatchDto>(
                totalCount,
                items);
        }

        public async Task<DispatchDto> Post(DispatchEditDto model)
        {
            var dispatch = await _dispatchRepository.FirstOrDefaultAsync(model.Id);

            if (dispatch == null)
            {
                throw new UserFriendlyException($"Dispatch with id {model.Id} wasn't found");
            }

            if (!await (await _dispatchRepository.GetQueryAsync()).AnyAsync(x => x.Id == model.Id && x.Driver.UserId == Session.UserId,
                    CancellationTokenProvider.Token))
            {
                throw new UserFriendlyException($"You cannot modify dispatches assigned to other users");
            }

            var oldDispatchStatus = dispatch.Status;

            if (dispatch.Status != DispatchStatus.Canceled)
            {
                dispatch.Status = model.Status;
            }
            dispatch.Acknowledged = model.AcknowledgedDateTime;
            if (dispatch.IsMultipleLoads)
            {
                dispatch.IsMultipleLoads = model.IsMultipleLoads;
            }

            if (dispatch.Status != oldDispatchStatus)
            {
                if (dispatch.Status == DispatchStatus.Completed)
                {
                    await CurrentUnitOfWork.SaveChangesAsync();
                    await _dispatchingAppService.RunPostDispatchCompletionLogic(model.Id);
                }
            }

            await CurrentUnitOfWork.SaveChangesAsync();
            //todo send notifications etc, add status validation if needed
            await _syncRequestSender.SendSyncRequest(new SyncRequest()
                .AddChange(EntityEnum.Dispatch, dispatch.ToChangedEntity())
                //.SetIgnoreForDeviceId(input.DeviceId) //TODO add DeviceId to all DriverApp requests
                .AddLogMessage("Dispatch was updated from Driver App"));

            return (await Get(new GetInput { Id = model.Id })).Items.FirstOrDefault();
        }
    }
}
