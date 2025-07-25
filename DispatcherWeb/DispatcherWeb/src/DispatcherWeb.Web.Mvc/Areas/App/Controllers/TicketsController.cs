using System.Threading.Tasks;
using Abp.Application.Services.Dto;
using Abp.AspNetCore.Mvc.Authorization;
using DispatcherWeb.Authorization;
using DispatcherWeb.Infrastructure.Extensions;
using DispatcherWeb.Tickets;
using DispatcherWeb.Tickets.Dto;
using DispatcherWeb.Web.Areas.App.Models.Shared;
using DispatcherWeb.Web.Controllers;
using DispatcherWeb.Web.Utils;
using Microsoft.AspNetCore.Mvc;

namespace DispatcherWeb.Web.Areas.App.Controllers
{
    [Area("app")]
    [AbpMvcAuthorize]
    public class TicketsController : DispatcherWebControllerBase
    {
        private readonly ITicketAppService _ticketService;

        public TicketsController(
            ITicketAppService ticketService
        )
        {
            _ticketService = ticketService;
        }

        [AbpMvcAuthorize(AppPermissions.Pages_Tickets_View, AppPermissions.CustomerPortal_TicketList, AppPermissions.LeaseHaulerPortal_Tickets)]
        public IActionResult Index()
        {
            return View();
        }

        [AbpMvcAuthorize(AppPermissions.Pages_Tickets_View, AppPermissions.LeaseHaulerPortal_TicketsByDriver)]
        public IActionResult TicketsByDriver()
        {
            return View();
        }

        [Modal]
        [AbpMvcAuthorize(AppPermissions.Pages_Tickets_Edit, AppPermissions.CustomerPortal_TicketList)]
        public async Task<PartialViewResult> CreateOrEditTicketModal(int? id, bool? readOnly)
        {
            var model = await _ticketService.GetTicketEditDto(new NullableIdDto(id));
            model.ReadOnly = readOnly;
            return PartialView("_CreateOrEditTicketModal", model);
        }

        [Modal]
        [AbpMvcAuthorize(AppPermissions.Pages_Tickets_View)]
        public PartialViewResult SelectDriverModal()
        {
            return PartialView("_SelectDriverModal");
        }

        [Modal]
        [AbpMvcAuthorize(AppPermissions.Pages_Tickets_View)]
        public PartialViewResult SelectDateModal()
        {
            return PartialView("_SelectDateModal");
        }

        [AbpMvcAuthorize(AppPermissions.Pages_Tickets_View, AppPermissions.CustomerPortal_TicketList, AppPermissions.LeaseHaulerPortal_Tickets)]
        public async Task<IActionResult> GetTicketPhoto(int id)
        {
            var result = await _ticketService.GetTicketPhoto(id);
            if (result == null)
            {
                return NotFound();
            }

            Response.Headers["Content-Disposition"] = "inline; filename=" + result.FileName.SanitizeFilename();
            return File(result.FileBytes, result.MimeType);
        }

        [AbpMvcAuthorize(AppPermissions.Pages_PrintOrders, AppPermissions.LeaseHaulerPortal_Tickets, AppPermissions.CustomerPortal_TicketList)]
        public async Task<IActionResult> GetTicketPrintOut(GetTicketPrintOutInput input)
        {
            var report = await _ticketService.GetTicketPrintOut(input);
            if (report == null)
            {
                return View("UserFriendlyException", new UserFriendlyExceptionViewModel
                {
                    Message = "Ticket was deleted",
                });
            }
            return InlinePdfFile(report.FileBytes, report.FileName.SanitizeFilename());
        }
    }
}
