namespace DispatcherWeb.Tickets.Dto
{
    public class GetTruckAndTrailerForTicketDriverResult
    {
        public int? TruckId { get; set; }
        public string TruckCode { get; set; }
        public int? TrailerId { get; set; }
        public string TrailerTruckCode { get; set; }
        public int? CarrierId { get; set; }
        public string CarrierName { get; set; }
    }
}
