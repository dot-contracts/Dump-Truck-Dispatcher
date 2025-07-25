using DispatcherWeb.Tickets;

namespace DispatcherWeb.Dispatching.Dto
{
    public class ViewDispatchTicketDto : ITicketQuantity
    {
        public decimal? FreightQuantity { get; set; }
        public decimal? MaterialQuantity { get; set; }
        public DesignationEnum Designation { get; set; }
        public int? OrderLineMaterialUomId { get; set; }
        public int? OrderLineFreightUomId { get; set; }
        public int? TicketUomId { get; set; }
    }
}
