using System.Threading.Tasks;
using DispatcherWeb.Infrastructure.Reports.Dto;

namespace DispatcherWeb.Infrastructure.Reports
{
    public interface IScheduledReportGeneratorAppService
    {
        Task GenerateReport(ScheduledReportGeneratorInput scheduledReportGeneratorInput);
    }
}