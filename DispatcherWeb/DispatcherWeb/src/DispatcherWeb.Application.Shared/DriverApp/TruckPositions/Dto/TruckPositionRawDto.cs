using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using DispatcherWeb.Infrastructure;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace DispatcherWeb.DriverApp.TruckPositions.Dto
{
    public class TruckPositionRawDto
    {
        public List<LocationDto> Location { get; set; }

        public int? TruckId { get; set; }

        [JsonExtensionData]
        public IDictionary<string, JToken> AdditionalData { get; set; }
        public string Json => JsonConvert.SerializeObject(AdditionalData);

        public class LocationDto
        {
            [JsonProperty("coords")]
            public CoordinatesDto Coordinates { get; set; }

            public ExtrasDto Extras { get; set; }

            public ActivityDto Activity { get; set; }

            public GeofenceDto Geofence { get; set; }

            public BatteryDto Battery { get; set; }

            public DateTime TimeStamp { get; set; }

            public Guid Uuid { get; set; }

            [JsonIgnore]
            public TruckPositionEvent Event { get; private set; }

            private string _eventRaw;

            [JsonProperty(PropertyName = "event")]
            public string EventRaw
            {
                get
                {
                    return _eventRaw;
                }
                set
                {
                    _eventRaw = value;
                    switch (value)
                    {
                        case "motionchange": Event = TruckPositionEvent.MotionChange; break;
                        case "geofence": Event = TruckPositionEvent.Geofence; break;
                        case "heartbeat": Event = TruckPositionEvent.Heartbeat; break;
                        case "providerchange": Event = TruckPositionEvent.ProviderChange; break;
                        default: Event = 0; break;
                    }
                }
            }

            public bool IsMoving { get; set; }

            public double? Odometer { get; set; }

            public class CoordinatesDto
            {
                public double? Latitude { get; set; }
                public double? Longitude { get; set; }
                public double? Accuracy { get; set; }
                public double? Speed { get; set; }
                public double? Heading { get; set; }
                public double? Altitude { get; set; }
            }

            public class ExtrasDto
            {
                [JsonExtensionData]
                public IDictionary<string, JToken> AdditionalData { get; set; }
                public string Json => JsonConvert.SerializeObject(AdditionalData);
            }

            public class ActivityDto
            {
                public int Confidence { get; set; }

                [JsonIgnore]
                public TruckPositionActivityType Type { get; private set; }

                private string _typeRaw;

                [JsonProperty(PropertyName = "type")]
                public string TypeRaw
                {
                    get
                    {
                        return _typeRaw;
                    }
                    set
                    {
                        _typeRaw = value;
                        Type = _types.ContainsKey(value) ? _types[value] : 0;
                    }
                }

                private static readonly Dictionary<string, TruckPositionActivityType> _types = new Dictionary<string, TruckPositionActivityType>
                {
                    { "still", TruckPositionActivityType.Still },
                    { "on_foot", TruckPositionActivityType.OnFoot },
                    { "walking", TruckPositionActivityType.Walking },
                    { "running", TruckPositionActivityType.Running },
                    { "in_vehicle", TruckPositionActivityType.InVehicle },
                    { "on_bicycle", TruckPositionActivityType.OnBicycle },
                    { "unknown", TruckPositionActivityType.Unknown },
                };
            }

            public class GeofenceDto
            {
                [StringLength(EntityStringFieldLengths.TruckPosition.GeofenceIdentifier)]
                public string Identifier { get; set; }

                [JsonIgnore]
                public TruckPositionGeofenceAction Action { get; private set; }

                private string _actionRaw;

                [JsonProperty(PropertyName = "action")]
                public string ActionRaw
                {
                    get
                    {
                        return _actionRaw;
                    }
                    set
                    {
                        _actionRaw = value;
                        switch (value)
                        {
                            case "ENTER": Action = TruckPositionGeofenceAction.Enter; break;
                            case "EXIT": Action = TruckPositionGeofenceAction.Exit; break;
                            default: Action = 0; break;
                        }
                    }
                }
            }

            public class BatteryDto
            {
                public double? Level { get; set; }

                [JsonProperty(PropertyName = "is_charging")]
                public bool? IsCharging { get; set; }
            }
        }
    }
}
