using System.Collections.Generic;
using Newtonsoft.Json;

namespace DispatcherWeb.Fulcrum.Dto
{
    public class FulcrumPageResponse<T>
    {
        [JsonProperty("data")]
        public List<T> Data { get; set; }

        [JsonProperty("page")]
        public FulcrumResponsePageObject Page { get; set; }
    }
}
