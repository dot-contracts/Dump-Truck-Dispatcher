namespace DispatcherWeb.Exceptions
{
    public class EntityDeletedException : ExtendedUserFriendlyException
    {
        public string EntityKind
        {
            get { return GetParameter("EntityKind") as string; }
            set { SetParameter("EntityKind", value); }
        }

        public EntityDeletedException(string entityKind, string message, string details = null)
            : base(message, details)
        {
            Kind = "EntityDeletedException";
            EntityKind = entityKind;
        }
    }
}
