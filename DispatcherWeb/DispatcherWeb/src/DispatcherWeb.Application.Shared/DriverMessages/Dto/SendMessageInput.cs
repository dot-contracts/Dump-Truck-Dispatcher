using System.ComponentModel.DataAnnotations;
using DispatcherWeb.Infrastructure;

namespace DispatcherWeb.DriverMessages.Dto
{
    public class SendMessageInput
    {
        [StringLength(EntityStringFieldLengths.DriverMessage.Subject)]
        public string Subject { get; set; }

        [StringLength(EntityStringFieldLengths.DriverMessage.Body)]
        public string Body { get; set; }

        public int[] DriverIds { get; set; }

        public int[] OfficeIds { get; set; }
    }
}
