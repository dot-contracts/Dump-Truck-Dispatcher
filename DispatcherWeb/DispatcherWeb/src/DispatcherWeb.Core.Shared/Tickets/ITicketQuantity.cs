namespace DispatcherWeb.Tickets
{
    public interface ITicketQuantity
    {
        decimal? MaterialQuantity { get; }
        decimal? FreightQuantity { get; }
        DesignationEnum Designation { get; }
        int? OrderLineMaterialUomId { get; }
        int? OrderLineFreightUomId { get; }
        int? TicketUomId { get; }
    }
}
