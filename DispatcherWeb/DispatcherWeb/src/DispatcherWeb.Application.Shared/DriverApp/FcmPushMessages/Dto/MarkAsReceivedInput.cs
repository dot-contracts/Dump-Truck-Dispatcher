using System;
using System.Collections.Generic;

namespace DispatcherWeb.DriverApp.FcmPushMessages.Dto
{
    public class MarkAsReceivedInput
    {
        public List<Guid> Guids { get; set; }
    }
}
