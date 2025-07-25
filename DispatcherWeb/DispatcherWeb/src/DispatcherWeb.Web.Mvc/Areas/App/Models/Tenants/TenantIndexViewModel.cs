using System.Collections.Generic;
using DispatcherWeb.Editions.Dto;

namespace DispatcherWeb.Web.Areas.App.Models.Tenants
{
    public class TenantIndexViewModel
    {
        public List<SubscribableEditionComboboxItemDto> EditionItems { get; set; }
    }
}