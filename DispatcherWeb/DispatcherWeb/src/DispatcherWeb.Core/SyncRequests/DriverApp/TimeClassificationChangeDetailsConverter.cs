using System;
using DispatcherWeb.SyncRequests.Entities;
using DispatcherWeb.SyncRequests.FcmPushMessages.ChangeDetails;

namespace DispatcherWeb.SyncRequests.DriverApp
{
    public class TimeClassificationChangeDetailsConverter : GenericChangeDetailsConverter
    {
        public override EntityEnum GetEntityTypeForPushMessage(EntityEnum sourceEntityType)
        {
            switch (sourceEntityType)
            {
                case EntityEnum.EmployeeTimeClassification: return EntityEnum.TimeClassification;
                default: return sourceEntityType;
            }
        }

        public override FcmEntityChangeDetailsDto GetChangeDetails(IChangedDriverAppEntity changedEntity, ChangeType changeType)
        {
            if (changedEntity is ChangedEmployeeTimeClassification changedEmployeeTimeClassification)
            {
                return new FcmEntityChangeDetailsDto<int>
                {
                    Id = changedEmployeeTimeClassification.TimeClassificationId,
                    ChangeType = changeType,
                };
            }
            throw new ApplicationException("Only ChangedEmployeeTimeClassification entity is supported");
        }
    }
}
