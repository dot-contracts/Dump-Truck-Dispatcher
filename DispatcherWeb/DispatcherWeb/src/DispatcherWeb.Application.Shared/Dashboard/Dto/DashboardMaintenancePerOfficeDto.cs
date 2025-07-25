using System;
using System.Collections.Generic;

namespace DispatcherWeb.Dashboard.Dto
{
    public class DashboardMaintenancePerOfficeDto
    {
        public int OfficeId { get; set; }
        public string OfficeName { get; set; }
        public int NumberOfTrucksOutOfService { get; set; }
        public int NumberOfTrucksOverdueForService { get; set; }
        public int NumberOfTrucksDueForService { get; set; }
        public List<OutOfServiceDto> OutOfService { get; set; }
        public int NumberOfTrucksWithPlateExpiringThisMonth { get; set; }

        public class OutOfServiceDto
        {
            public string TruckCode { get; set; }
            public DateTime OutOfServiceDate { get; set; }
            public int OutOfServiceDays { get; set; }
            public string Reason { get; set; }
            public int OutOfHistoryId { get; set; }
        }
    }
}
