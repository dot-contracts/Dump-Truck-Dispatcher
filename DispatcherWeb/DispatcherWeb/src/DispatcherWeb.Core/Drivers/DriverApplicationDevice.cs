using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Abp.Domain.Entities.Auditing;
using DispatcherWeb.Infrastructure;

namespace DispatcherWeb.Drivers
{
    [Table("DriverApplicationDevice")]
    public class DriverApplicationDevice : FullAuditedEntity
    {
        [StringLength(EntityStringFieldLengths.DriverApplicationDevice.Useragent)]
        public string Useragent { get; set; }

        [StringLength(EntityStringFieldLengths.DriverApplicationDevice.AppVersion)]
        public string AppVersion { get; set; }

        public DateTime? LastSeen { get; set; }
    }
}
