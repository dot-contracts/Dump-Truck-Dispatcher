using System;
using Abp.Extensions;

namespace DispatcherWeb.Infrastructure.Telematics.Dto.DtdTracker
{
    public class GpsMessageDto
    {
        public DateTime GpsTimestamp { get; set; }
        public double Latitude { get; set; }
        public double Longitude { get; set; }
        public double? AltitudeInMeters { get; set; }
        public int SpeedInKMPH { get; set; }
        public int Heading { get; set; }

        public override string ToString()
        {
            var altitude = AltitudeInMeters.HasValue ? $"ALT:{AltitudeInMeters}" : "";
            return $"REG;{GpsTimestamp.ToUnixTimestamp()};{Longitude};{Latitude};{SpeedInKMPH};{Heading};{altitude};;;;;";
        }
    }
}
