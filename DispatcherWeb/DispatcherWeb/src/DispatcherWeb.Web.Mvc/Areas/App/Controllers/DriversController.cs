using System.Threading.Tasks;
using Abp.AspNetCore.Mvc.Authorization;
using DispatcherWeb.Authorization;
using DispatcherWeb.Drivers;
using DispatcherWeb.Dto;
using DispatcherWeb.Web.Controllers;
using Microsoft.AspNetCore.Mvc;

namespace DispatcherWeb.Web.Mvc.Areas.App.Controllers
{
    [Area("App")]
    [AbpMvcAuthorize(AppPermissions.Pages_Drivers)]
    public class DriversController : DispatcherWebControllerBase
    {
        private readonly IDriverAppService _driverAppService;

        public DriversController(
            IDriverAppService driverAppService)
        {
            _driverAppService = driverAppService;
        }

        public IActionResult Index()
        {
            return View();
        }

        public async Task<PartialViewResult> CreateOrEditModal(NullableIdNameDto input)
        {
            var model = await _driverAppService.GetDriverForEdit(input);
            return PartialView("_CreateOrEditModal", model);
        }
    }
}