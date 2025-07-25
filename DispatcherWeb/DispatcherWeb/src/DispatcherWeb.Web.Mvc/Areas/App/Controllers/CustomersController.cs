using System.Threading.Tasks;
using Abp.AspNetCore.Mvc.Authorization;
using DispatcherWeb.Authorization;
using DispatcherWeb.Customers;
using DispatcherWeb.Customers.Dto;
using DispatcherWeb.Dto;
using DispatcherWeb.Web.Controllers;
using Microsoft.AspNetCore.Mvc;

namespace DispatcherWeb.Web.Mvc.Areas.App.Controllers
{
    [Area("app")]
    [AbpMvcAuthorize(AppPermissions.Pages_Customers)]
    public class CustomersController : DispatcherWebControllerBase
    {
        private readonly ICustomerAppService _customerAppService;

        public CustomersController(ICustomerAppService customerAppService)
        {
            _customerAppService = customerAppService;
        }

        public IActionResult Index()
        {
            return View();
        }

        public async Task<PartialViewResult> CreateOrEditCustomerModal(NullableIdNameDto input)
        {
            var model = await _customerAppService.GetCustomerForEdit(input);
            return PartialView("_CreateOrEditCustomerModal", model);
        }

        public async Task<PartialViewResult> CreateOrEditCustomerContactModal(GetCustomerContactForEditInput input)
        {
            var model = await _customerAppService.GetCustomerContactForEdit(input);
            return PartialView("_CreateOrEditCustomerContactModal", model);
        }
    }
}