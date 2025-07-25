using Newtonsoft.Json;

namespace DispatcherWeb.Infrastructure.Telematics.Dto.DtdTracker
{
    public class TokenLoginResultUser
    {
        public int Id { get; set; }

        [JsonProperty("nm")]
        public string Name { get; set; }

        [JsonProperty("bact")]
        public int AccountId { get; set; }
    }
}
