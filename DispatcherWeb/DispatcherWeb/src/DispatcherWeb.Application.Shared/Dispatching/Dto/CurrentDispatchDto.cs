using System;

namespace DispatcherWeb.Dispatching.Dto
{
    public class CurrentDispatchDto
    {
        public int TenantId { get; set; }
        public int TruckId { get; set; }
        public int DriverId { get; set; }
        public DateTime? OrderDate { get; set; }
        public DispatchStatus Status { get; set; }
        public bool IsDeleted { get; set; }
    }
}
