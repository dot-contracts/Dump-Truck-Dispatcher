using System;
using System.ComponentModel.DataAnnotations;
using Abp.Application.Services.Dto;

namespace DispatcherWeb.Infrastructure.SecureFiles.Dto
{
    public class SecureFileDefinitionDto : EntityDto<Guid>
    {
        [StringLength(200)]
        [Required]
        public string Client { get; set; }

        [StringLength(500)]
        public string Description { get; set; }

        public string[] FileNames { get; set; }
    }
}
