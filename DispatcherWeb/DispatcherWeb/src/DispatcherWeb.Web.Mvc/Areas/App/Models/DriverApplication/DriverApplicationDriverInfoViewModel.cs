using System;

namespace DispatcherWeb.Web.Areas.App.Models.DriverApplication
{
    public class DriverApplicationClockViewModel
    {
        public DateTime ElapsedTime { get; set; }
        public bool ClockIsStarted { get; set; }
    }

    public class DriverApplicationDriverInfoViewModel<T> : DriverApplicationClockViewModel where T : new()
    {
        public T DriverInfoViewModel { get; set; }
    }
}
