using System;

namespace DispatcherWeb.Drivers.Dto
{
    public class DriverDto
    {
        public int Id { get; set; }

        public string FirstName { get; set; }

        public string LastName { get; set; }

        public string OfficeName { get; set; }

        public bool IsInactive { get; set; }

        public string LicenseNumber { get; set; }

        public string LicenseState { get; set; }

        public string TypeOfLicense { get; set; }

        public DateTime? LicenseExpirationDate { get; set; }

        public DateTime? LastPhysicalDate { get; set; }

        public DateTime? NextPhysicalDueDate { get; set; }

        public DateTime? LastMvrDate { get; set; }

        public DateTime? NextMvrDueDate { get; set; }

        public DateTime? DateOfHire { get; set; }

        public DateTime? TerminationDate { get; set; }

        public string FullName => $"{FirstName} {LastName}".Trim();

        public string EmployeeId { get; set; }
    }
}
