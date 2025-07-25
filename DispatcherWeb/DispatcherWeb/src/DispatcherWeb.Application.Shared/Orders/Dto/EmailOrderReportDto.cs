namespace DispatcherWeb.Orders.Dto
{
    public class EmailOrderReportDto
    {
        public int Id { get; set; }
        public bool UseReceipts { get; set; }
        public string From { get; set; }
        public string To { get; set; }
        public string CC { get; set; }
        public string Subject { get; set; }
        public string Body { get; set; }
    }
}
