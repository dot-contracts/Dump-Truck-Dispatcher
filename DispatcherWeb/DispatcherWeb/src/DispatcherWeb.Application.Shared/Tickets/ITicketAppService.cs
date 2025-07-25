using System.Threading.Tasks;
using Abp.Application.Services.Dto;
using DispatcherWeb.Dto;
using DispatcherWeb.Tickets.Dto;

namespace DispatcherWeb.Tickets
{
    public interface ITicketAppService
    {
        Task<EditOrderTicketOutput> EditOrderTicket(OrderTicketEditDto model);
        Task<TicketEditDto> GetTicketEditDto(NullableIdDto input);
        Task<TicketEditDto> EditTicket(TicketEditDto model);
        Task<TicketPhotoDto> GetTicketPhoto(int ticketId);
        Task<PagedResultDto<TicketListItemViewModel>> LoadTicketsByOrderLineId(int orderLineId);
        Task<string> CheckIfTruckIsOutOfServiceOrInactive(TicketEditDto model);

        Task<DeleteTicketOutput> DeleteTicket(EntityDto input);
        Task MarkAsBilledTicket(EntityDto input);
        Task<FileDto> GetTicketPhotosForInvoice(int invoiceId);
        Task<string> GenerateTicketNumber(int ticketId);
        Task<GetDriverAndTrailerForTicketTruckResult> GetDriverAndTrailerForTicketTruck(GetDriverAndTrailerForTicketTruckInput input);
        Task<GetTruckAndTrailerForTicketDriverResult> GetTruckAndTrailerForTicketDriver(GetTruckAndTrailerForTicketDriverInput input);
        Task<FileBytesDto> GetTicketPrintOut(GetTicketPrintOutInput input);

        Task<PagedResultDto<TicketListViewDto>> TicketListView(TicketListInput input);
        Task GenerateTicketImagesPdf(GenerateTicketImagesInput input);
        Task GenerateTicketImagesZip(GenerateTicketImagesInput input);

    }
}
