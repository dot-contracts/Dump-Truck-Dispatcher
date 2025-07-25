using System.Collections.Generic;
using DispatcherWeb.Dto;

namespace DispatcherWeb.HostEmails.Dto
{
    public class HostEmailViewDto
    {
        public int Id { get; set; }
        public string Subject { get; set; }
        public string Body { get; set; }
        public List<SelectListDto> Editions { get; set; }
        public bool? ActiveFilter { get; set; }
        public List<SelectListDto> Tenants { get; set; }
        public HostEmailType Type { get; set; }
        public List<SelectListDto> Roles { get; set; }

        public class Receiver
        {
            public string FullName { get; set; }
            public string EmailAddress { get; set; }
        }
    }
}
