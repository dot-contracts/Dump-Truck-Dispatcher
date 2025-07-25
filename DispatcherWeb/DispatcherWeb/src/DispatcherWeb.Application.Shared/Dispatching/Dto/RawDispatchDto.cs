using System;

namespace DispatcherWeb.Dispatching.Dto
{
    public class RawDispatchDto
    {
        public int Id { get; set; }
        public int DriverId { get; set; }
        public string TruckCode { get; set; }
        public string DriverLastFirstName { get; set; }
        public DateTime? Sent { get; set; }
        public DateTime? Acknowledged { get; set; }
        public DateTime? Loaded { get; set; }
        public DateTime? Delivered { get; set; }
        public DispatchStatus Status { get; set; }
        public string CustomerName { get; set; }
        public string QuoteName { get; set; }
        public string JobNumber { get; set; }
        public string LoadAtName { get; set; }
        public string DeliverToName { get; set; }
        public string Item { get; set; }
        public Guid Guid { get; set; }
        public bool IsMultipleLoads { get; set; }
        public int? FilledTicketCount { get; set; }
    }
}
