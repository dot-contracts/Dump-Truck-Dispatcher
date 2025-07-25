using System;

namespace DispatcherWeb.Dispatching.Dto
{
    public class DispatchCompleteInfoTicketDto
    {
        public string TicketNumber { get; set; }

        public decimal? MaterialQuantity { get; set; }

        public decimal? FreightQuantity { get; set; }

        public int? MaterialItemId { get; set; }

        public string MaterialItemName { get; set; }

        public int? FreightItemId { get; set; }

        public string FreightItemName { get; set; }

        public int? LoadCount { get; set; }

        public Guid? TicketPhotoId { get; set; }

    }
}
