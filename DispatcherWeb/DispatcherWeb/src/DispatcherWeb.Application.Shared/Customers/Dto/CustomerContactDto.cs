namespace DispatcherWeb.Customers.Dto
{
    public class CustomerContactDto
    {
        public int Id { get; set; }

        public int CustomerId { get; set; }

        public string Name { get; set; }

        public string PhoneNumber { get; set; }

        public string Fax { get; set; }

        public string Email { get; set; }

        public string Title { get; set; }

        public bool IsActive { get; set; }
    }
}
