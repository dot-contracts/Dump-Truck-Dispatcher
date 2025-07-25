namespace DispatcherWeb.Web.Areas.App.Models.Dispatches
{
    public class DispatchListViewModel
    {
        public int? OrderLineId { get; set; }
        public int? TruckId { get; set; }
        public string TruckCode { get; set; }
        public int? DriverId { get; set; }
        public string DriverName { get; set; }
    }
}
