using System.Threading.Tasks;
using Abp.Application.Services.Dto;
using DispatcherWeb.ScheduledReports.Dto;

namespace DispatcherWeb.ScheduledReports
{
    public interface IScheduledReportAppService
    {
        Task<PagedResultDto<ScheduledReportDto>> GetScheduledReportPagedList(GetScheduledReportPagedListInput input);
        Task<ScheduledReportEditDto> GetScheduledReportForEdit(NullableIdDto input);
    }
}