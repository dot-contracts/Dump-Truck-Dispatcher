using Newtonsoft.Json;

namespace DispatcherWeb.Infrastructure.Telematics.Dto.DtdTracker
{
    public class WialonResult : IWialonResult
    {
        [JsonProperty("error")]
        public int ErrorCode { get; set; }

        [JsonProperty("reason")]
        public string ErrorReason { get; set; }
    }
}
