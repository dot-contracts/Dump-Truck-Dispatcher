using Abp.Application.Services.Dto;
using Abp.Authorization;
using Abp.Domain.Entities;
using Abp.Domain.Repositories;
using Abp.Extensions;
using Abp.Notifications;
using Abp.UI;
using DispatcherWeb.Authorization;
using DispatcherWeb.Dispatching;
using DispatcherWeb.DriverApp.Tickets.Dto;
using DispatcherWeb.Features;
using DispatcherWeb.FuelSurchargeCalculations;
using DispatcherWeb.Infrastructure;
using DispatcherWeb.Infrastructure.Extensions;
using DispatcherWeb.Notifications;
using DispatcherWeb.Orders;
using DispatcherWeb.Tickets;
using Microsoft.EntityFrameworkCore;

namespace DispatcherWeb.DriverApp.Tickets
{
    [AbpAuthorize(AppPermissions.Pages_DriverApplication_ReactNativeDriverApp)]
    public class TicketAppService : DispatcherWebDriverAppAppServiceBase, ITicketAppService
    {
        private readonly IRepository<Ticket> _ticketRepository;
        private readonly IRepository<Load> _loadRepository;
        private readonly IRepository<Dispatch> _dispatchRepository;
        private readonly IFuelSurchargeCalculator _fuelSurchargeCalculator;
        private readonly ITicketQuantityHelper _ticketQuantityHelper;
        private readonly IAppNotifier _appNotifier;
        private readonly IDispatchingAppService _dispatchingAppService;

        public TicketAppService(
            IRepository<Ticket> ticketRepository,
            IRepository<Load> loadRepository,
            IRepository<Dispatch> dispatchRepository,
            IFuelSurchargeCalculator fuelSurchargeCalculator,
            ITicketQuantityHelper ticketQuantityHelper,
            IAppNotifier appNotifier,
            IDispatchingAppService dispatchingAppService
            )
        {
            _ticketRepository = ticketRepository;
            _loadRepository = loadRepository;
            _dispatchRepository = dispatchRepository;
            _fuelSurchargeCalculator = fuelSurchargeCalculator;
            _ticketQuantityHelper = ticketQuantityHelper;
            _appNotifier = appNotifier;
            _dispatchingAppService = dispatchingAppService;
        }

        public async Task<TicketDto> Post(TicketEditDto model)
        {
            var ticket = model.Id == 0
                ? new Ticket()
                : (await _ticketRepository.GetQueryAsync()).FirstOrDefault(x => x.Id == model.Id);

            var separateItems = await FeatureChecker.IsEnabledAsync(AppFeatures.SeparateMaterialAndFreightItems);

            model.TicketNumber = model.TicketNumber?.TruncateWithPostfix(EntityStringFieldLengths.Ticket.TicketNumber);
            model.TicketPhotoFilename = model.TicketPhotoFilename?.TruncateWithPostfixAsFilename(EntityStringFieldLengths.Ticket.TicketPhotoFilename);

            if (ticket == null)
            {
                var deletedTicket = await _ticketRepository.GetDeletedEntity(new EntityDto(model.Id), CurrentUnitOfWork);
                if (deletedTicket == null)
                {
                    throw new UserFriendlyException($"Ticket with id {model.Id} wasn't found");
                }
                await SendDeletedRnEntityNotificationIfNeededAsync(deletedTicket, model);
                deletedTicket.UnDelete();
                ticket = deletedTicket;
            }

            if (!model.LoadId.HasValue)
            {
                throw new UserFriendlyException("LoadId is required");
            }

            if (!await (await _loadRepository.GetQueryAsync()).AnyAsync(x => x.Id == model.LoadId,
                      CancellationTokenProvider.Token))
            {
                var deletedLoad = await _loadRepository.GetDeletedEntity(new EntityDto(model.LoadId.Value), CurrentUnitOfWork);
                if (deletedLoad == null)
                {
                    throw new UserFriendlyException($"Load with id {model.LoadId} wasn't found");
                }
                await SendDeletedRnEntityNotificationIfNeededAsync(deletedLoad);
                deletedLoad.UnDelete();
            }

            if (!model.DispatchId.HasValue)
            {
                throw new UserFriendlyException("DispatchId is required");
            }

            var dispatchData = await (await _dispatchRepository.GetQueryAsync())
                .Where(x => x.Id == model.DispatchId)
                .Select(x => new
                {
                    x.OrderLineId,
                    x.OrderLine.Order.OfficeId,
                    x.OrderLine.LoadAtId,
                    x.OrderLine.DeliverToId,
                    x.TruckId,
                    x.Truck.TruckCode,
                    x.OrderLineTruck.TrailerId,
                    LeaseHaulerId = (int?)x.Truck.LeaseHaulerTruck.LeaseHaulerId,
                    x.OrderLine.Order.CustomerId,
                    x.OrderLine.FreightItemId,
                    x.DriverId,
                    x.OrderLine.Designation,
                    x.OrderLine.MaterialUomId,
                    x.OrderLine.FreightUomId,
                    x.TenantId,
                }).FirstOrDefaultAsync(CancellationTokenProvider.Token);

            if (dispatchData == null)
            {
                throw new UserFriendlyException($"Dispatch with id {model.DispatchId} wasn't found");
            }

            var orderTotalsBeforeUpdate = await _dispatchingAppService.GetOrderTotalsAsync(dispatchData.OrderLineId);

            if (ticket.Id == 0)
            {
                ticket.OrderLineId = dispatchData.OrderLineId;
                ticket.OfficeId = dispatchData.OfficeId;
                ticket.LoadAtId = dispatchData.LoadAtId;
                ticket.DeliverToId = dispatchData.DeliverToId;
                ticket.TruckId = dispatchData.TruckId;
                ticket.TruckCode = dispatchData.TruckCode;
                ticket.TrailerId = dispatchData.TrailerId;
                ticket.CarrierId = dispatchData.LeaseHaulerId;
                ticket.CustomerId = dispatchData.CustomerId;
                ticket.DriverId = dispatchData.DriverId;
                ticket.TenantId = dispatchData.TenantId;
            }

            model.FreightItemId ??= model.ItemId;
            model.FreightUomId ??= model.UnitOfMeasureId;
            if (model.Version < 2)
            {
                model.FreightQuantity = model.Quantity;
                model.MaterialQuantity = model.Quantity;
            }

            await _ticketQuantityHelper.SetTicketQuantity(ticket, model);

            ticket.TicketDateTime = model.TicketDateTime;
            ticket.TicketNumber = model.TicketNumber;
            ticket.TicketPhotoId = model.TicketPhotoId;
            ticket.TicketPhotoFilename = model.TicketPhotoFilename;
            ticket.LoadCount = model.LoadCount;
            //ticket.Nonbillable = model.Nonbillable;

            if (ticket.Id == 0)
            {
                ticket.NonbillableFreight = !dispatchData.Designation.HasFreight();
                ticket.NonbillableMaterial = !dispatchData.Designation.HasMaterial();

                ticket.LoadId = model.LoadId;

                await _ticketRepository.InsertAndGetIdAsync(ticket);

                model.Id = ticket.Id;
            }

            ticket.IsInternal = ticket.TicketNumber.IsNullOrEmpty() || ticket.TicketNumber == "G-" + ticket.Id;

            if (model.GenerateTicketNumber)
            {
                ticket.TicketNumber = "G-" + ticket.Id;
                model.TicketNumber = ticket.TicketNumber;
            }

            await CurrentUnitOfWork.SaveChangesAsync();
            await _fuelSurchargeCalculator.RecalculateTicket(ticket.Id);

            await CurrentUnitOfWork.SaveChangesAsync();
            await _dispatchingAppService.NotifyDispatchersAfterTicketUpdateIfNeeded(dispatchData.OrderLineId, orderTotalsBeforeUpdate);

            return model;
        }

