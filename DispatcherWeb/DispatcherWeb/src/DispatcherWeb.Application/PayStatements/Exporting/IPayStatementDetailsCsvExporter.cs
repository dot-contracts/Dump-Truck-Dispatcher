using System.Collections.Generic;
using System.Threading.Tasks;
using DispatcherWeb.Dto;
using DispatcherWeb.PayStatements.Dto;

namespace DispatcherWeb.PayStatements.Exporting
{
    public interface IPayStatementDetailsCsvExporter
    {
        Task<FileDto> ExportToFileAsync(IList<PayStatementItemEditDto> rows, PayStatementEditDto details);
    }
}
