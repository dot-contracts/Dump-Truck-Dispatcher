using System.Collections.Generic;
using System.Threading.Tasks;
using DispatcherWeb.Orders.RevenueBreakdownByTruckReport.Dto;
using DispatcherWeb.Orders.RevenueBreakdownReport.Dto;

namespace DispatcherWeb.Orders.RevenueBreakdownReport
{
    public interface IRevenueBreakdownTimeCalculator
    {
        Task FillDriversTimeForTrucks(List<RevenueBreakdownByTruckItem> items, RevenueBreakdownByTruckReportInput input);
        Task FillDriversTimeForOrderLines(List<RevenueBreakdownItem> items, RevenueBreakdownReportInput input);
        Task FillDriversTime(FillDriversTimeInput input, FillDriverTimeCallback fillTimeCallback);
        Task<bool> HaveTimeWithNoEndDate(FillDriversTimeInput input);
        Task FillDriversTimeAsync(FillDriversTimeInput input, FillDriverTimeAsyncCallback fillTimeCallback);
    }
}
