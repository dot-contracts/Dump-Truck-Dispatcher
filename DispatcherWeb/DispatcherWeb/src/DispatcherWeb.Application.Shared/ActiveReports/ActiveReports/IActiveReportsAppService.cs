using System.Collections.Generic;
using System.Threading.Tasks;
using Abp.Application.Services;
using DispatcherWeb.ActiveReports.ActiveReports.Dto;

namespace DispatcherWeb.ActiveReports.ActiveReports
{
    public interface IActiveReportsAppService : IApplicationService
    {
        Task<List<ActiveReportListItemDto>> GetActiveReportsList();
    }
}
