using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Abp.Domain.Entities;
using DispatcherWeb.Authorization.Users;
using Microsoft.Extensions.Logging;


namespace DispatcherWeb.Drivers
{
    [Table("DriverApplicationLog")]
    public class DriverApplicationLog : Entity, IMayHaveTenant
    {
        public long OriginalLogId { get; set; }
        public bool? ServiceWorker { get; set; }
        public int BatchOrder { get; set; }
        public int? TenantId { get; set; }
        public int? DriverId { get; set; }
        public virtual Driver Driver { get; set; }
        public long? UserId { get; set; }
        public virtual User User { get; set; }
        public int? DeviceId { get; set; }
        public Guid? DeviceGuid { get; set; }
        public virtual DriverApplicationDevice Device { get; set; }
        public DateTime DateTime { get; set; }
        public LogLevel Level { get; set; }
        public string Message { get; set; }

        [StringLength(40)]
        public string AppVersion { get; set; }
    }
}
