using System.Threading.Tasks;
using Abp.Application.Services.Dto;
using Abp.AspNetCore.Mvc.Authorization;
using DispatcherWeb.Authorization;
using DispatcherWeb.Dispatching.Dto;
using DispatcherWeb.DriverAssignments.Dto;
using DispatcherWeb.OrderPayments;
using DispatcherWeb.Orders;
using DispatcherWeb.Orders.Dto;
using DispatcherWeb.Quotes.Dto;
using DispatcherWeb.Web.Areas.App.Models.Orders;
using DispatcherWeb.Web.Controllers;
using DispatcherWeb.Web.Utils;
using Microsoft.AspNetCore.Mvc;

namespace DispatcherWeb.Web.Areas.app.Controllers
{
    [Area("App")]
    [AbpMvcAuthorize]
    public class OrdersController : DispatcherWebControllerBase
    {
        private readonly IOrderAppService _orderAppService;
        private readonly IOrderPaymentAppService _orderPaymentAppService;

        public OrdersController(
            IOrderAppService orderAppService,
            IOrderPaymentAppService orderPaymentAppService
        )
        {
            _orderAppService = orderAppService;
            _orderPaymentAppService = orderPaymentAppService;
        }

        [AbpMvcAuthorize(AppPermissions.Pages_Orders_View)]
        public IActionResult Index()
        {
            return View();
        }

        [AbpMvcAuthorize(AppPermissions.Pages_Orders_Edit)]
        public IActionResult Details()
        {
            return RedirectPermanent("/");
        }

        [AbpMvcAuthorize(AppPermissions.Pages_Reports_Receipts)]
        public IActionResult Receipts()
        {
            return View();
        }

        [Modal]
        [AbpMvcAuthorize(AppPermissions.Pages_Orders_Edit)]
        public async Task<PartialViewResult> CreateOrEditOrderLineModal(int? id, int? orderId)
        {
            var model = await _orderAppService.GetOrderLineForEdit(new GetOrderLineForEditInput(id, orderId));
            return PartialView("_CreateOrEditOrderLineModal", model);
        }

        [Modal]
        [AbpMvcAuthorize(AppPermissions.Pages_Orders_Edit)]
        public PartialViewResult AddQuoteBasedOrderLinesModal(QuoteBasedOrderLinesModalInput input)
        {
            return PartialView("_AddQuoteBasedOrderLinesModal", input);
        }

        [Modal]
        [AbpMvcAuthorize(AppPermissions.Pages_Orders_Edit, AppPermissions.LeaseHaulerPortal_Jobs_Edit)]
        public async Task<PartialViewResult> SetStaggeredTimesModal(int? orderLineId)
        {
            var model = await _orderAppService.GetStaggeredTimesForEdit(new NullableIdDto(orderLineId));
            return PartialView("_SetStaggeredTimesModal", model);
        }

        [Modal]
        [AbpMvcAuthorize(AppPermissions.Pages_Orders_Edit, AppPermissions.LeaseHaulerPortal_Jobs_Edit, AppPermissions.LeaseHaulerPortal_Truck_Request)]
        public async Task<PartialViewResult> CreateOrEditTicketModal(int orderLineId)
        {
            var orderLine = await _orderAppService.GetOrderLineForEdit(new GetOrderLineForEditInput(orderLineId, null));
            var model = new CreateOrEditTicketModalViewModel
            {
                OrderLineId = orderLineId,
                OrderLineDesignation = orderLine.Designation,
                OrderLineIsProductionPay = orderLine.ProductionPay,
            };
            return PartialView("_CreateOrEditTicketModal", model);
        }

        [Modal]
        [AbpMvcAuthorize(AppPermissions.Pages_Orders_Edit)]
        public async Task<PartialViewResult> CopyOrderModal(CopyOrderInput model)
        {
            var today = await GetToday();
            model.DateBegin = today;
            model.DateEnd = today;
            return PartialView("_CopyOrderModal", model);
        }

        [Modal]
        [AbpMvcAuthorize(AppPermissions.Pages_Orders_Edit)]
        public async Task<PartialViewResult> CarryUnfinishedPortionForwardModal(CopyOrderInput model)
        {
            var today = await GetToday();
            model.DateBegin = today;
            model.DateEnd = today; // For consistency, though this will be hidden in the UI
            return PartialView("_CarryUnfinishedPortionForwardModal", model);
        }

        [Modal]
        [AbpMvcAuthorize(AppPermissions.Pages_Orders_View)]
        public PartialViewResult CreateQuoteFromOrderModal(int orderId)
        {
            return PartialView("_CreateQuoteFromOrderModal", new CreateQuoteFromOrderInput { OrderId = orderId });
        }

        [Modal]
        [AbpMvcAuthorize(AppPermissions.Pages_Schedule)]
        public PartialViewResult SetNoDriverForTruckModal(SetNoDriverForTruckInput model)
        {
            return PartialView("_SetNoDriverForTruckModal", model);
        }

        [Modal]
        [AbpMvcAuthorize(AppPermissions.Pages_Schedule)]
        public PartialViewResult SetDefaultDriverForTruckModal(SetDefaultDriverForTruckInput model)
        {
            return PartialView("_SetDefaultDriverForTruckModal", model);
        }

