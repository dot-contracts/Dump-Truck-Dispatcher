using System;

namespace DispatcherWeb.Drivers.Dto
{
    public class DriverCompanyDto
    {
        public int DriverId { get; set; }
        public string CompanyName { get; set; }
        public bool IsActive { get; set; }
        public DateTime? DateOfHire { get; set; }
        public DateTime? TerminationDate { get; set; }
    }
}
