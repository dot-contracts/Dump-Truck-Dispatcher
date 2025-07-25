using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Abp.Domain.Entities.Auditing;
using DispatcherWeb.Authorization.Users;
using DispatcherWeb.Infrastructure;

namespace DispatcherWeb.CustomerNotifications
{
    [Table("CustomerNotification")]
    public class CustomerNotification : FullAuditedEntity
    {
        public DateTime StartDate { get; set; }

        public DateTime EndDate { get; set; }

        [StringLength(EntityStringFieldLengths.CustomerNotification.Title)]
        public string Title { get; set; }

        [StringLength(EntityStringFieldLengths.CustomerNotification.Body)]
        public string Body { get; set; }

        public virtual ICollection<CustomerNotificationEdition> Editions { get; set; }

        public virtual ICollection<CustomerNotificationTenant> Tenants { get; set; }

        public HostEmailType Type { get; set; }

        public virtual ICollection<CustomerNotificationRole> Roles { get; set; }

        public virtual User CreatorUser { get; set; }

        public virtual ICollection<DismissedCustomerNotification> Dismissions { get; set; }
    }
}
