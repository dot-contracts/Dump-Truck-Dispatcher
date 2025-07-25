using System.Threading.Tasks;
using Abp.AspNetCore.Mvc.Authorization;
using DispatcherWeb.Authorization;
using DispatcherWeb.LeaseHaulers;
using DispatcherWeb.Web.Areas.App.Models.Welcome;
using DispatcherWeb.Web.Controllers;
using Microsoft.AspNetCore.Mvc;

namespace DispatcherWeb.Web.Areas.App.Controllers
{
    [Area("App")]
    [AbpMvcAuthorize]
    public class WelcomeController : DispatcherWebControllerBase
    {
        private readonly ILeaseHaulerAppService _leaseHaulerAppService;

        public WelcomeController(
            ILeaseHaulerAppService leaseHaulerAppService)
        {
            _leaseHaulerAppService = leaseHaulerAppService;
        }

        public async Task<ActionResult> Index()
        {
            var model = new WelcomeViewModel(L("WelcomePage_Info"));
            if (await IsGrantedAsync(AppPermissions.LeaseHaulerPortal))
            {
                var leaseHaulerCompany = await _leaseHaulerAppService.GetLeaseHaulerCompanyName();
                model = new WelcomeViewModel(L("WelcomePage_LeaseHaulerPortal", leaseHaulerCompany));
            }
            else if (await IsGrantedAsync(AppPermissions.Pages_DriverApplication))
            {
                model.DetailsMessage = L("WelcomePage_DriverInfo");
            }
            return View(model);
        }
        public ActionResult ChooseRedirectTarget()
        {
            return View();
        }
    }
}