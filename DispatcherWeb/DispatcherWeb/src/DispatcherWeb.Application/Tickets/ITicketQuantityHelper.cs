using System.Threading.Tasks;
using DispatcherWeb.Orders;
using DispatcherWeb.Tickets.Dto;

namespace DispatcherWeb.Tickets
{
    public interface ITicketQuantityHelper
    {
        Task<decimal> GetMinimumFreightAmount(UnitOfMeasureBaseEnum? freightUomBaseId);
        Task<TicketControlVisibilityDto> GetVisibleTicketControls(int orderLineId);
        Task SetTicketQuantity(Ticket ticket, ITicketEditQuantity model);
    }
}
