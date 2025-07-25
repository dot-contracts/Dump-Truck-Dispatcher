using System;

namespace DispatcherWeb.DriverApplication.Dto
{
    public class GetDriverGuidResult
    {
        public long UserId { get; set; }
        public bool IsAdmin { get; set; }
        public bool IsDriver { get; set; }
        public Guid DriverGuid { get; set; }
        public int DriverId { get; set; }
        public string DriverName { get; set; }
        public int? DriverLeaseHaulerId { get; set; }
    }
}
