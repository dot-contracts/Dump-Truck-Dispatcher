namespace DispatcherWeb.Trucks.Dto
{
    public interface IGetTruckListFilter
    {
        string TruckCode { get; set; }
        int? OfficeId { get; set; }
        int? VehicleCategoryId { get; set; }
        FilterActiveStatus Status { get; set; }
        bool? IsOutOfService { get; set; }
        bool PlatesExpiringThisMonth { get; set; }
        bool IncludeSold { get; set; }
    }
}