        private async Task<string> GetMeaningfulTicketDiffAsync(Ticket deletedTicket, TicketEditDto model)
        {
            var result = "";
            if (deletedTicket.TicketNumber != model.TicketNumber)
            {
                result += $"TicketNumber: {deletedTicket.TicketNumber} ➔ {(model.TicketNumber ?? "-")}; ";
            }
            else
            {
                result += $"TicketNumber: {(model.TicketNumber ?? "-")}; ";
            }

            if (deletedTicket.MaterialQuantity != model.MaterialQuantity)
            {
                result += $"Material Quantity: {deletedTicket.MaterialQuantity} ➔ {model.MaterialQuantity}; ";
            }

            if (deletedTicket.LoadCount != model.LoadCount)
            {
                result += $"Load Count: {deletedTicket.LoadCount} ➔ {model.LoadCount}; ";
            }
            if (deletedTicket.TicketDateTime != model.TicketDateTime)
            {
                var timezone = await GetTimezone();
                result += $"Ticket Date: {deletedTicket.TicketDateTime?.ConvertTimeZoneTo(timezone):g} ➔ {model.TicketDateTime?.ConvertTimeZoneTo(timezone):g} ";
            }
            if (deletedTicket.TicketPhotoId != model.TicketPhotoId)
            {
                result += $"Ticket Photo Id: {deletedTicket.TicketPhotoId} ➔ {model.TicketPhotoId}; ";
            }

            return result;
        }

        private async Task SendDeletedRnEntityNotificationIfNeededAsync(Ticket deletedTicket, TicketEditDto model)
        {
            if (!await FeatureChecker.IsEnabledAsync(AppFeatures.SendRnConflictsToUsers))
            {
                return;
            }

            var ticketDiff = await GetMeaningfulTicketDiffAsync(deletedTicket, model);

            await _appNotifier.SendNotificationAsync(
                new SendNotificationInput(
                    AppNotificationNames.SimpleMessage,
                    $"A ticket that has been deleted in the main app was uploaded from the native driver app. Driver: {await GetCurrentUserFullName()}; {ticketDiff}",
                    NotificationSeverity.Warn
                )
                {
                    IncludeLocalUsers = true,
                    PermissionFilter = AppPermissions.ReceiveRnConflicts,
                });
        }

        private async Task SendDeletedRnEntityNotificationIfNeededAsync(Load deletedLoad)
        {
            if (!await FeatureChecker.IsEnabledAsync(AppFeatures.SendRnConflictsToUsers))
            {
                return;
            }

            await _appNotifier.SendNotificationAsync(
                new SendNotificationInput(
                    AppNotificationNames.SimpleMessage,
                    $"A load that has been deleted in the main app was uploaded from the native driver app. Driver: {await GetCurrentUserFullName()};",
                    NotificationSeverity.Warn
                )
                {
                    IncludeLocalUsers = true,
                    PermissionFilter = AppPermissions.ReceiveRnConflicts,
                });
        }
    }
}
