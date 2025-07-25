using System;

namespace DispatcherWeb.Dispatching.Dto
{
    public class GetJobForEditInput
    {
        public int? OrderLineId { get; set; }
        public DateTime? DeliveryDate { get; set; }
        public Shift? Shift { get; set; }
        public int? OfficeId { get; set; }
        public string OfficeName { get; set; }
        public string FocusFieldId { get; set; }
    }
}
