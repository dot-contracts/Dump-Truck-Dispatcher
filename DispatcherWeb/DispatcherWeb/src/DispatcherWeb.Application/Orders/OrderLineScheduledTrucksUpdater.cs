using System;
using System.Linq;
using System.Threading.Tasks;
using Abp.Application.Services;
using Abp.Domain.Repositories;
using Abp.UI;
using DispatcherWeb.Dispatching;
using DispatcherWeb.Dispatching.Dto;
using DispatcherWeb.Infrastructure.Extensions;
using DispatcherWeb.Orders.Dto;
using Microsoft.EntityFrameworkCore;

namespace DispatcherWeb.Orders
{
    [RemoteService(false)]
    public class OrderLineScheduledTrucksUpdater : DispatcherWebAppServiceBase, IOrderLineScheduledTrucksUpdater
    {
        private readonly IRepository<OrderLineTruck> _orderLineTruckRepository;
        private readonly IRepository<OrderLine> _orderLineRepository;
        private readonly IDispatchingAppService _dispatchingAppService;

        public OrderLineScheduledTrucksUpdater(
            IRepository<OrderLineTruck> orderLineTruckRepository,
            IRepository<OrderLine> orderLineRepository,
            IDispatchingAppService dispatchingAppService
        )
        {
            _orderLineTruckRepository = orderLineTruckRepository;
            _orderLineRepository = orderLineRepository;
            _dispatchingAppService = dispatchingAppService;
        }

        public async Task UpdateScheduledTrucks(OrderLine orderLine, double? scheduledTrucks)
        {
            scheduledTrucks = scheduledTrucks.Round(2);

            await EnsureNumberOfTrucksIsNotLessCurrentUtilization();

            orderLine.ScheduledTrucks = scheduledTrucks;

            // Local functions
            async Task EnsureNumberOfTrucksIsNotLessCurrentUtilization()
            {
                var newNumberOfTrucks = Convert.ToDecimal(scheduledTrucks ?? 0);
                if (newNumberOfTrucks < Convert.ToDecimal(orderLine.NumberOfTrucks) && newNumberOfTrucks < await GetOrderLineUtilization(orderLine.Id))
                {
                    throw new ApplicationException("Cannot set number of trucks less than the current utilization!");
                }
            }
        }

        public async Task<decimal> GetOrderLineUtilization(int orderLineId)
        {
            return await (await _orderLineTruckRepository.GetQueryAsync())
                .Where(olt => olt.OrderLineId == orderLineId && olt.Truck.VehicleCategory.IsPowered)
                .SumAsync(olt => olt.Utilization);
        }

        public async Task DeleteOrderLineTrucks(DeleteOrderLineTrucksInput input)
        {
            var hasAcknowledgedOrLoadedDispatches = await (await _orderLineRepository.GetQueryAsync())
                .AnyAsync(ol =>
                    ol.Id == input.OrderLineId
                    && ol.Dispatches.Any(d => Dispatch.AcknowledgedOrLoadedStatuses.Contains(d.Status))
                );
            if (hasAcknowledgedOrLoadedDispatches)
            {
                throw new UserFriendlyException("There is an acknowledged or loaded dispatch!");
            }

            await _dispatchingAppService.CancelDispatches(new CancelDispatchesInput
            {
                OrderLineId = input.OrderLineId,
            });

            var orderLineTrucks = await (await _orderLineTruckRepository.GetQueryAsync())
                .Where(x => x.OrderLineId == input.OrderLineId)
                .ToListAsync();

            if (input.MarkAsDone)
            {
                foreach (var orderLineTruck in orderLineTrucks)
                {
                    orderLineTruck.IsDone = true;
                    orderLineTruck.Utilization = 0;
                }
            }
            else
            {
                await _orderLineTruckRepository.DeleteRangeAsync(orderLineTrucks);
            }
        }
    }
}
