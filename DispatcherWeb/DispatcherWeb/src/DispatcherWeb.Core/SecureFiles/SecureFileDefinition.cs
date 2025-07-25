using System;
using System.ComponentModel.DataAnnotations;
using Abp.Domain.Entities;

namespace DispatcherWeb.SecureFiles
{
    public class SecureFileDefinition : Entity<Guid>
    {
        [StringLength(200)]
        [Required]
        public string Client { get; set; }
        [StringLength(500)]
        public string Description { get; set; }
    }
}
