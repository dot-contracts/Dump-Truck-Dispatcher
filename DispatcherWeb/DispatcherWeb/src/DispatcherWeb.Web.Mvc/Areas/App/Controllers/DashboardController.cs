using System;
using System.Threading.Tasks;
using Abp.AspNetCore.Mvc.Authorization;
using Abp.UI;
using DispatcherWeb.Authorization;
using DispatcherWeb.Dashboard;
using DispatcherWeb.Dashboard.Dto;
using DispatcherWeb.Web.Areas.App.Models.Shared;
using DispatcherWeb.Web.Controllers;
using Microsoft.AspNetCore.Mvc;

namespace DispatcherWeb.Web.Areas.App.Controllers
{
    [Area("App")]
    [AbpMvcAuthorize(AppPermissions.Pages_Dashboard)]
    public class DashboardController : DispatcherWebControllerBase
    {
        private readonly IDashboardAppService _dashboardService;

        public DashboardController(IDashboardAppService dashboardService)
        {
            _dashboardService = dashboardService;
        }

        public ActionResult Index()
        {
            return View();
        }

        public async Task<PartialViewResult> ScheduledTruckCount()
        {
            try
            {
                var model = await _dashboardService.GetScheduledTruckCountDto();
                return PartialView("_ScheduledTruckCount", model);
            }
            catch (UserFriendlyException ex)
            {
                return PartialView("UserFriendlyException", new UserFriendlyExceptionViewModel(ex));
            }
        }

        [AbpMvcAuthorize(AppPermissions.Pages_Dashboard_Revenue)]
        public async Task<PartialViewResult> RevenueCharts(int ticketType, DateTime periodBegin, DateTime periodEnd)
        {
            try
            {
                var model = await _dashboardService.GetRevenueChartsData(new GetRevenueChartsDataInput
                {
                    TicketType = (TicketType)ticketType,
                    PeriodBegin = periodBegin,
                    PeriodEnd = periodEnd,
                });
                return PartialView("_RevenueCharts", model);
            }
            catch (UserFriendlyException ex)
            {
                return PartialView("UserFriendlyException", new UserFriendlyExceptionViewModel(ex));
            }
        }

        public async Task<PartialViewResult> EditDashboardSettingsModal()
        {
            var model = await _dashboardService.GetDashboardSettings();
            return PartialView("_EditDashboardSettingsModal", model);
        }

        public PartialViewResult GettingStarted()
        {
            return PartialView("_GettingStarted");
        }
    }
}
