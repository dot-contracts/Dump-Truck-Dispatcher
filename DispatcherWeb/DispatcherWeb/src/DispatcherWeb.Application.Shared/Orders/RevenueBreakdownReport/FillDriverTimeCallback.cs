using System.Threading.Tasks;
using DispatcherWeb.Orders.RevenueBreakdownReport.Dto;

namespace DispatcherWeb.Orders.RevenueBreakdownReport
{
    public delegate void FillDriverTimeCallback(FillDriversTimeCallbackArgs e);
    public delegate Task FillDriverTimeAsyncCallback(FillDriversTimeCallbackArgs e);
}
