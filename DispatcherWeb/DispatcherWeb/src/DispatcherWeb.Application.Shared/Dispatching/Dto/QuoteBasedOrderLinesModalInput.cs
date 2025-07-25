namespace DispatcherWeb.Dispatching.Dto
{
    public class QuoteBasedOrderLinesModalInput
    {
        public string TitleText { get; set; }

        public string SaveButtonText { get; set; }

        public bool LimitSelectionToSingleOrderLine { get; set; }
    }
}
