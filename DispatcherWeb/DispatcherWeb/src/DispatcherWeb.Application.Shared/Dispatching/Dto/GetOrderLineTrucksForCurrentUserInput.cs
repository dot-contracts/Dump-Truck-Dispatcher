using System;

namespace DispatcherWeb.Dispatching.Dto
{
    public class GetOrderLineTrucksForCurrentUserInput
    {
        public DateTime? UpdatedAfterDateTime { get; set; }
        public Guid? DriverGuid { get; set; }
    }
}
