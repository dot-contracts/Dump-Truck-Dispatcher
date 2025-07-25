using System.Collections.Generic;
using System.Threading.Tasks;
using DispatcherWeb.Dto;
using DispatcherWeb.LeaseHaulers.Dto;

namespace DispatcherWeb.LeaseHaulers.Exporting
{
    public interface ILeaseHaulerListCsvExporter
    {
        Task<FileDto> ExportToFileAsync(List<LeaseHaulerEditDto> leaseHaulerEditDtos);
    }
}
