using System;

namespace DispatcherWeb.Dispatching.Dto.DispatchSender
{
    public class DispatchDto
    {
        public int Id { get; set; }
        public DispatchStatus Status { get; set; }
        public int OrderLineId { get; set; }
        public int? OrderLineTruckId { get; set; }
        public int TruckId { get; set; }
        public int SortOrder { get; set; }
        public int DriverId { get; set; }
        public DateTime? TimeOnJob { get; set; }
        public DateTime? Acknowledged { get; set; }
        public Guid Guid { get; set; }
        public string Message { get; set; }
    }
}
