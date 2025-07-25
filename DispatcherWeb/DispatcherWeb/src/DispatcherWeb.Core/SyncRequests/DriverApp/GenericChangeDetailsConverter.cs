using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using DispatcherWeb.SyncRequests.Entities;
using DispatcherWeb.SyncRequests.FcmPushMessages.ChangeDetails;

namespace DispatcherWeb.SyncRequests.DriverApp
{
    public class GenericChangeDetailsConverter
    {
        public virtual Task CacheDataIfNeeded(IEnumerable<SyncRequestChangeDetailAbstract> changes)
        {
            return Task.CompletedTask;
        }

        public virtual EntityEnum GetEntityTypeForPushMessage(EntityEnum sourceEntityType)
        {
            return sourceEntityType;
        }

        public virtual FcmEntityChangeDetailsDto GetChangeDetails(IChangedDriverAppEntity changedEntity, ChangeType changeType)
        {
            if (changedEntity is ChangedDriverAppEntity<int> changedIntKeyedEntity)
            {
                return new FcmEntityChangeDetailsDto<int>
                {
                    Id = changedIntKeyedEntity.Id,
                    ChangeType = changeType,
                };
            }
            if (changedEntity is ChangedDriverAppEntity<long> changedLongKeyedEntity)
            {
                return new FcmEntityChangeDetailsDto<long>
                {
                    Id = changedLongKeyedEntity.Id,
                    ChangeType = changeType,
                };
            }
            if (changedEntity is ChangedDriverAppEntity<Guid> changedGuidKeyedEntity)
            {
                return new FcmEntityChangeDetailsDto<Guid>
                {
                    Id = changedGuidKeyedEntity.Id,
                    ChangeType = changeType,
                };
            }
            throw new NotImplementedException("The ChangedDriverAppEntity key is not supported yet, please add the appropriate type");
        }
    }
}
