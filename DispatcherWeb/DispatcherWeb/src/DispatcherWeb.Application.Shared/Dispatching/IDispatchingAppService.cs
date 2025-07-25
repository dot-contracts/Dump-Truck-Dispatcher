using System.Collections.Generic;
using System.Threading.Tasks;
using Abp.Application.Services.Dto;
using DispatcherWeb.Dispatching.Dto;
using MigraDocCore.DocumentObjectModel;

namespace DispatcherWeb.Dispatching
{
    public interface IDispatchingAppService
    {
        Task SendDispatchMessage(SendDispatchMessageInput input);
        Task<DispatchInfoBaseDto> GetDispatchInfo(GetDispatchInfoInput input);
        Task LoadDispatch(LoadDispatchInput dispatchTicket);
        Task<CompleteDispatchResult> CompleteDispatch(CompleteDispatchDto completeDispatch);
        Task CancelDispatch(CancelDispatchDto cancelDispatch);
        Task CancelOrEndAllDispatches(CancelOrEndAllDispatchesInput input);

        Task<ViewDispatchDto> ViewDispatch(int dispatchId);
        Task<SetDispatchTimeOnJobDto> GetDispatchTimeOnJob(int dispatchId);
        Task<List<TruckDispatchListItemDto>> TruckDispatchList(TruckDispatchListInput input);
        Task<bool> CanAddDispatchBasedOnTime(CanAddDispatchBasedOnTimeInput input);
        Task<SendDispatchMessageDto> CreateSendDispatchMessageDto(int orderLineId, bool firstDispatchForDay = false);
        Task DuplicateDispatch(DuplicateDispatchInput input);
        Task SendOrdersToDrivers(SendOrdersToDriversInput input);
        Task CancelDispatches(CancelDispatchesInput input);
        Task<bool> GetDispatchTruckStatus(int dispatchId);
        Task AcknowledgeDispatch(AcknowledgeDispatchInput input);
        Task<PagedResultDto<DispatchListDto>> GetDispatchPagedList(GetDispatchPagedListInput input);
        Task<Document> GetDriverActivityDetailReport(GetDriverActivityDetailReportInput input);
        Task<GetOrderTotalsResult> GetOrderTotalsAsync(int orderLineId);
        Task NotifyDispatchersAfterTicketUpdateIfNeeded(int orderLineId, GetOrderTotalsResult orderTotalsBeforeUpdate);
        Task RunPostDispatchCompletionLogic(int dispatchId);
    }
}
