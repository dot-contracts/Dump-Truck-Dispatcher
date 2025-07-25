using System;

namespace DispatcherWeb.Scheduling.Dto
{
    public interface IGetScheduleInput
    {
        int? OfficeId { get; set; }
        DateTime Date { get; set; }
        Shift? Shift { get; set; }
    }
}
