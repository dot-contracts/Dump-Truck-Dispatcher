using System;

namespace DispatcherWeb.Scheduling.Dto
{
    public class OrderLineTruckDetailsDto : OrderLineTruckUtilizationEditDto
    {
        public DateTime? TimeOnJob { get; set; }

        public bool? UpdateDispatchesTimeOnJob { get; set; }
    }
}
