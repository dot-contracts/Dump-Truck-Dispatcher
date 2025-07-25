using System;
using System.ComponentModel.DataAnnotations;

namespace DispatcherWeb.LeaseHaulerRequests.Dto
{
    public class SendRequestsInput
    {
        public DateTime Date { get; set; }
        public Shift? Shift { get; set; }
        public int OfficeId { get; set; }

        [Required]
        public string Message { get; set; }

        public int[] LeaseHaulerIds { get; set; }
    }
}
