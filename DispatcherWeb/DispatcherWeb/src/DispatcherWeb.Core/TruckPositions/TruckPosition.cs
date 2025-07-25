using System;
using System.ComponentModel.DataAnnotations;
using Abp.Timing;
using Azure;
using Azure.Data.Tables;
using DispatcherWeb.Infrastructure;

namespace DispatcherWeb.TruckPositions
{
    public class TruckPosition : ITableEntity
    {
        public TruckPosition()
        {
            CreationTime = Clock.Now;
        }

        public string PartitionKey { get => PartitionKeyUnparsableValue ?? (TenantId + "-" + TruckId); set => SetPartitionKey(value); }

        public string PartitionKeyUnparsableValue { get; set; }

        private void SetPartitionKey(string value)
        {
            PartitionKeyUnparsableValue = value;

            if (value?.Contains('-') != true)
            {
                return;
            }

            var valueParts = value.Split('-');
            if (valueParts.Length != 2
                || !int.TryParse(valueParts[0], out var tenantId)
                || !int.TryParse(valueParts[1], out var truckId))
            {
                return;
            }

            TenantId = tenantId;
            TruckId = truckId;
            PartitionKeyUnparsableValue = null;
        }

        public string RowKey { get => RowKeyUnparsableValue ?? GpsTimestamp.ToString("u"); set => SetRowKey(value); }

        public string RowKeyUnparsableValue { get; set; }

        private void SetRowKey(string value)
        {
            RowKeyUnparsableValue = value;
            if (string.IsNullOrEmpty(value)
                || !DateTime.TryParse(value, out var dateTime))
            {
                return;
            }

            GpsTimestamp = dateTime;
            RowKeyUnparsableValue = null;
        }

        /// <summary>
        /// This is last modification datetime of the entity in Azure Tables, please use GpsTimestamp to get the timestamp of the original event
        /// </summary>
        public DateTimeOffset? Timestamp { get; set; }

        public ETag ETag { get; set; }

        public int TenantId { get; set; }

        public int TruckId { get; set; }

        public string DtdTrackerUniqueId { get; set; }

        public int? DriverId { get; set; }

        public double? Latitude { get; set; }

        public double? Longitude { get; set; }

        public double? Accuracy { get; set; }

        public double? Speed { get; set; }

        public double? Heading { get; set; }

        public double? Altitude { get; set; }

        public TruckPositionActivityType ActivityType { get; set; }

        [StringLength(EntityStringFieldLengths.TruckPosition.ActivityTypeRaw)]
        public string ActivitiTypeRaw { get; set; }

        public int? ActivityConfidence { get; set; }

        [StringLength(EntityStringFieldLengths.TruckPosition.GeofenceIdentifier)]
        public string GeofenceIdentifier { get; set; }

        public TruckPositionGeofenceAction? GeofenceAction { get; set; }

        [StringLength(EntityStringFieldLengths.TruckPosition.GeofenceActionRaw)]
        public string GeofenceActionRaw { get; set; }

        public double? BatteryLevel { get; set; }

        public bool? BatteryIsCharging { get; set; }

        public DateTime GpsTimestamp { get; set; }

        public Guid Uuid { get; set; }

        public TruckPositionEvent Event { get; set; }

        [StringLength(EntityStringFieldLengths.General.Medium)]
        public string EventRaw { get; set; }

        public bool IsMoving { get; set; }

        public double? Odometer { get; set; }

        public DateTime CreationTime { get; set; }

        public long? CreatorUserId { get; set; }
    }
}
