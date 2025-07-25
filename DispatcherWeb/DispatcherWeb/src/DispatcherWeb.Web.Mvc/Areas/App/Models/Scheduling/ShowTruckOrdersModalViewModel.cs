using System;

namespace DispatcherWeb.Web.Areas.App.Models.Scheduling
{
    public class ShowTruckOrdersModalViewModel
    {
        public int TruckId { get; set; }
        public DateTime ScheduleDate { get; set; }
        public Shift? Shift { get; set; }

        public DateTime? StartTime { get; set; }
    }
}
