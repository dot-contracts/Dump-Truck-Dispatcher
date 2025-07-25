using System.Collections.Generic;
using System.Threading.Tasks;
using Abp.Application.Services;
using DispatcherWeb.DashboardCustomization.Dto;

namespace DispatcherWeb.DashboardCustomization
{
    public interface IDashboardCustomizationAppService : IApplicationService
    {
        Task<Dashboard> GetUserDashboard(GetDashboardInput input);

        Task SavePage(SavePageInput input);

        Task RenamePage(RenamePageInput input);

        Task<AddNewPageOutput> AddNewPage(AddNewPageInput input);

        Task<Widget> AddWidget(AddWidgetInput input);

        Task DeletePage(DeletePageInput input);

        Task<DashboardOutput> GetDashboardDefinition(GetDashboardInput input);

        Task<List<WidgetOutput>> GetAllWidgetDefinitions(GetDashboardInput input);
    }
}
