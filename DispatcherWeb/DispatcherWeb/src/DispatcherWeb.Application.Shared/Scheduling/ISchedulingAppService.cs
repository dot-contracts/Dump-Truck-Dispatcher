using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Abp.Application.Services;
using Abp.Application.Services.Dto;
using DispatcherWeb.Dto;
using DispatcherWeb.LeaseHaulers.Dto;
using DispatcherWeb.Orders.Dto;
using DispatcherWeb.Scheduling.Dto;

namespace DispatcherWeb.Scheduling
{
    public interface ISchedulingAppService : IApplicationService
    {
        Task<ListResultDto<ScheduleTruckDto>> GetScheduleTrucks(GetScheduleTrucksInput input);

        Task<PagedResultDto<ScheduleOrderLineDto>> GetScheduleOrders(GetScheduleOrdersInput input);

        Task<AddOrderTruckResult> AddOrderLineTruck(AddOrderLineTruckInput input);

        Task<DeleteOrderLineTruckResult> DeleteOrderLineTruck(DeleteOrderLineTruckInput input);

        Task<CopyOrderTrucksResult> CopyOrderTrucks(CopyOrderTrucksInput input);

        Task SetOrderLineTime(SetOrderLineTimeInput input);

        Task SetOrderLineIsComplete(SetOrderLineIsCompleteInput input);

        Task SetOrderLineIsCompleteBatch(SetOrderLineIsCompleteBatchInput input);

        Task SetLeaseHaulerRequestAsAccepted(SetLeaseHaulerRequestAsAcceptedInput input);

        Task<SetOrderOfficeIdInput> GetOrderOfficeIdForEdit(EntityDto input);

        Task<SetOrderDirectionsInput> GetOrderDirectionsForEdit(EntityDto input);

        Task<SetOrderLineNoteInput> GetOrderLineNoteForEdit(NullableIdDto input);

        Task<OrderLineTruckDetailsDto> GetOrderTruckUtilizationForEdit(EntityDto input);

        Task SetOrderTruckUtilization(OrderLineTruckUtilizationEditDto input);

        Task<IList<OrderLineTruckDto>> GetOrderLineTrucks(int orderLineId);

        Task<SetOrderLineMaterialQuantityResult> SetOrderLineMaterialQuantity(SetOrderLineMaterialQuantityInput input);

        Task<SetOrderLineFreightQuantityResult> SetOrderLineFreightQuantity(SetOrderLineFreightQuantityInput input);

        Task<SetOrderDateInput> GetSetOrderDateInput(int orderLineId);

        Task<SetOrderLineLoadsResult> SetOrderLineLoads(SetOrderLineLoadsInput input);

        Task<HasDispatchesResult> HasDispatches(DeleteOrderLineTruckInput input);

        Task<DateTime?> GetStartTimeForTruckOrderLines(GetTruckOrdersInput input);

        Task<IList<SelectListDto>> GetClosedTrucksSelectList(int orderLineId);

        Task<bool> ActivateClosedTrucks(ActivateClosedTrucksInput input);

        Task<string> GetDeviceIdsStringForOrderLineTrucks(int orderLineId);

        Task<SetOrderLineScheduledTrucksResult> SetOrderLineScheduledTrucks(SetOrderLineScheduledTrucksInput input);

        Task<CopyOrderTrucksResult> CopyOrdersTrucks(CopyOrdersTrucksInput input);

        Task<MoveTruckResult> MoveTruck(MoveTruckInput input);

        Task<PagedResultDto<SelectListDto>> GetOrderLinesToAssignTrucksToSelectList(GetSelectListIdInput input);

        Task<IList<SelectListDto>> GetTrucksSelectList(int orderLineId);

        Task ReassignTrucks(ReassignTrucksInput input);

        Task<HasDispatchesResult> SomeOrderLineTrucksHaveDispatches(SomeOrderLineTrucksHaveDispatchesInput input);

        Task<OrderLineTruckToChangeDriverDto> GetOrderLineTruckToChangeDriverModel(int orderLineTruckId);

        Task<SendOrderLineToHaulingCompanyInput> GetInputForSendOrderLineToHaulingCompany(int orderLineId);

        Task DeleteRequestedLeaseHaulerTruck(int id);

        Task<FileDto> GetScheduleOrdersToCsv(GetScheduleOrdersInput input);
    }
}
