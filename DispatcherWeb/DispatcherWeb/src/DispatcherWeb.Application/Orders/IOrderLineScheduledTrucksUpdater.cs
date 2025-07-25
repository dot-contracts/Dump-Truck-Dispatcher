using System.Threading.Tasks;
using DispatcherWeb.Orders.Dto;

namespace DispatcherWeb.Orders
{
    public interface IOrderLineScheduledTrucksUpdater
    {
        Task UpdateScheduledTrucks(OrderLine orderLine, double? scheduledTrucks);
        Task<decimal> GetOrderLineUtilization(int orderLineId);
        Task DeleteOrderLineTrucks(DeleteOrderLineTrucksInput input);
    }
}
