using Abp.Application.Editions;
using Abp.Domain.Entities;

namespace DispatcherWeb.HostEmails
{
    public class HostEmailEdition : Entity, ISoftDelete
    {
        public bool IsDeleted { get; set; }

        public int HostEmailId { get; set; }
        public virtual HostEmail HostEmail { get; set; }

        public int EditionId { get; set; }
        public virtual Edition Edition { get; set; }
    }
}
