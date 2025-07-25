using System.ComponentModel.DataAnnotations;

namespace DispatcherWeb.Scheduling.Dto
{
    public class SetOrderDirectionsInput
    {
        public int OrderId { get; set; }

        [StringLength(1000)]
        public string Directions { get; set; }
    }
}
