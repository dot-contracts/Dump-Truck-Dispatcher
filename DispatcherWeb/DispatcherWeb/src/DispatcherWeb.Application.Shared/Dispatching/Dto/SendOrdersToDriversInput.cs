using System;

namespace DispatcherWeb.Dispatching.Dto
{
    public class SendOrdersToDriversInput
    {
        public DateTime DeliveryDate { get; set; }
        public Shift? Shift { get; set; }
        public int[] OfficeIds { get; set; }
        public bool SendOnlyFirstOrder { get; set; }
    }
}
