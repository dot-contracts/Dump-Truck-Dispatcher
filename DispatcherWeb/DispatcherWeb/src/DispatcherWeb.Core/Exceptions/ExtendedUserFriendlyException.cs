using System.Collections.Generic;
using Abp.UI;

namespace DispatcherWeb.Exceptions
{
    public class ExtendedUserFriendlyException : UserFriendlyException
    {
        public Dictionary<string, object> Parameters { get; } = new Dictionary<string, object>();

        public string Kind
        {
            get { return GetParameter("Kind") as string; }
            set { SetParameter("Kind", value); }
        }

        protected object GetParameter(string key)
        {
            return Parameters.ContainsKey(key) ? Parameters[key] : null;
        }

        protected void SetParameter(string key, object value)
        {
            Parameters[key] = value;
        }

        public ExtendedUserFriendlyException(string message, string details)
            : base(message, details)
        {
        }
    }
}
