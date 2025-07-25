using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Abp.Domain.Repositories;
using Abp.Extensions;
using DispatcherWeb.Dispatching;
using DispatcherWeb.SyncRequests.Entities;
using DispatcherWeb.SyncRequests.FcmPushMessages.ChangeDetails;
using Microsoft.EntityFrameworkCore;

namespace DispatcherWeb.SyncRequests.DriverApp
{
    public class DispatchChangeDetailsConverter : GenericChangeDetailsConverter
    {
        private readonly IRepository<Dispatch> _dispatchRepository;
        private List<DispatchDetailsDto> _dispatchDetails;

        public DispatchChangeDetailsConverter(IRepository<Dispatch> dispatchRepository)
        {
            _dispatchRepository = dispatchRepository;
        }

        public override async Task CacheDataIfNeeded(IEnumerable<SyncRequestChangeDetailAbstract> changes)
        {
            var dispatchChanges = changes
                .OfType<SyncRequestChangeDetail<ChangedDispatch>>()
                .ToList();

            var dispatchIds = dispatchChanges.Select(x => x.Entity.Id).Distinct().ToList();

            _dispatchDetails = await (await _dispatchRepository.GetQueryAsync())
                .Where(x => dispatchIds.Contains(x.Id))
                .Select(x => new DispatchDetailsDto
                {
                    Id = x.Id,
                    DeliveryDate = x.OrderLine.Order.DeliveryDate,
                    Status = x.Status,
                }).ToListAsync();
        }

        public override FcmEntityChangeDetailsDto GetChangeDetails(IChangedDriverAppEntity changedEntity, ChangeType changeType)
        {
            if (_dispatchDetails == null)
            {
                throw new ApplicationException("Cache wasn't populated before calling GetChangedDetails");
            }

            if (changedEntity is ChangedDispatch changedDispatch)
            {
                var dispatchDetails = _dispatchDetails.FirstOrDefault(x => x.Id == changedDispatch.Id);

                return new FcmDispatchDetailsDto
                {
                    Id = changedDispatch.Id,
                    ChangeType = dispatchDetails?.Status.IsIn(DispatchStatus.Canceled, DispatchStatus.Error) == false ? ChangeType.Modified : ChangeType.Removed, //'Removed' for true and null
                    DeliveryDate = dispatchDetails?.DeliveryDate.ToString("yyyy-MM-dd"),
                };
            }
            throw new NotImplementedException("Only ChangedDispatch entities are expected");
        }

        private class DispatchDetailsDto
        {
            public int Id { get; internal set; }
            public DateTime DeliveryDate { get; internal set; }
            public DispatchStatus Status { get; internal set; }
        }
    }
}
