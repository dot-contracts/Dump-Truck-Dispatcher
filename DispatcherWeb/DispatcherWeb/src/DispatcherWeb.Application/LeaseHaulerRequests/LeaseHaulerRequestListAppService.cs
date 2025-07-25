using System.Linq;
using System.Linq.Dynamic.Core;
using System.Threading.Tasks;
using Abp.Application.Services.Dto;
using Abp.Authorization;
using Abp.Domain.Repositories;
using Abp.Linq.Extensions;
using DispatcherWeb.Authorization;
using DispatcherWeb.Infrastructure.Extensions;
using DispatcherWeb.LeaseHaulerRequests.Dto;
using DispatcherWeb.Orders;
using Microsoft.EntityFrameworkCore;

namespace DispatcherWeb.LeaseHaulerRequests
{
    [AbpAuthorize]
    public class LeaseHaulerRequestListAppService : DispatcherWebAppServiceBase
    {
        private readonly IRepository<LeaseHaulerRequest> _leaseHaulerRequestRepository;
        private readonly IRepository<OrderLineTruck> _orderLineTruckRepository;
        private readonly IRepository<AvailableLeaseHaulerTruck> _availableLeaseHaulerTruckRepository;

        public LeaseHaulerRequestListAppService(
            IRepository<LeaseHaulerRequest> leaseHaulerRequestRepository,
            IRepository<OrderLineTruck> orderLineTruckRepository,
            IRepository<AvailableLeaseHaulerTruck> availableLeaseHaulerTruckRepository
        )
        {
            _leaseHaulerRequestRepository = leaseHaulerRequestRepository;
            _orderLineTruckRepository = orderLineTruckRepository;
            _availableLeaseHaulerTruckRepository = availableLeaseHaulerTruckRepository;
        }

        [AbpAuthorize(AppPermissions.Pages_LeaseHaulerRequests, AppPermissions.LeaseHaulerPortal_Truck_Request)]
        public async Task<PagedResultDto<LeaseHaulerRequestDto>> GetLeaseHaulerRequestPagedList(GetLeaseHaulerRequestPagedListInput input)
        {
            var query = await GetFilteredLeaseHaulerRequestQueryAsync(input);

            var totalCount = await query.CountAsync();

            var items = await (await GetLeaseHaulerRequestDtoQuery(query))
                .OrderBy(input.Sorting)
                .PageBy(input)
                .ToListAsync();

            return new PagedResultDto<LeaseHaulerRequestDto>(totalCount, items);
        }

        private async Task<IQueryable<LeaseHaulerRequest>> GetFilteredLeaseHaulerRequestQueryAsync(IGetLeaseHaulerRequestPagedListInput input)
        {
            var leaseHaulerIdFilter = await GetLeaseHaulerIdFilterAsync(AppPermissions.Pages_LeaseHaulerRequests, AppPermissions.LeaseHaulerPortal_Truck_Request);
            var officeIds = await GetOfficeIds();

            return (await _leaseHaulerRequestRepository.GetQueryAsync())
                .Where(lhr => lhr.Date >= input.DateBegin && lhr.Date < input.DateEnd.AddDays(1))
                .WhereIf(leaseHaulerIdFilter.HasValue, lhr => lhr.LeaseHaulerId == leaseHaulerIdFilter)
                .WhereIf(input.OfficeId.HasValue, lhr => lhr.OfficeId == input.OfficeId)
                .WhereIf(!input.OfficeId.HasValue, lhr => officeIds.Contains(lhr.OfficeId))
                .WhereIf(input.Shift.HasValue && input.Shift != Shift.NoShift, lhr => lhr.Shift == input.Shift)
                .WhereIf(input.Shift.HasValue && input.Shift == Shift.NoShift, da => da.Shift == null)
                .WhereIf(input.LeaseHaulerId.HasValue, lhr => lhr.LeaseHaulerId == input.LeaseHaulerId);
        }

        private async Task<IQueryable<LeaseHaulerRequestDto>> GetLeaseHaulerRequestDtoQuery(IQueryable<LeaseHaulerRequest> query)
        {
            var shiftDictionary = await SettingManager.GetShiftDictionary();
            var oltQuery = await _orderLineTruckRepository.GetQueryAsync();
            var availableLeaseHaulerTruckQuery = await _availableLeaseHaulerTruckRepository.GetQueryAsync();
            return query.Select(lhr => new LeaseHaulerRequestDto
            {
                Id = lhr.Id,
                Date = lhr.Date,
                Shift = lhr.Shift.HasValue ? shiftDictionary[lhr.Shift.Value] : "",
                LeaseHauler = lhr.LeaseHauler.Name,
                Sent = lhr.Sent,
                Message = lhr.Message,
                Comments = lhr.Comments,
                NumberTrucksRequested = lhr.NumberTrucksRequested,
                Available = lhr.Available,
                Approved = lhr.Approved,
                Scheduled = oltQuery
                    .Where(olt =>
                        olt.OrderLine.Order.OfficeId == lhr.OfficeId
                        && olt.OrderLine.Order.Shift == lhr.Shift
                        && olt.OrderLine.Order.DeliveryDate == lhr.Date
                        && olt.Truck.LeaseHaulerTruck.LeaseHaulerId == lhr.LeaseHaulerId
                    )
                    .Select(olt => olt.TruckId)
                    .Distinct()
                    .Count(),
                SpecifiedTrucks = availableLeaseHaulerTruckQuery
                    .Where(aLhTruck =>
                        aLhTruck.OfficeId == lhr.OfficeId
                        && aLhTruck.Shift == lhr.Shift
                        && aLhTruck.Date == lhr.Date
                        && aLhTruck.LeaseHaulerId == lhr.LeaseHaulerId
                    )
                    .Select(aLhTruck => aLhTruck.TruckId)
                    .Distinct()
                    .Count(),
            });
        }
    }
}
