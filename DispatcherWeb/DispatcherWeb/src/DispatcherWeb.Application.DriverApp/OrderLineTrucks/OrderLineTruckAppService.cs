using Abp.Application.Services.Dto;
using Abp.Authorization;
using Abp.Domain.Entities;
using Abp.Domain.Repositories;
using Abp.Extensions;
using Abp.Linq.Extensions;
using Abp.Notifications;
using Abp.Runtime.Session;
using Abp.UI;
using DispatcherWeb.Authorization;
using DispatcherWeb.DriverApp.OrderLineTrucks.Dto;
using DispatcherWeb.Features;
using DispatcherWeb.Infrastructure;
using DispatcherWeb.Notifications;
using DispatcherWeb.Orders;
using Microsoft.EntityFrameworkCore;

namespace DispatcherWeb.DriverApp.OrderLineTrucks
{
    [AbpAuthorize(AppPermissions.Pages_DriverApplication_ReactNativeDriverApp)]
    public class OrderLineTruckAppService : DispatcherWebDriverAppAppServiceBase, IOrderLineTruckAppService
    {
        private readonly IAppNotifier _appNotifier;
        private readonly IRepository<OrderLineTruck> _orderLineTruckRepository;

        public OrderLineTruckAppService(
            IAppNotifier appNotifier,
            IRepository<OrderLineTruck> orderLineTruckRepository
            )
        {
            _appNotifier = appNotifier;
            _orderLineTruckRepository = orderLineTruckRepository;
        }

        public async Task<IPagedResult<OrderLineTruckDto>> Get(GetInput input)
        {
            var currentUserId = Session.GetUserId();
            var query = (await _orderLineTruckRepository.GetQueryAsync())
                .Where(x => input.Ids.Contains(x.Id))
                .Select(x => new OrderLineTruckDto
                {
                    Id = x.Id,
                    DriverNote = x.DriverNote,
                })
                .OrderBy(x => x.Id);

            var totalCount = await query.CountAsync(CancellationTokenProvider.Token);
            var items = await query
                .PageBy(input)
                .ToListAsync(CancellationTokenProvider.Token);

            return new PagedResultDto<OrderLineTruckDto>(
                totalCount,
                items);
        }

        public async Task Post(OrderLineTruckDto model)
        {
            var orderLineTruck = await _orderLineTruckRepository.FirstOrDefaultAsync(model.Id);

            model.DriverNote = model.DriverNote?.TruncateWithPostfix(EntityStringFieldLengths.OrderLineTruck.DriverNote);

            if (orderLineTruck == null)
            {
                var deletedOrderLineTruck = await _orderLineTruckRepository.GetDeletedEntity(new EntityDto(model.Id), CurrentUnitOfWork);
                if (deletedOrderLineTruck == null)
                {
                    Logger.Error($"OrderLineTruck.Post: OrderLineTruck with id {model.Id} for tenant {await AbpSession.GetTenantIdOrNullAsync()} and user {AbpSession.UserId} wasn't found");
                    throw new UserFriendlyException($"OrderLineTruck with specified Id ('{model.Id}') wasn't found");
                }
                await SendDeletedRnEntityNotificationIfNeededAsync(deletedOrderLineTruck, model);
                deletedOrderLineTruck.UnDelete();
                deletedOrderLineTruck.IsDone = true;
                orderLineTruck = deletedOrderLineTruck;
                await CurrentUnitOfWork.SaveChangesAsync();
            }

            if (!await (await _orderLineTruckRepository.GetQueryAsync()).AnyAsync(x => x.Id == model.Id && x.Driver.UserId == Session.UserId,
                CancellationTokenProvider.Token))
            {
                Logger.Error($"OrderLineTruck.Post: OrderLineTruck with id {model.Id} is not assigned to user {AbpSession.UserId}");
                throw new UserFriendlyException($"You cannot modify OrderLineTrucks assigned to other users");
            }

            orderLineTruck.DriverNote = model.DriverNote;
        }

        private string GetMeaningfulOrderLineTruckDiff(OrderLineTruck deletedOrderLineTruck, OrderLineTruckDto model)
        {
            var result = "";
            if (deletedOrderLineTruck.DriverNote != model.DriverNote)
            {
                result += $"Driver Note: {deletedOrderLineTruck.DriverNote} ➔ {model.DriverNote}; ";
            }

            return result;
        }

        private async Task SendDeletedRnEntityNotificationIfNeededAsync(OrderLineTruck deletedOrderLineTruck, OrderLineTruckDto model)
        {
            if (!await FeatureChecker.IsEnabledAsync(AppFeatures.SendRnConflictsToUsers))
            {
                return;
            }

            var orderLineTruckDiff = GetMeaningfulOrderLineTruckDiff(deletedOrderLineTruck, model);

            await _appNotifier.SendNotificationAsync(
                new SendNotificationInput(
                    AppNotificationNames.SimpleMessage,
                    $"A truck assignment was updated in native driver app, but it is already deleted in the main up. Driver: {await GetCurrentUserFullName()}; {orderLineTruckDiff}",
                    NotificationSeverity.Warn
                )
                {
                    IncludeLocalUsers = true,
                    PermissionFilter = AppPermissions.ReceiveRnConflicts,
                });
        }


    }
}
