using System.Collections.Generic;
using Newtonsoft.Json;

namespace DispatcherWeb.Fulcrum.Dto
{
    public class FulcrumResponseObject<T>
    {
        [JsonProperty("data")]
        public List<T> Data { get; set; }
    }
}
