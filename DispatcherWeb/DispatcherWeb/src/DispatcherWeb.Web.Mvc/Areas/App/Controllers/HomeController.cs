using System.Linq;
using System.Threading.Tasks;
using Abp.AspNetCore.Mvc.Authorization;
using Abp.Domain.Repositories;
using Abp.MultiTenancy;
using DispatcherWeb.Authorization;
using DispatcherWeb.Trucks;
using DispatcherWeb.Web.Controllers;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DispatcherWeb.Web.Areas.App.Controllers
{
    [Area("App")]
    [AbpMvcAuthorize]
    public class HomeController : DispatcherWebControllerBase
    {
        private readonly IRepository<Truck> _truckRepository;

        public HomeController(
            IRepository<Truck> truckRepository
        )
        {
            _truckRepository = truckRepository;
        }

        public async Task<ActionResult> Index()
        {
            if (await AbpSession.GetMultiTenancySideAsync() == MultiTenancySides.Host)
            {
                if (await IsGrantedAsync(AppPermissions.Pages_Administration_Host_Dashboard))
                {
                    return RedirectToAction("Index", "HostDashboard");
                }

                if (await IsGrantedAsync(AppPermissions.Pages_Tenants))
                {
                    return RedirectToAction("Index", "Tenants");
                }
            }
            else
            {
                var hasDashboardPermission = await IsGrantedAsync(AppPermissions.Pages_Dashboard);
                var hasPwaDriverAppPermission = await IsGrantedAsync(AppPermissions.Pages_DriverApplication_WebBasedDriverApp);
                var hasLeaseHaulerPortalPermission = await IsGrantedAsync(AppPermissions.LeaseHaulerPortal_MyCompany);
                var isImpersonating = AbpSession.ImpersonatorUserId.HasValue;

                var redirectRelatedPermissionList = new[]
                {
                    hasDashboardPermission,
                    hasPwaDriverAppPermission,
                    hasLeaseHaulerPortalPermission,
                    isImpersonating,
                };

                if (redirectRelatedPermissionList.Count(x => x == true) == 1)
                {
                    if (hasDashboardPermission)
                    {
                        if (await IsGrantedAsync(AppPermissions.Pages_Schedule))
                        {
                            if (!await (await _truckRepository.GetQueryAsync()).AnyAsync())
                            {
                                return RedirectToAction("Index", "Scheduling");
                            }
                        }

                        return RedirectToAction("Index", "Dashboard");
                    }

                    if (hasPwaDriverAppPermission)
                    {
                        return RedirectToAction("PWA", "DriverApplication");
                    }

                    if (hasLeaseHaulerPortalPermission)
                    {
                        return RedirectToAction("Index", "MyCompany");
                    }
                }
                else if (redirectRelatedPermissionList.Any(x => x == true))
                {
                    return RedirectToAction("ChooseRedirectTarget", "Welcome");
                }
            }

            //Default page if no permission to the pages above
            return RedirectToAction("Index", "Welcome");
        }
    }
}
