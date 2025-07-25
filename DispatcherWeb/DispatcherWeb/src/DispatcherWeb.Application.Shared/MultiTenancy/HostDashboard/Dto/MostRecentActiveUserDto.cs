using System;

namespace DispatcherWeb.MultiTenancy.HostDashboard.Dto
{
    public class MostRecentActiveUserDto
    {
        public string TenancyName { get; set; }
        public string FullName { get; set; }
        public DateTime LastTransaction { get; set; }
        public int NumberOfTransactions { get; set; }

    }
}
