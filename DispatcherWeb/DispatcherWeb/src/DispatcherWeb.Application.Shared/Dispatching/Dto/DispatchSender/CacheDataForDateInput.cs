using System;

namespace DispatcherWeb.Dispatching.Dto.DispatchSender
{
    public class CacheDataForDateInput
    {
        public DateTime DeliveryDate { get; set; }
        public Shift? Shift { get; set; }
        public int[] OfficeIds { get; set; }
    }
}
