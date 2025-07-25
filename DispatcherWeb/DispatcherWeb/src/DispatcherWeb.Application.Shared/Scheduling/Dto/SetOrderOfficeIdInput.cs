namespace DispatcherWeb.Scheduling.Dto
{
    public class SetOrderOfficeIdInput
    {
        public int OrderId { get; set; }
        public int? OrderLineId { get; set; }
        public int OfficeId { get; set; }
        public string OfficeName { get; set; }
    }
}
