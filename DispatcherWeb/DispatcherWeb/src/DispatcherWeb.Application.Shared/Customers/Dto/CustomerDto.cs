namespace DispatcherWeb.Customers.Dto
{
    public class CustomerDto
    {
        public int Id { get; set; }

        public string Name { get; set; }

        public string AccountNumber { get; set; }

        public string TaxIdNumber { get; set; }

        public string Address1 { get; set; }

        public string Address2 { get; set; }

        public string City { get; set; }

        public string State { get; set; }

        public string ZipCode { get; set; }

        public string CountryCode { get; set; }

        public bool IsActive { get; set; }

        public string Notes { get; set; }

    }
}
