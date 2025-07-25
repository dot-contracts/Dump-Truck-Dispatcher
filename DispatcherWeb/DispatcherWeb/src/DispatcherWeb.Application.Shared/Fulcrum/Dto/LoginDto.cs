using Newtonsoft.Json;

namespace DispatcherWeb.Fulcrum.Dto
{
    public class LoginDto
    {
        [JsonProperty("token")]
        public string Token { get; set; }

        [JsonProperty("tokenExpiresUTC")]
        public string TokenExpiresUTC { get; set; }

        [JsonProperty("email")]
        public string Email { get; set; }

        [JsonProperty("customerNumber")]
        public string CustomerNumber { get; set; }

        [JsonProperty("role")]
        public string Role { get; set; }
    }
}
