using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using DispatcherWeb.Infrastructure;

namespace DispatcherWeb.HostEmails.Dto
{
    public class SendHostEmailInput
    {
        [StringLength(EntityStringFieldLengths.HostEmail.Subject)]
        public string Subject { get; set; }

        [StringLength(EntityStringFieldLengths.HostEmail.Body)]
        public string Body { get; set; }

        public List<int> EditionIds { get; set; }

        public bool? ActiveFilter { get; set; }

        public List<int> TenantIds { get; set; }

        public HostEmailType Type { get; set; }

        public List<string> RoleNames { get; set; }
    }
}
