using DispatcherWeb.Dto;

namespace DispatcherWeb.Scheduling.Dto
{
    public class AddOrderTruckResult : OperationResultDto<ScheduleOrderLineTruckDto>
    {
        public decimal OrderUtilization { get; set; }
    }
}
