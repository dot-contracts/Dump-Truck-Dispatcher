using System;

namespace DispatcherWeb.LeaseHaulers.Dto.CrossTenantOrderSender
{
    public class TruckDto
    {
        public int Id { get; set; }
        public int TenantId { get; set; }
        public string TruckCode { get; set; }
        public int VehicleCategoryId { get; set; }
        public bool IsActive { get; set; }
        public DriverDto DefaultDriver { get; set; }
        public DateTime? InactivationDate { get; set; }
        public bool CanPullTrailer { get; set; }
    }
}
