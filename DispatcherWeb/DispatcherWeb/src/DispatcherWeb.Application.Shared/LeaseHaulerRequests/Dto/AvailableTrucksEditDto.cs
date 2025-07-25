using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace DispatcherWeb.LeaseHaulerRequests.Dto
{
    public class AvailableTrucksEditDto
    {
        public int Id { get; set; }

        public int TenantId { get; set; }

        public int LeaseHaulerId { get; set; }

        public int OfficeId { get; set; }

        public int LeaseHaulerRequestId { get; set; }

        public string CustomerName { get; set; }

        public DateTime Date { get; set; }

        public Shift? Shift { get; set; }

        public string ShiftName { get; set; }

        [Required(ErrorMessage = "Number of trucks available is required")]
        public int? Available { get; set; }
        public int? Approved { get; set; }
        public int? Scheduled { get; set; }
        public string Comments { get; set; }
        public string Message { get; set; }

        public bool HasAvailableBeenSent => Available.HasValue;
        public bool IsExpired { get; set; }
        public bool ShowTruckControls => Approved > 0;

        public List<AvailableTrucksTruckEditDto> Trucks { get; set; }
    }
}
