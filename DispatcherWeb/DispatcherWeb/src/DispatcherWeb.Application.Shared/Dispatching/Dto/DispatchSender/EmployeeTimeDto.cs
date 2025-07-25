using System;

namespace DispatcherWeb.Dispatching.Dto.DispatchSender
{
    public class EmployeeTimeDto
    {
        public int Id { get; set; }
        public long UserId { get; set; }
        public DateTime StartDateTime { get; set; }
        public DateTime? EndDateTime { get; set; }
        public bool IsImported { get; set; }
    }
}
