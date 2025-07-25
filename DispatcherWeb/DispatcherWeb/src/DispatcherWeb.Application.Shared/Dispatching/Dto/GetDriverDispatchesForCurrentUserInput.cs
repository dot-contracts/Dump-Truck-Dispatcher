using System;

namespace DispatcherWeb.Dispatching.Dto
{
    public class GetDriverDispatchesForCurrentUserInput
    {
        public DateTime? UpdatedAfterDateTime { get; set; }
        public Guid? DriverGuid { get; set; }
    }
}
