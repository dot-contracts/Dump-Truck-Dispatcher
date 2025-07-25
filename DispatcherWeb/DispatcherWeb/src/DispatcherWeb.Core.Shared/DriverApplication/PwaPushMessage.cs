using System;

namespace DispatcherWeb.DriverApplication
{
    public class PwaPushMessage
    {
        public string Message { get; set; }
        public DriverApplicationPushAction Action { get; set; }
        public Guid Guid { get; set; }
    }
}
