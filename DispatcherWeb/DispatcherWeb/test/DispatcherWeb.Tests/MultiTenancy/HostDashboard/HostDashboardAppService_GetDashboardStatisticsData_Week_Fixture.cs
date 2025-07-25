using System;

namespace DispatcherWeb.Tests.MultiTenancy.HostDashboard
{
    public class HostDashboardAppService_GetDashboardStatisticsData_Week_Fixture : HostDashboardAppService_GetDashboardStatisticsData_Fixture
    {
        protected override (DateTime startDate, DateTime endDate) GetInterval()
        {
            var date = DateTime.UtcNow.Date.AddDays(-8);
            return (date, date.AddDays(8).AddMilliseconds(-1));
        }
    }
}
