using System.Collections.Generic;
using Abp.Application.Services.Dto;

namespace DispatcherWeb.MultiTenancy.HostDashboard.Dto
{
    public class GetTenantStatisticsResult : PagedResultDto<TenantStatisticsDto>
    {
        public TenantStatisticsDto Total { get; set; }

        public GetTenantStatisticsResult()
        {
        }

        public GetTenantStatisticsResult(int totalCount, IReadOnlyList<TenantStatisticsDto> items) : base(totalCount, items)
        {
        }
    }
}
