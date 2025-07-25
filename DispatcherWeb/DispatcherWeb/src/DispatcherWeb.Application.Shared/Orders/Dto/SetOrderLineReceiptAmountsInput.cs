namespace DispatcherWeb.Orders.Dto
{
    public class SetOrderLineReceiptAmountsInput
    {
        public int OrderLineId { get; set; }
        //public int OfficeId { get; set; }
        public decimal? MaterialQuantity { get; set; }
        public decimal? FreightQuantity { get; set; }
    }
}
