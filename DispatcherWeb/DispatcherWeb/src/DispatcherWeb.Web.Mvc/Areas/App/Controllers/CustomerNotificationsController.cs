using System.Threading.Tasks;
using Abp.Application.Services.Dto;
using Abp.AspNetCore.Mvc.Authorization;
using DispatcherWeb.Authorization;
using DispatcherWeb.CustomerNotifications;
using DispatcherWeb.Web.Controllers;
using DispatcherWeb.Web.Utils;
using Microsoft.AspNetCore.Mvc;

namespace DispatcherWeb.Web.Areas.App.Controllers
{
    [Area("app")]
    [AbpMvcAuthorize(AppPermissions.Pages_CustomerNotifications)]
    public class CustomerNotificationsController : DispatcherWebControllerBase
    {
        private readonly ICustomerNotificationAppService _customerNotificationAppService;

        public CustomerNotificationsController(
            ICustomerNotificationAppService customerNotificationAppService
        )
        {
            _customerNotificationAppService = customerNotificationAppService;
        }

        public IActionResult Index()
        {
            return View();
        }

        [Modal]
        public async Task<PartialViewResult> CreateOrEditCustomerNotificationModal(NullableIdDto input)
        {
            var model = await _customerNotificationAppService.GetCustomerNotificationForEdit(input);
            return PartialView("_CreateOrEditCustomerNotificationModal", model);
        }
    }
}
