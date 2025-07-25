using System;

namespace DispatcherWeb.Dispatching.Dto
{
    public class EditDispatchDto
    {
        public int Id { get; set; }
        public decimal? MaterialQuantity { get; set; }
        public decimal? FreightQuantity { get; set; }
        public DateTime? TimeOnJob { get; set; }
    }
}
