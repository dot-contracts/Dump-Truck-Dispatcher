using System.Threading.Tasks;
using DispatcherWeb.Orders.RevenueAnalysisReport.Dto;

namespace DispatcherWeb.Orders.RevenueAnalysisReport
{
    public interface IRevenueAnalysisReportAppService
    {
        Task<RevenueAnalysisReportOutput> GetRevenueAnalysis(RevenueAnalysisReportInput input);
    }
}