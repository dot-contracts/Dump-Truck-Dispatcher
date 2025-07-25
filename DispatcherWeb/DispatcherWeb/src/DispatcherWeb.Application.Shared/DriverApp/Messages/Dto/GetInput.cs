using System.ComponentModel.DataAnnotations;
using DispatcherWeb.Dto;

namespace DispatcherWeb.DriverApp.Messages.Dto
{
    public class GetInput : PagedInputDto
    {
        public long? AfterId { get; set; }

        [Required]
        public long? TargetUserId { get; set; }
    }
}
