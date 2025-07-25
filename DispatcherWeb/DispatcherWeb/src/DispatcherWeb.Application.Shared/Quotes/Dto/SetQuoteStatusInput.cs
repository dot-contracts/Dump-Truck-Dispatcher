namespace DispatcherWeb.Quotes.Dto
{
    public class SetQuoteStatusInput
    {
        public int Id { get; set; }

        public QuoteStatus Status { get; set; }
    }
}
