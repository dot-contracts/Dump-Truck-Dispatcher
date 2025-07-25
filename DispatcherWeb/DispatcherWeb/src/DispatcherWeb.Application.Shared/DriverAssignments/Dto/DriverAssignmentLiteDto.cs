using System;

namespace DispatcherWeb.DriverAssignments.Dto
{
    public class DriverAssignmentLiteDto
    {
        public int Id { get; set; }

        public int? OfficeId { get; set; }

        public Shift? Shift { get; set; }

        public DateTime Date { get; set; }

        public int TruckId { get; set; }

        public string TruckCode { get; set; }

        public int? TruckLeaseHaulerId { get; set; }

        public string DriverName { get; set; }

        public string DriverFirstName { get; set; }

        public string DriverLastName { get; set; }

        public int? DriverId { get; set; }

        public DateTime? StartTime { get; set; }
        public bool DriverIsExternal { get; set; }
        public bool DriverIsActive { get; set; }

        public virtual T CopyTo<T>(T other) where T : DriverAssignmentLiteDto
        {
            other.Id = Id;
            other.OfficeId = OfficeId;
            other.Shift = Shift;
            other.Date = Date;
            other.TruckId = TruckId;
            other.TruckCode = TruckCode;
            other.TruckLeaseHaulerId = TruckLeaseHaulerId;
            other.DriverName = DriverName;
            other.DriverFirstName = DriverFirstName;
            other.DriverLastName = DriverLastName;
            other.DriverId = DriverId;
            other.StartTime = StartTime;
            other.DriverIsExternal = DriverIsExternal;
            other.DriverIsActive = DriverIsActive;
            return other;
        }
    }
}
