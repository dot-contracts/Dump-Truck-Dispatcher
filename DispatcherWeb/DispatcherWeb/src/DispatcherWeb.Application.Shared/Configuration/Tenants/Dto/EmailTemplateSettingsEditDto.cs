using System.ComponentModel.DataAnnotations;
using DispatcherWeb.Infrastructure;

namespace DispatcherWeb.Configuration.Tenants.Dto
{
    public class EmailTemplateSettingsEditDto
    {
        [StringLength(EntityStringFieldLengths.HostEmail.Subject)]
        public string UserEmailSubjectTemplate { get; set; }

        [StringLength(EntityStringFieldLengths.HostEmail.Body)]
        public string UserEmailBodyTemplate { get; set; }

        [StringLength(EntityStringFieldLengths.HostEmail.Subject)]
        public string DriverEmailSubjectTemplate { get; set; }

        [StringLength(EntityStringFieldLengths.HostEmail.Body)]
        public string DriverEmailBodyTemplate { get; set; }

        [StringLength(EntityStringFieldLengths.HostEmail.Subject)]
        public string LeaseHaulerInviteEmailSubjectTemplate { get; set; }

        [StringLength(EntityStringFieldLengths.HostEmail.Body)]
        public string LeaseHaulerInviteEmailBodyTemplate { get; set; }

        [StringLength(EntityStringFieldLengths.HostEmail.Subject)]
        public string LeaseHaulerDriverEmailSubjectTemplate { get; set; }

        [StringLength(EntityStringFieldLengths.HostEmail.Body)]
        public string LeaseHaulerDriverEmailBodyTemplate { get; set; }

        [StringLength(EntityStringFieldLengths.HostEmail.Subject)]
        public string LeaseHaulerJobRequestEmailSubjectTemplate { get; set; }

        [StringLength(EntityStringFieldLengths.HostEmail.Body)]
        public string LeaseHaulerJobRequestEmailBodyTemplate { get; set; }

        [StringLength(EntityStringFieldLengths.HostEmail.Subject)]
        public string CustomerPortalEmailSubjectTemplate { get; set; }

        [StringLength(EntityStringFieldLengths.HostEmail.Body)]
        public string CustomerPortalEmailBodyTemplate { get; set; }
    }
}
