using System;
using DispatcherWeb.Authorization;

namespace DispatcherWeb.Application.Infrastructure.Utilities
{
    public static class ReportTypeHelper
    {
        public static string GetPermissionName(this ReportType reportType)
        {
            switch (reportType)
            {
                case ReportType.OutOfServiceTrucks:
                    return AppPermissions.Pages_Reports_OutOfServiceTrucks;
                default:
                    throw new ArgumentOutOfRangeException($"Wrong ReportType: {reportType}");
            }
        }


    }
}
