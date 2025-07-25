using System.ComponentModel.DataAnnotations;
using Abp.Domain.Entities;
using DispatcherWeb.Infrastructure;

namespace DispatcherWeb.HostEmails
{
    public class HostEmailRole : Entity, ISoftDelete
    {
        public bool IsDeleted { get; set; }

        public int HostEmailId { get; set; }
        public virtual HostEmail HostEmail { get; set; }

        [StringLength(EntityStringFieldLengths.HostEmailRole.RoleName)]
        public string RoleName { get; set; }
    }
}
