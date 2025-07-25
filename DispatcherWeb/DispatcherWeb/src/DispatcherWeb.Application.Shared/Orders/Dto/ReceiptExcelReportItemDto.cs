namespace DispatcherWeb.Orders.Dto
{
    public class ReceiptExcelReportItemDto : ReceiptReportItemDto
    {
        public string FreightItemName { get; set; }
        public string MaterialItemName { get; set; }
        public string LoadAtName { get; set; }
        public string DeliverToName { get; set; }
        public string MaterialUomName { get; set; }
        public string FreightUomName { get; set; }
        public DesignationEnum Designation { get; set; }
        public string DesignationName => Designation.GetDisplayName();
    }
}
