using System.ComponentModel.DataAnnotations;

namespace DispatcherWeb.DriverApp.Messages.Dto
{
    public class PostInput
    {
        public long TargetUserId { get; set; }

        public int? SourceTruckId { get; set; }

        public int? SourceTrailerId { get; set; }

        public int? SourceDriverId { get; set; }

        [Required]
        public string Message { get; set; }
    }
}
