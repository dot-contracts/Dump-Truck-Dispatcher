namespace DispatcherWeb.Tickets.Dto
{
    public class GetTruckAndDriverForTicketTrailerResult
    {
        public int? TruckId { get; set; }
        public string TruckCode { get; set; }
        public int? DriverId { get; set; }
        public string DriverName { get; set; }
        public int? CarrierId { get; set; }
        public string CarrierName { get; set; }
    }
}
