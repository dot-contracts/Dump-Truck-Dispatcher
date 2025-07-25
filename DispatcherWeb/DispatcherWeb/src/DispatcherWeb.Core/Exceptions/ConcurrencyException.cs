namespace DispatcherWeb.Exceptions
{
    public class ConcurrencyException : ExtendedUserFriendlyException
    {
        public ConcurrencyException()
            : base("Concurrency Error", "We aren't sure that your last transaction was saved to the database. Please verify. If it is missing, you will need to add it again.")
        {
            Kind = "ConcurrencyException";
        }
    }
}
