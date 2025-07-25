using System;

namespace DispatcherWeb.Orders.Dto
{
    public class WorkOrderReportLoadDto
    {
        public DateTime? LoadTime { get; set; }
        public DateTime? DeliveryTime { get; set; }
        public short? TravelTime { get; set; }
        public string SignatureName { get; set; }
        public Guid? SignatureId { get; set; }
        public byte[] SignatureBytes { get; set; }
    }
}
