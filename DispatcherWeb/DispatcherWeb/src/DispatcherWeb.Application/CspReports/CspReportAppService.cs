using Abp.Authorization;
using DispatcherWeb.CspReports.Dto;

namespace DispatcherWeb.CspReports
{
    [AbpAllowAnonymous]
    public class CspReportAppService : DispatcherWebAppServiceBase, ICspReportAppService
    {
        [AbpAllowAnonymous]
        public void PostReport(PostReportDto postReport)
        {
            // Nothing here. Look Audit Logs for data
        }
    }
}
