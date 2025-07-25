using System;

namespace DispatcherWeb.Scheduling.Dto
{
    public class GetLeaseHaulerSelectionModelInput
    {
        public int OfficeId { get; set; }
        public DateTime Date { get; set; }
        public Shift? Shift { get; set; }
    }
}
