namespace DispatcherWeb.LeaseHaulers.Dto
{
    public class EditLeaseHaulerTruckFromListInput
    {
        public int Id { get; set; }
        public bool IsActive { get; set; }
        public bool AlwaysShowOnSchedule { get; set; }
    }
}
