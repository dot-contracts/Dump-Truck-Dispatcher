namespace DispatcherWeb.Customers.Dto
{
    public class GetCustomerContactDuplicateCountInput
    {
        public int CustomerId { get; set; }
        public string Name { get; set; }
        public int? ExceptId { get; set; }
    }
}
