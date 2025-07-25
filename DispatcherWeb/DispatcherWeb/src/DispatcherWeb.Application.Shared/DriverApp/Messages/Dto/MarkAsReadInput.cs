using System.ComponentModel.DataAnnotations;

namespace DispatcherWeb.DriverApp.Messages.Dto
{
    public class MarkAsReadInput
    {
        [Required]
        public long? TargetUserId { get; set; }
    }
}
