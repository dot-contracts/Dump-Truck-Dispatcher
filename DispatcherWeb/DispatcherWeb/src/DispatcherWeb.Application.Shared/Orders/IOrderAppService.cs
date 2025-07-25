using System.Threading.Tasks;
using Abp.Application.Services;
using Abp.Application.Services.Dto;
using DispatcherWeb.Dispatching.Dto;
using DispatcherWeb.Dto;
using DispatcherWeb.Orders.Dto;
using DispatcherWeb.Scheduling.Dto;
using MigraDocCore.DocumentObjectModel;

namespace DispatcherWeb.Orders
{
    public interface IOrderAppService : IApplicationService
    {
        Task<PagedResultDto<OrderDto>> GetOrders(GetOrdersInput input);

        Task<OrderEditDto> GetOrderForEdit(NullableIdDto input);

        Task<EditOrderResult> EditOrder(OrderEditDto model);

        Task<int[]> CopyOrder(CopyOrderInput input);

        Task<OrderInternalNotesDto> GetOrderInternalNotes(EntityDto input);

        Task SetOrderInternalNotes(OrderInternalNotesDto input);

        Task<int> GetOrderDuplicateCount(GetOrderDuplicateCountInput input);

        Task DeleteOrder(EntityDto input);

        Task<PagedResultDto<OrderLineEditDto>> GetOrderLines(GetOrderLinesInput input);

        Task<OrderLineEditDto> GetOrderLineForEdit(GetOrderLineForEditInput input);

        Task<EditOrderLineOutput> EditOrderLine(OrderLineEditDto model);

        Task<DeleteOrderLineOutput> DeleteOrderLine(DeleteOrderLineInput input);

        Task<StaggeredTimesDto> GetStaggeredTimesForEdit(NullableIdDto input);

        Task<PagedResultDto<ReceiptReportDto>> GetReceipts(GetReceiptReportInput input);

        Task<FileDto> ExportReceiptsToExcel(GetReceiptReportInput input);

        Task SetOrderIsBilled(SetOrderIsBilledInput input);

        Task<Document> GetWorkOrderReport(GetWorkOrderReportInput input);

        Task<byte[]> GetOrderSummaryReport(GetOrderSummaryReportInput input);

        Task<byte[]> GetPaymentReconciliationReport(GetPaymentReconciliationReportInput input);

        Task<EmailOrderReportDto> GetEmailReceiptReportModel(EntityDto input);

        Task<EmailOrderReportDto> GetEmailOrderReportModel(EntityDto input);

        Task<EmailOrderReportResult> EmailOrderReport(EmailOrderReportDto input);

        Task SetOrderOfficeId(SetOrderOfficeIdInput input);

        Task<SetOrderDateResult> SetOrderDate(SetOrderDateInput input);

        Task<ListResultDto<SelectListDto>> GetOrderIdsSelectList(GetSelectListInput input);

        Task<JobEditDto> GetJobForEdit(GetJobForEditInput input);

        Task<ValidateOrderLineTimeOnJobResult> ValidateOrderLineTimeOnJob(ValidateOrderLineTimeOnJobInput input);

        Task<ValidateOrderLineTruckTimeOnJobResult> ValidateOrderLineTruckTimeOnJob(ValidateOrderLineTruckTimeOnJobInput input);
    }
}
