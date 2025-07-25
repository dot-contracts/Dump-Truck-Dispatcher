using System.Collections.Generic;
using System.Threading.Tasks;
using DispatcherWeb.Dto;
using DispatcherWeb.Tickets.Dto;

namespace DispatcherWeb.Tickets.Exporting
{
    public interface ITicketListCsvExporter
    {
        Task<FileDto> ExportToFileAsync(List<TicketListViewDto> ticketDtos, string fileName, bool hideColumnsForInvoiceExport);
    }
}
