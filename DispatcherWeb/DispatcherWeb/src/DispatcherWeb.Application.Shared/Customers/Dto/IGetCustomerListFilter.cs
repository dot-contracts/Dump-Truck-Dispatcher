namespace DispatcherWeb.Customers.Dto
{
    public interface IGetCustomerListFilter
    {
        string Name { get; set; }
        FilterActiveStatus Status { get; set; }
    }
}
