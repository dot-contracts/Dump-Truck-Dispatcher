using System.ComponentModel.DataAnnotations;
using DispatcherWeb.Infrastructure;

namespace DispatcherWeb.Orders.Dto
{
    public class OrderInternalNotesDto
    {
        public int OrderId { get; set; }

        [StringLength(EntityStringFieldLengths.Order.InternalNotes)]
        public string InternalNotes { get; set; }
    }
}
