using System.ComponentModel.DataAnnotations;
using Abp.Application.Services.Dto;

namespace DispatcherWeb.VehicleServiceTypes.Dto
{
    public class VehicleServiceTypeDto : EntityDto
    {
        [Required]
        [StringLength(100)]
        public string Name { get; set; }

    }
}
