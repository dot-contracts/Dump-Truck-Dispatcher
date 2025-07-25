using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Abp.Domain.Entities;
using Abp.Domain.Entities.Auditing;
using DispatcherWeb.Authorization.Users;
using DispatcherWeb.Emailing;
using DispatcherWeb.Infrastructure;
using DispatcherWeb.Sms;

namespace DispatcherWeb.Drivers
{
    [Table("DriverMessage")]
    public class DriverMessage : FullAuditedEntity, IMustHaveTenant
    {
        public int TenantId { get; set; }

        public int DriverId { get; set; }
        public Driver Driver { get; set; }

        public DateTime TimeSent { get; set; }

        public DriverMessageType MessageType { get; set; }

        [StringLength(EntityStringFieldLengths.DriverMessage.Subject)]
        public string Subject { get; set; }

        [StringLength(EntityStringFieldLengths.DriverMessage.Body)]
        public string Body { get; set; }

        public User CreatorUser { get; set; }

        public Guid? TrackableEmailId { get; set; }

        public TrackableEmail TrackableEmail { get; set; }

        public int? SentSmsId { get; set; }
        public SentSms SentSms { get; set; }
    }
}
