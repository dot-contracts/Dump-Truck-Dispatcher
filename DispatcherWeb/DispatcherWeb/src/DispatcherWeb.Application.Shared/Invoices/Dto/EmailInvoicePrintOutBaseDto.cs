namespace DispatcherWeb.Invoices.Dto
{
    public class EmailInvoicePrintOutBaseDto
    {
        public string From { get; set; }
        public string CC { get; set; }
        public string Subject { get; set; }
        public string Body { get; set; }

        public T CopyTo<T>(T other) where T : EmailInvoicePrintOutBaseDto
        {
            other.From = From;
            other.CC = CC;
            other.Subject = Subject;
            other.Body = Body;
            return other;
        }
    }
}