        [AbpMvcAuthorize(AppPermissions.Pages_PrintOrders)]
        public async Task<FileContentResult> GetWorkOrderReport(GetWorkOrderReportInput input)
        {
            var report = await _orderAppService.GetWorkOrderReport(input);
            return InlinePdfFile(report.SaveToBytesArray(), "WorkOrderReport.pdf");
        }

        [AbpMvcAuthorize(AppPermissions.Pages_PrintOrders)]
        public async Task<FileContentResult> GetOrderSummaryReport(GetOrderSummaryReportInput input)
        {
            var report = await _orderAppService.GetOrderSummaryReport(input);
            return InlinePdfFile(report, "OrderSummaryReport.pdf");
        }

        [AbpMvcAuthorize(AppPermissions.Pages_Reports_PaymentReconciliation)]
        public async Task<FileContentResult> GetPaymentReconciliationReport(GetPaymentReconciliationReportInput input)
        {
            var report = await _orderAppService.GetPaymentReconciliationReport(input);
            return InlinePdfFile(report, "PaymentReconciliationReport.pdf");
        }

        [AbpMvcAuthorize(AppPermissions.Pages_PrintOrders)]
        public PartialViewResult PrintOrdersWithDeliveryInfoModal()
        {
            return PartialView("_PrintOrdersWithDeliveryInfoModal");
        }

        [AbpMvcAuthorize(AppPermissions.Pages_PrintOrders)]
        public PartialViewResult PrintOrderWithDeliveryInfoModal(EntityDto model)
        {
            return PartialView("_PrintOrderWithDeliveryInfoModal", model);
        }

        [AbpMvcAuthorize(AppPermissions.Pages_PrintOrders)]
        public PartialViewResult SpecifyPrintOptionsModal()
        {
            return PartialView("_SpecifyPrintOptionsModal");
        }

        [AbpMvcAuthorize(AppPermissions.Pages_SendOrdersToDrivers)]
        public PartialViewResult SendOrdersToDriversModal(SendOrdersToDriversModalInput input)
        {
            return PartialView("_SendOrdersToDriversModal", input);
        }

        [AbpMvcAuthorize(AppPermissions.Pages_Reports_PaymentReconciliation)]
        public async Task<PartialViewResult> PrintPaymentReconciliationReportModal()
        {
            var today = await GetToday();
            return PartialView("_PrintPaymentReconciliationReportModal", new GetPaymentReconciliationReportInput
            {
                StartDate = today,
                EndDate = today,
            });
        }

        [Modal]
        [AbpMvcAuthorize(AppPermissions.Pages_Orders_Edit)]
        public async Task<PartialViewResult> CreateOrEditOrderInternalNotesModal(int id)
        {
            var model = await _orderAppService.GetOrderInternalNotes(new EntityDto(id));
            return PartialView("_CreateOrEditOrderInternalNotesModal", model);
        }

        [Modal]
        [AbpMvcAuthorize(AppPermissions.Pages_Orders_Edit)]
        public async Task<PartialViewResult> AuthorizeOrderChargeModal(int id)
        {
            var model = await _orderPaymentAppService.GetAuthorizeOrderChargeModel(new EntityDto(id));
            return PartialView("_AuthorizeOrderChargeModal", model);
        }

        [Modal]
        [AbpMvcAuthorize(AppPermissions.Pages_Orders_Edit)]
        public async Task<PartialViewResult> CaptureOrderAuthorizationModal(int id)
        {
            var model = await _orderPaymentAppService.GetCaptureOrderAuthorizationModel(new EntityDto(id));
            return PartialView("_CaptureOrderAuthorizationModal", model);
        }

        [Modal]
        [AbpMvcAuthorize(AppPermissions.Pages_Orders_View)]
        public async Task<PartialViewResult> EmailOrderReportModal(int id, bool useReceipts = false)
        {
            var model = useReceipts
                ? await _orderAppService.GetEmailReceiptReportModel(new EntityDto(id))
                : await _orderAppService.GetEmailOrderReportModel(new EntityDto(id));
            return PartialView("_EmailOrderReportModal", model);
        }

        [Modal]
        [AbpMvcAuthorize(AppPermissions.Pages_Orders_View)]
        public PartialViewResult SelectOrderQuoteModal()
        {
            return PartialView("_SelectOrderQuoteModal");
        }

        [Modal]
        [AbpMvcAuthorize(AppPermissions.Pages_Orders_Edit, AppPermissions.LeaseHaulerPortal_Jobs_Edit, AppPermissions.LeaseHaulerPortal_Truck_Request)]
        public async Task<PartialViewResult> CreateOrEditJobModal(GetJobForEditInput input)
        {
            var model = await _orderAppService.GetJobForEdit(input);
            return PartialView("_CreateOrEditJobModal", model);
        }

        [Modal]
        [AbpMvcAuthorize(AppPermissions.Pages_Orders_Edit)]
        public async Task<PartialViewResult> CreateOrEditOrderModal(int? id)
        {
            var model = await _orderAppService.GetOrderForEdit(new NullableIdDto(id));
            return PartialView("_CreateOrEditOrderModal", model);
        }

        [Modal]
        [AbpMvcAuthorize(AppPermissions.Pages_TicketsByDriver)]
        public PartialViewResult SelectOrderLineToMoveTicketsByDriverToModal(SelectOrderLineToMoveTicketsByDriverToDto model)
        {
            return PartialView("_SelectOrderLineToMoveTicketsByDriverToModal", model);
        }
    }
}
