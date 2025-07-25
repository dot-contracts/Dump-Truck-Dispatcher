using Newtonsoft.Json;

namespace DispatcherWeb.Infrastructure.Telematics.Dto.DtdTracker
{
    public class AccountDto
    {
        [JsonProperty("nm")]
        public string Name { get; set; }
    }
}
