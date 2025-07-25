namespace DispatcherWeb.Orders.Dto
{
    public class EmailOrderReportResult
    {
        public bool Success { get; set; }
        public bool FromEmailAddressIsNotVerifiedError { get; set; }
    }
}
