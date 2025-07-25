using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using DispatcherWeb.Dto;
using DispatcherWeb.Infrastructure;

namespace DispatcherWeb.CustomerNotifications.Dto
{
    public class CustomerNotificationEditDto
    {
        public int? Id { get; set; }

        [Required]
        public DateTime? StartDate { get; set; }

        [Required]
        public DateTime? EndDate { get; set; }

        [StringLength(EntityStringFieldLengths.CustomerNotification.Title)]
        public string Title { get; set; }

        [StringLength(EntityStringFieldLengths.CustomerNotification.Body)]
        public string Body { get; set; }

        public List<int> EditionIds { get; set; }

        public List<SelectListDto> Editions { get; set; }

        public List<int> TenantIds { get; set; }

        public List<SelectListDto> Tenants { get; set; }

        public HostEmailType Type { get; set; }

        public List<string> RoleNames { get; set; }

        public List<SelectListDto> Roles { get; set; }
    }
}
