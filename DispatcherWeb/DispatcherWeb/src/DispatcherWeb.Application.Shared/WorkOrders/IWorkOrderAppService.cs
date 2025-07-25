using System.Threading.Tasks;
using Abp.Application.Services.Dto;
using DispatcherWeb.WorkOrders.Dto;

namespace DispatcherWeb.WorkOrders
{
    public interface IWorkOrderAppService
    {
        Task<WorkOrderEditDto> GetWorkOrderForEdit(NullableIdDto input);
        Task<WorkOrderEditDto> SaveWorkOrder(WorkOrderEditDto model);
        Task<PagedResultDto<WorkOrderLineDto>> GetWorkOrderLines(GetWorkOrderLinesInput input);
        Task<WorkOrderLineEditDto> GetWorkOrderLineForEdit(GetWorkOrderLineForEditInput input);
        Task<SaveWorkOrderLineResult> SaveWorkOrderLine(WorkOrderLineEditDto model);
        Task<WorkOrderPictureEditDto> SavePicture(WorkOrderPictureEditDto model);
        Task<WorkOrderPictureEditDto> GetPictureEditDto(int id);
        Task CreateWorkOrdersFromPreventiveMaintenance(CreateWorkOrdersFromPreventiveMaintenanceInput input);
    }
}