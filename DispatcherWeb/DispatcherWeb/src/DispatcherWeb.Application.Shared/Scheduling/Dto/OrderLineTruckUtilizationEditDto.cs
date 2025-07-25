namespace DispatcherWeb.Scheduling.Dto
{
    public class OrderLineTruckUtilizationEditDto
    {
        public int OrderLineTruckId { get; set; }
        public decimal Utilization { get; set; }
        public decimal MaxUtilization { get; set; }
        public string TruckCode { get; set; }
        public int OrderLineId { get; set; }
    }
}
