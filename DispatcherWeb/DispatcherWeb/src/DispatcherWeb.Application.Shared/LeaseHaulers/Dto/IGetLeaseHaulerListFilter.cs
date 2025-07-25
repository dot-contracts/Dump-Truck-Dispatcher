namespace DispatcherWeb.LeaseHaulers.Dto
{
    public interface IGetLeaseHaulerListFilter
    {
        string Name { get; set; }
        string City { get; set; }
        string State { get; set; }
        FilterActiveStatus? Status { get; set; }
    }
}
