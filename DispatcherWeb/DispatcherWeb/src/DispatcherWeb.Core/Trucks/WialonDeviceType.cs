using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Abp.Domain.Entities;
using DispatcherWeb.Infrastructure;

namespace DispatcherWeb.Trucks
{
    [Table("WialonDeviceType")]
    public class WialonDeviceType : Entity<long>
    {
        [StringLength(EntityStringFieldLengths.WialonDeviceType.Name)]
        public string Name { get; set; }

        [StringLength(EntityStringFieldLengths.WialonDeviceType.DeviceCategory)]
        public string DeviceCategory { get; set; } //hw_category, captioned "hardware type" in the Wialon documentation

        public int? TcpPort { get; set; }

        public int? UdpPort { get; set; }

        [StringLength(EntityStringFieldLengths.WialonDeviceType.ServerAddress)]
        public string ServerAddress { get; set; }
    }
}
