using DispatcherWeb.CspReports.Dto;

namespace DispatcherWeb.CspReports
{
    public interface ICspReportAppService
    {
        void PostReport(PostReportDto postReport);
    }
}