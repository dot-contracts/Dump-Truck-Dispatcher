using System.ComponentModel.DataAnnotations;
using DispatcherWeb.Infrastructure;

namespace DispatcherWeb.Scheduling.Dto
{
    public class SetOrderLineNoteInput
    {
        public int? OrderLineId { get; set; }

        [StringLength(EntityStringFieldLengths.OrderLine.Note)]
        public string Note { get; set; }
    }
}
