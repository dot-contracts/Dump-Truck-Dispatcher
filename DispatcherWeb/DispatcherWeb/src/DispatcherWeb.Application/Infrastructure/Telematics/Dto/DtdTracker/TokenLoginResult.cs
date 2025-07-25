using Newtonsoft.Json;

namespace DispatcherWeb.Infrastructure.Telematics.Dto.DtdTracker
{
    public class TokenLoginResult : WialonResult
    {
        [JsonProperty("eid")]
        public string SessionId { get; set; }
        public bool ShouldSerializeSessionId()
        {
            //we should never return sessionId to the front end
            return false;
        }

        [JsonProperty("hw_gw_ip")]
        public string HardwareGatewayIp { get; set; }

        [JsonProperty("hw_gw_dns")]
        public string HardwareGatewayDomain { get; set; }

        public TokenLoginResultUser User { get; set; }
    }
}
