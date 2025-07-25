using System.ComponentModel.DataAnnotations;
using Abp;

namespace DispatcherWeb.Chat.Dto
{
    public class SendMessageInput
    {
        [Required]
        public long TargetUserId { get; set; }

        [Required]
        public string Message { get; set; }

        public int? SourceTruckId { get; set; }

        public int? SourceTrailerId { get; set; }

        public int? SourceDriverId { get; set; }

        //the below properties are optional and will be populated if left empty

        public UserIdentifier SenderIdentifier { get; set; }
    }
}
