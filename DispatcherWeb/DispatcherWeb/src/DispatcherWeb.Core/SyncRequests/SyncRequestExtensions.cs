using DispatcherWeb.Chat;
using DispatcherWeb.Dispatching;
using DispatcherWeb.Drivers;
using DispatcherWeb.SyncRequests.Entities;
using DispatcherWeb.TimeClassifications;

namespace DispatcherWeb.SyncRequests
{
    public static class SyncRequestExtensions
    {


        public static ChangedDispatch ToChangedEntity(this Dispatch entity)
        {
            return new ChangedDispatch
            {
                Id = entity.Id,
                DriverId = entity.DriverId,
                Status = entity.Status,
                IsMultipleLoads = entity.IsMultipleLoads,
                OrderLineId = entity.OrderLineId,
                OrderLineTruckId = entity.OrderLineTruckId,
                EntityReference = entity,
            }.SetLastUpdateDateTime(entity);
        }

        public static ChangedEmployeeTime ToChangedEntity(this EmployeeTime entity)
        {
            return new ChangedEmployeeTime
            {
                Id = entity.Id,
                DriverId = entity.DriverId,
                Guid = entity.Guid,
                TruckId = entity.EquipmentId,
                UserId = entity.UserId,
                EntityReference = entity,
            }.SetLastUpdateDateTime(entity);
        }

        public static ChangedDriverAssignment ToChangedEntity(this DriverAssignment entity)
        {
            return new ChangedDriverAssignment
            {
                Id = entity.Id,
                DriverId = entity.DriverId,
                TruckId = entity.TruckId,
                EntityReference = entity,
            }.SetLastUpdateDateTime(entity);
        }

        public static ChangedEmployeeTimeClassification ToChangedEntity(this EmployeeTimeClassification entity)
        {
            return new ChangedEmployeeTimeClassification
            {
                Id = entity.Id,
                DriverId = entity.DriverId,
                TimeClassificationId = entity.TimeClassificationId,
                EntityReference = entity,
            }.SetLastUpdateDateTime(entity);
        }

        public static ChangedTimeClassification ToChangedEntity(this TimeClassification entity)
        {
            return new ChangedTimeClassification
            {
                Id = entity.Id,
                EntityReference = entity,
            }.SetLastUpdateDateTime(entity);
        }

        public static ChangedChatMessage ToChangedEntity(this ChatMessage entity)
        {
            return new ChangedChatMessage
            {
                Id = entity.Id,
                UserId = entity.UserId,
                TargetUserId = entity.TargetUserId,
                LastUpdateDateTime = entity.CreationTime,
                EntityReference = entity,
            };
        }
    }
}
