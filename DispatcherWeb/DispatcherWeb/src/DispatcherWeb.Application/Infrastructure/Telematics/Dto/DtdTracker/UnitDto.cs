using Newtonsoft.Json;

namespace DispatcherWeb.Infrastructure.Telematics.Dto.DtdTracker
{
    public class UnitDto
    {
        [JsonProperty("nm")]
        public string Name { get; set; }
        //"nm": "3361", /* name */

        public int Id { get; set; }
        //"id": 400431854, /* unit ID */

        [JsonProperty("bact")]
        public int AccountId { get; set; }
        //"bact": 400346472, /* account ID */

        [JsonProperty("uid")]
        public string UniqueId { get; set; }

        [JsonProperty("cneh")]
        public decimal EngineHours { get; set; }
        //"cneh": 0, /* engine hours counter, h */

        [JsonProperty("cnm")]
        public decimal Mileage { get; set; }
        //"cnm": 0, /* mileage counter, km or miles */

        [JsonProperty("mu")]
        public WialonMeasureUnits MeasureUnits { get; set; }
        //"mu": 1, /* measure units: 0 - si, 1 - us, 2 - imperial, 3 - metric with gallons */


        [JsonProperty("hw")]
        public long DeviceTypeId { get; set; }

        [JsonProperty("psw")]
        public string Password { get; set; }

        //"act":<bool>	/* unit deactivated - 0, activated - 1 */
        //"dactt":<long>	/* deactivation time UNIX, 0 - unit is activated */
    }
}
