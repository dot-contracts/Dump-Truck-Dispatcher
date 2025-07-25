using System.Collections.Generic;
using DispatcherWeb.Dto;

namespace DispatcherWeb.UnitsOfMeasure.Dto
{
    public class GetUnitsOfMeasureSelectListInput : GetSelectListInput
    {
        public bool GetUomBaseId { get; set; }

        public List<UnitOfMeasureBaseEnum> UomBaseIds { get; set; }
    }
}
