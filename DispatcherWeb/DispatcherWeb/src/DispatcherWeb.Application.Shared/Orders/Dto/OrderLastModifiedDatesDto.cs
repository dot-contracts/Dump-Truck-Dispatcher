using System;

namespace DispatcherWeb.Orders.Dto
{
    public class OrderLastModifiedDatesDto
    {
        public DateTime? LastModificationTime { get; set; }
        public string LastModifierName { get; set; }
        public DateTime CreationTime { get; set; }
        public string CreatorName { get; set; }
    }
}
