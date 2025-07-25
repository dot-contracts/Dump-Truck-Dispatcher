namespace DispatcherWeb.LeaseHaulers.Dto
{
    public class LeaseHaulerContactDto
    {
        public int Id { get; set; }
        public int LeaseHaulerId { get; set; }
        public string Name { get; set; }
        public string Title { get; set; }
        public string Phone { get; set; }
        public string Email { get; set; }
        public string CellPhoneNumber { get; set; }
    }
}
