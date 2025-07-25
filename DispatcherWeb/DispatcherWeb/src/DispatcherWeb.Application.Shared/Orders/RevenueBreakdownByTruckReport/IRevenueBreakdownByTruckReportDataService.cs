using System.Collections.Generic;
using System.Threading.Tasks;
using Abp.Application.Services;
using DispatcherWeb.Orders.RevenueBreakdownByTruckReport.Dto;

namespace DispatcherWeb.Orders.RevenueBreakdownByTruckReport
{
    public interface IRevenueBreakdownByTruckReportDataService : IApplicationService
    {
        Task<List<RevenueBreakdownByTruckItem>> GetRevenueBreakdownItems(RevenueBreakdownByTruckReportInput input);
    }
}