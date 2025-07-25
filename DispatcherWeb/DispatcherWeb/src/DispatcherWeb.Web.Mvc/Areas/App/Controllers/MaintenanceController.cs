using System.Threading.Tasks;
using Abp.AspNetCore.Mvc.Authorization;
using DispatcherWeb.Authorization;
using DispatcherWeb.Caching;
using DispatcherWeb.Web.Areas.App.Models.Maintenance;
using DispatcherWeb.Web.Controllers;
using Microsoft.AspNetCore.Mvc;

namespace DispatcherWeb.Web.Areas.App.Controllers
{
    [Area("App")]
    [AbpMvcAuthorize(AppPermissions.Pages_Administration_Host_Maintenance)]
    public class MaintenanceController : DispatcherWebControllerBase
    {
        private readonly ICachingAppService _cachingAppService;

        public MaintenanceController(ICachingAppService cachingAppService)
        {
            _cachingAppService = cachingAppService;
        }

        public async Task<ActionResult> Index()
        {
            var model = new MaintenanceViewModel
            {
                Caches = (await _cachingAppService.GetAllCaches()).Items,
            };

            return View(model);
        }
    }
}
