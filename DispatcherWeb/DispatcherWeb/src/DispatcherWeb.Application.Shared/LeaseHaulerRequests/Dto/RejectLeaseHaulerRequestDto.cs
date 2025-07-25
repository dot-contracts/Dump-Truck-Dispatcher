using System.ComponentModel.DataAnnotations;
using DispatcherWeb.Infrastructure;

namespace DispatcherWeb.LeaseHaulerRequests.Dto
{
    public class RejectLeaseHaulerRequestDto
    {
        public int Id { get; set; }

        [StringLength(EntityStringFieldLengths.LeaseHaulerRequest.Comments)]
        public string Comments { get; set; }
    }
}
