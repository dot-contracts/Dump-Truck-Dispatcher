using System;

namespace DispatcherWeb.Dispatching.Dto
{
    public class OrderLineDataForDispatchMessage
    {
        public int Id { get; set; }
        public int? MaterialItemId { get; set; }
        public string FreightItemName { get; set; }
        public string LoadAtName { get; set; }
        public string DeliverToName { get; set; }
        public string ChargeTo { get; set; }
        public DateTime DeliveryDate { get; set; }
        public Shift? Shift { get; set; }
        public int OrderNumber { get; set; }
        public string Customer { get; set; }
        public string Directions { get; set; }
        public string Note { get; set; }
        public DateTime? OrderLineTimeOnJobUtc { get; set; }
        public decimal? MaterialQuantity { get; set; }
        public decimal? FreightQuantity { get; set; }
        public string MaterialUom { get; set; }
        public string FreightUom { get; set; }
        public bool IsMultipleLoads { get; set; }
        public DesignationEnum Designation { get; set; }
    }
}
