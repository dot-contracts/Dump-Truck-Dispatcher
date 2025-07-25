using System.Collections.Generic;
using System.Threading.Tasks;
using DispatcherWeb.Customers.Dto;
using DispatcherWeb.Dto;

namespace DispatcherWeb.Customers.Exporting
{
    public interface ICustomerListCsvExporter
    {
        Task<FileDto> ExportToFileAsync(List<CustomerDto> customerDtos);
    }
}
