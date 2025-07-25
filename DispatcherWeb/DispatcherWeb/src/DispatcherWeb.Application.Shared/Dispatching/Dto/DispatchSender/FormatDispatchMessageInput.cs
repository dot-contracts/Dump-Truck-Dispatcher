using System;

namespace DispatcherWeb.Dispatching.Dto.DispatchSender
{
    public class FormatDispatchMessageInput
    {
        public string Message { get; set; }
        public Guid AcknowledgementGuid { get; set; }
        public string TruckCode { get; set; }
        public DateTime? TimeOnJob { get; set; }
        public DateTime? StartTime { get; set; }
        public bool MultipleLoads { get; set; }
        public int NumberOfDispatches { get; set; }
        public DispatchVia DispatchVia { get; set; }
    }
}
