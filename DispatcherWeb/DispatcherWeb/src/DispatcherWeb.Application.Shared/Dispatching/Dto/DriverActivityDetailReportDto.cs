using System.Collections.Generic;

namespace DispatcherWeb.Dispatching.Dto
{
    public class DriverActivityDetailReportDto
    {
        public string Timezone { get; set; }
        public List<DriverActivityDetailReportPageDto> Pages { get; set; }
    }
}
