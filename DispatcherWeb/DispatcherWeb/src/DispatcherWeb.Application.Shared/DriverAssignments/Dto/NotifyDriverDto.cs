using System;

namespace DispatcherWeb.DriverAssignments.Dto
{
    public class NotifyDriverDto
    {
        public string TruckCode { get; set; }
        public string DriverFullName { get; set; }
        public OrderNotifyPreferredFormat OrderNotifyPreferredFormat { get; set; }
        public string EmailAddress { get; set; }
        public string CellPhoneNumber { get; set; }
        public DateTime? StartTime { get; set; }
        public DateTime Date { get; set; }
    }
}
