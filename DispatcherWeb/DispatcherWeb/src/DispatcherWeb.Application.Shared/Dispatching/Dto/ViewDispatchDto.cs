using System;
using System.Collections.Generic;
using System.Linq;
using DispatcherWeb.Tickets;

namespace DispatcherWeb.Dispatching.Dto
{
    public class ViewDispatchDto
    {
        public int Id { get; set; }
        public string TruckCode { get; set; }
        public string CustomerName { get; set; }
        public string Item { get; set; }
        public decimal? TicketMaterialQuantity => GetTicketToUse()?.GetMaterialQuantity();
        public decimal? TicketFreightQuantity => GetTicketToUse()?.GetFreightQuantity();
        public decimal? MaterialQuantity { get; set; }
        public decimal? FreightQuantity { get; set; }
        public int? MaterialUomId { get; set; }
        public int? FreightUomId { get; set; }
        public DateTime? TimeOnJob { get; set; }
        public DispatchStatus Status { get; set; }
        public DateTime? Sent { get; set; }
        public DateTime? Loaded { get; set; }
        public DateTime? Delivered { get; set; }
        public List<ViewDispatchTicketDto> Tickets { get; set; }

        private ViewDispatchTicketDto GetTicketToUse()
        {
            return Tickets.FirstOrDefault();
        }
    }
}
