using System;
using DispatcherWeb.Dto;

namespace DispatcherWeb.DriverApp.Users.Dto
{
    public class GetInput : PagedInputDto
    {
        public long? Id { get; set; }
        public string[] IsInAnyRole { get; set; }
        public DateTime? ModifiedAfterDateTime { get; set; }
    }
}
