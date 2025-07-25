using Abp.Authorization;
using Abp.Domain.Repositories;
using DispatcherWeb.ActiveReports.ActiveReports.Dto;
using DispatcherWeb.Authorization;
using Microsoft.EntityFrameworkCore;

namespace DispatcherWeb.ActiveReports.ActiveReports
{
    [AbpAuthorize(AppPermissions.Pages_ActiveReports)]
    public class ActiveReportsAppService : DispatcherWebAppServiceBase, IActiveReportsAppService
    {
        private readonly IRepository<ActiveReport> _activeReportsRepository;

        public ActiveReportsAppService(
            IRepository<ActiveReport> activeReportsRepository)
        {
            _activeReportsRepository = activeReportsRepository;
        }

        public async Task<List<ActiveReportListItemDto>> GetActiveReportsList()
        {
            var reports = await (await _activeReportsRepository.GetQueryAsync())
                                .Distinct()
                                .Select(p => new ActiveReportListItemDto
                                {
                                    Name = p.Name,
                                    Description = p.Description,
                                    Path = p.Path,
                                })
                                .OrderBy(p => p.Name)
                                .ToListAsync();

            return reports;
        }
    }
}
