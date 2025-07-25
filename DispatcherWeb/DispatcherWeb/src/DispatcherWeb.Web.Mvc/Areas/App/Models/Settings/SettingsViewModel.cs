using System.Collections.Generic;
using Abp.Application.Services.Dto;
using DispatcherWeb.Configuration.Tenants.Dto;

namespace DispatcherWeb.Web.Areas.App.Models.Settings
{
    public class SettingsViewModel
    {
        public TenantSettingsEditDto Settings { get; set; }

        public List<ComboboxItemDto> TimezoneItems { get; set; }
    }
}