using System.Collections.Generic;
using DispatcherWeb.Dto;

namespace DispatcherWeb.MultiTenancy.Dto
{
    public class GetTenantsSelectListInput : GetSelectListInput
    {
        public List<int> EditionIds { get; set; }
        public bool? ActiveFilter { get; set; }
    }
}
