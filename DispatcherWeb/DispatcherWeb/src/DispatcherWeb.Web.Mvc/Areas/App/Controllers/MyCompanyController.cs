using System.Threading.Tasks;
using Abp.AspNetCore.Mvc.Authorization;
using DispatcherWeb.Authorization;
using DispatcherWeb.LeaseHaulerUsers;
using DispatcherWeb.Web.Controllers;
using Microsoft.AspNetCore.Mvc;

namespace DispatcherWeb.Web.Areas.App.Controllers;

[Area("App")]
[AbpMvcAuthorize(AppPermissions.LeaseHaulerPortal)]
public class MyCompanyController : DispatcherWebControllerBase
{
    private readonly ILeaseHaulerUserAppService _leaseHaulerUserService;

    public MyCompanyController(
        ILeaseHaulerUserAppService leaseHaulerUserService)
    {
        _leaseHaulerUserService = leaseHaulerUserService;
    }

    [AbpMvcAuthorize(AppPermissions.LeaseHaulerPortal_MyCompany)]
    public async Task<IActionResult> Index()
    {
        var model = await _leaseHaulerUserService.GetLeaseHaulerByUser();
        return View(model);
    }
}