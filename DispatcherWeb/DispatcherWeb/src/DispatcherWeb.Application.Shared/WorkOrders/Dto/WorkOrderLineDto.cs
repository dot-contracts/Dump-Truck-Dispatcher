namespace DispatcherWeb.WorkOrders.Dto
{
    public class WorkOrderLineDto
    {
        public int Id { get; set; }
        public string VehicleServiceName { get; set; }
        public string Note { get; set; }
        public decimal? LaborTime { get; set; }
        public decimal? LaborCost { get; set; }
        public decimal? LaborRate { get; set; }
        public decimal? PartsCost { get; set; }
    }
}
