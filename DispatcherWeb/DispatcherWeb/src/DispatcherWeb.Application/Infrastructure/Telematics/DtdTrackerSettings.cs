namespace DispatcherWeb.Infrastructure.Telematics
{
    public class DtdTrackerSettings
    {
        public string AccountName { get; set; }
        public int AccountId { get; internal set; }
        public int UserId { get; internal set; }

        public bool IsEmpty()
        {
            return string.IsNullOrEmpty(AccountName) || AccountId == 0;
        }
    }
}
