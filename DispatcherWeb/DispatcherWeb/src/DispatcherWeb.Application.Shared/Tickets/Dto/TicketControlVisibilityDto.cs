namespace DispatcherWeb.Tickets.Dto
{
    public class TicketControlVisibilityDto
    {
        public bool FreightItem { get; set; } //Freight Item / Item
        public bool MaterialItem { get; set; }

        public bool Quantity { get; set; }
        public bool FreightQuantity { get; set; }
        public bool MaterialQuantity { get; set; }

        public bool FreightUom { get; set; } //Freight UOM / UOM
        public bool MaterialUom { get; set; }
    }
}
