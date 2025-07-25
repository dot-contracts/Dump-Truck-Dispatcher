using System.ComponentModel.DataAnnotations;

namespace DispatcherWeb.Dispatching.Dto
{
    public class SendDispatchMessageInput
    {
        public int OrderLineId { get; set; }

        [Required]
        public string Message { get; set; }

        [Required]
        public int[] OrderLineTruckIds { get; set; }

        public int NumberOfDispatches { get; set; }
        public bool IsMultipleLoads { get; set; }
        public bool AddDispatchBasedOnTime { get; set; }
        public bool FirstDispatchForDay { get; set; }
        public bool UseTruckCapacityForLoadQuantity { get; set; }
        public decimal? LoadMaterialQuantity { get; set; }
    }
}
