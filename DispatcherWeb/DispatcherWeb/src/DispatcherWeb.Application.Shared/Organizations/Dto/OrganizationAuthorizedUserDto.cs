namespace DispatcherWeb.Organizations.Dto
{
    public class OrganizationAuthorizedUserDto
    {
        public long Id { get; set; }

        public int? OrganizationId { get; set; }

        public string Name { get; set; }

        public string Title { get; set; }

        public string Phone { get; set; }

        public string Email { get; set; }
    }
}
