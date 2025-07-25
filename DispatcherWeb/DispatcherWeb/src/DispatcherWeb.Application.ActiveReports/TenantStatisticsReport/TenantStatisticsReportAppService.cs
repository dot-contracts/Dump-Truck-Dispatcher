using Abp.Authorization;
using Abp.Domain.Repositories;
using Abp.Linq.Extensions;
using DispatcherWeb.ActiveReports.TenantStatisticsReport.Dto;
using DispatcherWeb.Authorization;
using DispatcherWeb.DailyHistory;
using Microsoft.EntityFrameworkCore;

namespace DispatcherWeb.ActiveReports.TenantStatisticsReport
{
    [AbpAuthorize(AppPermissions.Pages_ActiveReports_TenantStatisticsReport)]
    public class TenantStatisticsReportAppService : DispatcherWebActiveReportsAppServiceBase, ITenantStatisticsReportAppService
    {
        private readonly IRepository<TenantDailyHistory> _tenantDailyHistoryRepository;

        public TenantStatisticsReportAppService(
            IRepository<TenantDailyHistory> tenantDailyHistoryRepository
            )
        {
            _tenantDailyHistoryRepository = tenantDailyHistoryRepository;
        }

        public async Task<List<TenantDto>> GetTenants()
        {
            var tenants = await (await TenantManager.GetQueryAsync())
                            .Where(t => t.IsActive == true && !t.IsDeleted)
                            .Select(t => new TenantDto
                            {
                                TenantId = t.Id,
                                TenantName = t.Name,
                            })
                            .ToListAsync();
            return tenants;
        }

        public async Task<List<TenantDailyHistorySummaryDto>> GetTenantStatistics(GetTenantStatisticsInput input)
        {
            var summary = new List<TenantDailyHistorySummaryDto> {
                new TenantDailyHistorySummaryDto
                {
                    TenantId = input.TenantId ?? 0,
                },
            };

            var tenants = await (await TenantManager.GetQueryAsync())
                        .Where(t => t.CreationTime >= input.StartDate
                                    && t.CreationTime <= input.EndDate
                                    && t.IsActive == true && !t.IsDeleted)
                        .ToListAsync();

            var tenantDailyHistoryData = await (await _tenantDailyHistoryRepository.GetQueryAsync())
                .WhereIf(input.TenantId.HasValue, tdh => tdh.TenantId == input.TenantId.Value)
                .Where(tdh => tdh.Date >= input.StartDate && tdh.Date <= input.EndDate)
                .Select(s => new
                {
                    s.TenantId,
                    TenantName = s.Tenant.Name,
                    s.Date.Year,
                    MonthName = s.Date.ToString("MMM"),
                    MonthNumber = s.Date.Month,
                    s.ActiveUsers,
                    s.InternalTrucksScheduled,
                    s.LeaseHaulerScheduledDeliveries,
                    s.OrderLinesCreated,
                    s.TicketsCreated,
                    s.SmsSent,
                })
                .ToListAsync();

            summary = tenantDailyHistoryData
                            .GroupBy(p => new { p.TenantId, p.TenantName, p.Year, p.MonthName, p.MonthNumber })
                            .Select(g => new TenantDailyHistorySummaryDto
                            {
                                TenantId = g.Key.TenantId,
                                TenantName = g.Key.TenantName,
                                MonthYear = g.Key.Year,
                                MonthName = g.Key.MonthName,
                                MonthNumber = g.Key.MonthNumber,
                                ActiveUsers = g.Sum(a => a.ActiveUsers),
                                TrucksScheduled = g.Sum(a => a.InternalTrucksScheduled) + g.Sum(a => a.LeaseHaulerScheduledDeliveries),
                                OrderLines = g.Sum(a => a.OrderLinesCreated),
                                Tickets = g.Sum(a => a.TicketsCreated),
                                Sms = g.Sum(a => a.SmsSent),
                                TenantsAdded = tenants.Count(p => p.CreationTime.Month == g.Key.MonthNumber && p.CreationTime.Year == g.Key.Year),
                            })
                            .OrderBy(p => p.MonthNumber)
                            .ThenBy(p => p.MonthYear)
                            .ToList();

            return summary;
        }
    }
}
