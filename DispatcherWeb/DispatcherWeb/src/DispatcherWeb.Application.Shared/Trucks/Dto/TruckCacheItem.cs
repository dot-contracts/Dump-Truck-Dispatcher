namespace DispatcherWeb.Trucks.Dto
{
    public class TruckCacheItem
    {
        public int Id { get; set; }

        public string TruckCode { get; set; }

        public int? LeaseHaulerId { get; set; }

        public int? OfficeId { get; set; }
    }
}
