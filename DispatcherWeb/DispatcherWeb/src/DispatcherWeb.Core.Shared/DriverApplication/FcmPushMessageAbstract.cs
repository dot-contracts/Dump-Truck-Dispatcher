using System;

namespace DispatcherWeb.DriverApplication
{
    public abstract class FcmPushMessageAbstract
    {
        public FcmPushMessageAbstract()
        {
            Guid = Guid.NewGuid();
        }

        public Guid Guid { get; set; }

        public abstract FcmPushMessageType MessageType { get; }
    }
}
