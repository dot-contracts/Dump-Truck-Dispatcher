namespace DispatcherWeb.Tickets
{
    public interface ITicketEditQuantity
    {
        decimal? FreightQuantity { get; }
        decimal? MaterialQuantity { get; }

        //int? UnitOfMeasureId { get; } //renamed to FreightUomId
        //int? MaterialUomId { get; } //always comes from the order line so not needed from the incoming data
        int? FreightUomId { get; }

        //int? ItemId { get; } //renamed to FreightItemId
        int? FreightItemId { get; }
        int? MaterialItemId { get; }
    }
}
