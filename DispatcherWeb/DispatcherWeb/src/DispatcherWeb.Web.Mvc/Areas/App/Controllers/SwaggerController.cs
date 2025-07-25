using Abp.AspNetCore.Mvc.Authorization;
using DispatcherWeb.Authorization;
using DispatcherWeb.Web.Controllers;
using Microsoft.AspNetCore.Mvc;

namespace DispatcherWeb.Web.Areas.App.Controllers
{
    [Area("App")]
    [AbpMvcAuthorize(AppPermissions.Pages_SwaggerAccess)]
    public class SwaggerController : DispatcherWebControllerBase
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
