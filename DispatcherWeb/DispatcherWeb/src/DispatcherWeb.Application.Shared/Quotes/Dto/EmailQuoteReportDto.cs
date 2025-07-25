namespace DispatcherWeb.Quotes.Dto
{
    public class EmailQuoteReportDto
    {
        public int QuoteId { get; set; }
        public string From { get; set; }
        public string To { get; set; }
        public string CC { get; set; }
        public string Subject { get; set; }
        public string Body { get; set; }
        public bool HideLoadAt { get; set; }
    }
}
