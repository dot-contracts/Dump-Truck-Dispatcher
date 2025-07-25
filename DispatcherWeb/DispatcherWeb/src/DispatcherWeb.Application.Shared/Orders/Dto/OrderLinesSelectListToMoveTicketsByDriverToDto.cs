namespace DispatcherWeb.Orders.Dto
{
    public class OrderLinesSelectListToMoveTicketsByDriverToDto
    {
        public int OrderLineId { get; set; }

        public string OfficeName { get; set; }

        public int OrderId { get; set; }

        public string LoadAtName { get; set; }

        public string DeliverToName { get; set; }

        public string FreightItemName { get; set; }

        public string MaterialItemName { get; set; }

        public DesignationEnum Designation { get; set; }
    }
}
