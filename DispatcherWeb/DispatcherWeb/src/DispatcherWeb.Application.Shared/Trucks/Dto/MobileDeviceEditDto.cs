using System.ComponentModel.DataAnnotations;
using DispatcherWeb.Infrastructure;

namespace DispatcherWeb.Trucks.Dto
{
    public class MobileDeviceEditDto
    {
        public MobileDeviceEditDto()
        {
        }

        public MobileDeviceEditDto(DeviceTypeEnum deviceType)
        {
            DeviceType = deviceType;
        }

        public int? Id { get; set; }

        public DeviceTypeEnum DeviceType { get; set; }

        [StringLength(EntityStringFieldLengths.Truck.Make)]
        public string Make { get; set; }

        [StringLength(EntityStringFieldLengths.Truck.Model)]
        public string Model { get; set; }

        [StringLength(EntityStringFieldLengths.Truck.Imei)]
        public string Imei { get; set; }

        [StringLength(EntityStringFieldLengths.Truck.SimId)]
        public string SimId { get; set; }
    }
}
