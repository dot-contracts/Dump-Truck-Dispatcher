namespace DispatcherWeb.Infrastructure.Templates
{
    public class TemplateTokenDto
    {
        public string TruckCode { get; set; }
        public string DeliveryDate { get; set; }
        public string Shift { get; set; }
        public string OrderNumber { get; set; }
        public string Customer { get; set; }
        public string Directions { get; set; }
        public string TimeOnJob { get; set; }
        public string StartTime { get; set; }
        public string Item { get; set; }
        public string LoadAt { get; set; }
        public string MaterialQuantity { get; set; }
        public string FreightQuantity { get; set; }
        public string MaterialUom { get; set; }
        public string FreightUom { get; set; }
        public string UserFirstName { get; set; }
        public string UserLastName { get; set; }
        public string UserPhoneNumber { get; set; }
        public string CompanyName { get; set; }
        public string DeliverTo { get; set; }
        public string ChargeTo { get; set; }
        public string Note { get; set; }
        public DesignationEnum Designation { get; set; }
    }
}
