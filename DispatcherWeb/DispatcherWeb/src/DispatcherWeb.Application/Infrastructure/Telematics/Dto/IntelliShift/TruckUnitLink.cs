using Newtonsoft.Json;

namespace DispatcherWeb.Infrastructure.Telematics.Dto.IntelliShift
{
    public class TruckUnitLink
    {
        [JsonProperty("targetUri")]
        public string TargetUri { get; set; }

        [JsonProperty("relationType")]
        public string RelationType { get; set; }

        [JsonProperty("method")]
        public string Method { get; set; }
    }
}
