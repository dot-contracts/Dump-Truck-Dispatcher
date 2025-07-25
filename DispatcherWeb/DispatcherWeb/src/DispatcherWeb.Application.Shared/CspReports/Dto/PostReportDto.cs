using Newtonsoft.Json;

namespace DispatcherWeb.CspReports.Dto
{
    public class PostReportDto
    {
        [JsonProperty(PropertyName = "csp-report")]
        public CspReportDto CspReport { get; set; }
    }
}
