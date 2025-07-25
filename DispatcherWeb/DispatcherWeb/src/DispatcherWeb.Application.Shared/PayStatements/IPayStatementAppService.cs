using System.Threading.Tasks;
using Abp.Application.Services;
using Abp.Application.Services.Dto;
using DispatcherWeb.Dto;
using DispatcherWeb.PayStatements.Dto;

namespace DispatcherWeb.PayStatements
{
    public interface IPayStatementAppService : IApplicationService
    {
        Task<FileBytesDto> GetDriverPayStatementReport(GetDriverPayStatementReportInput input);
        Task<FileBytesDto> GetDriverPayStatementWarningsReport(EntityDto input);
        Task<PayStatementEditDto> GetPayStatementForEdit(EntityDto entityDto);
    }
}
