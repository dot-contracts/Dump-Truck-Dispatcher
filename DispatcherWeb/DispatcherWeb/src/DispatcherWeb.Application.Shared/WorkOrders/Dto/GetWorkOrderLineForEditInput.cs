using Abp.Application.Services.Dto;

namespace DispatcherWeb.WorkOrders.Dto
{
    public class GetWorkOrderLineForEditInput : NullableIdDto
    {
        public GetWorkOrderLineForEditInput(int? id, int workOrderId) : base(id)
        {
            WorkOrderId = workOrderId;
        }
        public int WorkOrderId { get; set; }
    }
}
