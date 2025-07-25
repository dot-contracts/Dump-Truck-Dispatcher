using Newtonsoft.Json;

namespace DispatcherWeb.Infrastructure.Telematics.Dto.DtdTracker
{
    public class DeviceTypeDto
    {
        public long Id { get; set; }
        public string Name { get; set; }

        [JsonProperty("hw_category")] //captioned "hardware type" in the documentation
        public string DeviceCategory { get; set; }

        [JsonProperty("tp")]
        public string TcpPort { get; set; }

        [JsonProperty("up")]
        public string UdpPort { get; set; }
    }
}
