using System;
using Abp.Auditing;

namespace DispatcherWeb.Dispatching.Dto
{
    public class AddSignatureInput
    {
        public int? DispatchId { get; set; }

        public Guid? Guid { get; set; } //deprecated, temporarily kept for backwards compatibility

        [DisableAuditing]
        public string Signature { get; set; }

        public string SignatureName { get; set; }

        public Guid? DeferredSignatureId { get; set; }
    }
}
