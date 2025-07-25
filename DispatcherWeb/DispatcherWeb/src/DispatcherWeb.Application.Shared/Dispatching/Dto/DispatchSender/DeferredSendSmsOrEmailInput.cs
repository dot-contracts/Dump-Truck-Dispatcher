using System;

namespace DispatcherWeb.Dispatching.Dto.DispatchSender
{
    public class DeferredSendSmsOrEmailInput : SendSmsOrEmailInput
    {
        public DispatchDto OldActiveDispatch { get; set; }
        public DispatchDto NewActiveDispatch { get; set; }
        public override bool? ActiveDispatchWasChanged { get => OldActiveDispatch?.Id != NewActiveDispatch?.Id; set { throw new NotSupportedException(); } }
        public DispatchDto Dispatch { get; set; }
    }
}
