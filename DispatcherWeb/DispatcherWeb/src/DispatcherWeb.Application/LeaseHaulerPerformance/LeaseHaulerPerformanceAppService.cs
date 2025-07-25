using System;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Threading.Tasks;
using Abp.Application.Services.Dto;
using Abp.Authorization;
using Abp.Domain.Repositories;
using Abp.Linq.Extensions;
using DispatcherWeb.Authorization;
using DispatcherWeb.Dispatching;
using DispatcherWeb.LeaseHaulerPerformance.Dto;
using Microsoft.EntityFrameworkCore;

namespace DispatcherWeb.LeaseHaulerPerformance
{
    [AbpAuthorize(AppPermissions.Pages_LeaseHaulerPerformance)]
    public class LeaseHaulerPerformanceAppService : DispatcherWebAppServiceBase, ILeaseHaulerPerformanceAppService
    {
        private readonly IRepository<Dispatch> _dispatchRepository;

        public LeaseHaulerPerformanceAppService(
            IRepository<Dispatch> dispatchRepository
        )
        {
            _dispatchRepository = dispatchRepository;
        }

        public async Task<PagedResultDto<LeaseHaulerPerformanceDto>> GetLeaseHaulerPerformances(GetLeaseHaulerPerformancesInput input)
        {
            input.StartDate = input.StartDate?.Date;
            input.EndDate = input.EndDate?.Date.AddDays(1);

            var query = (await _dispatchRepository.GetQueryAsync())
                .Where(d => d.Truck.LeaseHaulerTruck.LeaseHauler != null)
                .Where(d => d.Status == DispatchStatus.Completed || d.Status == DispatchStatus.Canceled)
                .WhereIf(input.StartDate.HasValue, d => d.OrderLine.Order.DeliveryDate >= input.StartDate)
                .WhereIf(input.EndDate.HasValue, d => d.OrderLine.Order.DeliveryDate < input.EndDate)
                .Select(d => new
                {
                    LeaseHaulerId = d.Truck.LeaseHaulerTruck.LeaseHaulerId,
                    LeaseHaulerName = d.Truck.LeaseHaulerTruck.LeaseHauler.Name,
                    Status = d.Status,
                })
                .GroupBy(x => new { x.LeaseHaulerId, x.LeaseHaulerName })
                .Select(g => new
                {
                    g.Key.LeaseHaulerId,
                    g.Key.LeaseHaulerName,
                    Completed = g.Count(d => d.Status == DispatchStatus.Completed),
                    Canceled = g.Count(d => d.Status == DispatchStatus.Canceled),
                    Total = g.Count(),
                })
                .Select(x => new LeaseHaulerPerformanceDto
                {
                    LeaseHaulerId = x.LeaseHaulerId,
                    LeaseHaulerName = x.LeaseHaulerName,
                    Completed = x.Completed,
                    Canceled = x.Canceled,
                    Total = x.Total,
                    PercentComplete = Math.Round(x.Total > 0 ? (decimal)x.Completed * 100 / x.Total : 0, 2),
                });

            var items = await query
                .OrderBy(input.Sorting)
                .ToListAsync();

            return new PagedResultDto<LeaseHaulerPerformanceDto>(
                items.Count,
                items);
        }
    }
}
