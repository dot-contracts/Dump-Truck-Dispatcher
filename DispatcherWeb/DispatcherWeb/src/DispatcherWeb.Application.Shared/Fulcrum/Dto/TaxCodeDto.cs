using System.Collections.Generic;

namespace DispatcherWeb.Fulcrum.Dto
{
    public class TaxCodeDto
    {
        public string Uiid { get; set; }

        public string DtId { get; set; }

        public bool Inactive { get; set; }

        public List<TaxCodeDetailDto> Details { get; set; }

    }

}
