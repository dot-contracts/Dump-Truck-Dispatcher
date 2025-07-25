using System.Threading.Tasks;
using Abp.Application.Services;
using DispatcherWeb.Configuration.Host.Dto;
using DispatcherWeb.Configuration.Tenants.Dto;

namespace DispatcherWeb.Configuration.Tenants
{
    public interface ITenantSettingsAppService : IApplicationService
    {
        Task<TenantSettingsEditDto> GetAllSettings();

        Task UpdateAllSettings(TenantSettingsEditDto input);

        Task SendTestEmail(SendTestEmailInput input);
        Task<bool> CanLinkDtdTrackerAccount();
        Task LinkDtdTrackerAccount(string accessToken);

        Task ClearLogo();
        Task ClearCustomCss();
    }
}
