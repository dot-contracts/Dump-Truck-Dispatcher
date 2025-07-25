using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Abp.Authorization;
using Abp.Domain.Repositories;
using Abp.Timing;
using DispatcherWeb.Authorization;
using DispatcherWeb.Dispatching;
using DispatcherWeb.Orders;
using DispatcherWeb.SyncRequests;
using DispatcherWeb.TrailerAssignments.Dto;
using DispatcherWeb.Trucks;
using Microsoft.EntityFrameworkCore;

namespace DispatcherWeb.TrailerAssignments
{
    [AbpAuthorize]
    public class TrailerAssignmentAppService : DispatcherWebAppServiceBase, ITrailerAssignmentAppService
    {
        private readonly IRepository<Truck> _truckRepository;
        private readonly IRepository<OrderLineTruck> _orderLineTruckRepository;
        private readonly IRepository<Dispatch> _dispatchRepository;
        private readonly ISyncRequestSender _syncRequestSender;

        public TrailerAssignmentAppService(
            IRepository<Truck> truckRepository,
            IRepository<OrderLineTruck> orderLineTruckRepository,
            IRepository<Dispatch> dispatchRepository,
            ISyncRequestSender syncRequestSender
            )
        {
            _truckRepository = truckRepository;
            _orderLineTruckRepository = orderLineTruckRepository;
            _dispatchRepository = dispatchRepository;
            _syncRequestSender = syncRequestSender;
        }

        [AbpAuthorize(AppPermissions.Pages_Schedule)]
        public async Task SetTrailerForTractor(SetTrailerForTractorInput input)
        {
            var tractor = await (await _truckRepository.GetQueryAsync())
                .Where(x => x.Id == input.TractorId)
                .FirstAsync();

            var oldTrailerId = tractor.CurrentTrailerId;
            tractor.CurrentTrailerId = input.TrailerId;

            if (input.TrailerId.HasValue)
            {
                var otherTractors = await (await _truckRepository.GetQueryAsync())
                    .Where(x => x.CurrentTrailerId == input.TrailerId.Value && x.Id != input.TractorId)
                    .ToListAsync();

                otherTractors.ForEach(x => x.CurrentTrailerId = null);
            }

            if (input.UpdateExistingOrderLineTrucks)
            {
                var orderLineTrucks = await (await _orderLineTruckRepository.GetQueryAsync())
                    .Where(x => input.Date == x.OrderLine.Order.DeliveryDate && !x.IsDone)
                    .Where(x => x.TrailerId == oldTrailerId)
                    .Where(x => input.TractorId == x.TruckId)
                    .ToListAsync();

                foreach (var orderLineTruck in orderLineTrucks)
                {
                    orderLineTruck.TrailerId = input.TrailerId;
                }

                var orderLineTruckIds = orderLineTrucks.Select(x => x.Id).ToList();

                var affectedDispatches = await (await _dispatchRepository.GetQueryAsync())
                    .Where(x => x.OrderLineTruckId.HasValue && orderLineTruckIds.Contains(x.OrderLineTruckId.Value))
                    .ToListAsync();

                await SendSyncRequestForAffectedDispatches(affectedDispatches, "Updated Trailer for dispatch(es)");
            }
        }

        [AbpAuthorize(AppPermissions.Pages_Schedule)]
        public async Task SetTractorForTrailer(SetTractorForTrailerInput input)
        {
            var otherTractors = await (await _truckRepository.GetQueryAsync())
                .Where(x => x.CurrentTrailerId == input.TrailerId && x.Id != input.TractorId)
                .ToListAsync();

            otherTractors.ForEach(x => x.CurrentTrailerId = null);

            if (input.TractorId.HasValue)
            {
                var tractor = await (await _truckRepository.GetQueryAsync())
                    .Where(x => x.Id == input.TractorId.Value)
                    .FirstAsync();

                tractor.CurrentTrailerId = input.TrailerId;
            }
        }

        [AbpAuthorize(AppPermissions.Pages_Schedule)]
        public async Task SetTrailerForOrderLineTruck(SetTrailerForOrderLineTruckInput input)
        {
            var orderLineTruck = await (await _orderLineTruckRepository.GetQueryAsync()).FirstAsync(x => x.Id == input.OrderLineTruckId);
            if (orderLineTruck.TrailerId == input.TrailerId)
            {
                return;
            }

            orderLineTruck.TrailerId = input.TrailerId;

            var affectedDispatches = await (await _dispatchRepository.GetQueryAsync())
                .Where(x => x.OrderLineTruckId == input.OrderLineTruckId)
                .ToListAsync();

            await SendSyncRequestForAffectedDispatches(affectedDispatches, "Updated Trailer for dispatch(es)");
        }

        private async Task SendSyncRequestForAffectedDispatches(List<Dispatch> affectedDispatches, string logMessage)
        {
            if (affectedDispatches.Any())
            {
                affectedDispatches.ForEach(d => d.LastModificationTime = Clock.Now);
                await CurrentUnitOfWork.SaveChangesAsync();

                await _syncRequestSender.SendSyncRequest(new SyncRequest()
                    .AddChanges(EntityEnum.Dispatch, affectedDispatches.Select(x => x.ToChangedEntity()))
                    .AddLogMessage(logMessage));
            }
        }
    }
}
