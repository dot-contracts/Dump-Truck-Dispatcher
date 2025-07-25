using System;
using Abp.Auditing;

namespace DispatcherWeb.Dispatching.Dto
{
    public class UploadDeferredBinaryObjectInput
    {
        public Guid DeferredId { get; set; }
        public DeferredBinaryObjectDestination Destination { get; set; }

        [DisableAuditing]
        public string BytesString { get; set; }
    }
}
