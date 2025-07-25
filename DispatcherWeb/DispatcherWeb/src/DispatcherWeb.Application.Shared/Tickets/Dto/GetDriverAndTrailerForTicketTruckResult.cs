namespace DispatcherWeb.Tickets.Dto
{
    public class GetDriverAndTrailerForTicketTruckResult
    {
        public int? DriverId { get; set; }
        public string DriverName { get; set; }
        public int? TrailerId { get; set; }
        public string TrailerTruckCode { get; set; }
        public int? CarrierId { get; set; }
        public string CarrierName { get; set; }
        public bool TruckCodeIsCorrect { get; set; }
    }
}
