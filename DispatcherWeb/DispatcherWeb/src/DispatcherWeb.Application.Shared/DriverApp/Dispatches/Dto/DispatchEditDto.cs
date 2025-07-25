using System;

namespace DispatcherWeb.DriverApp.Dispatches.Dto
{
    public class DispatchEditDto
    {
        public int Id { get; set; }
        public DispatchStatus Status { get; set; }
        public DateTime? AcknowledgedDateTime { get; set; }
        public bool IsMultipleLoads { get; set; }
    }
}
