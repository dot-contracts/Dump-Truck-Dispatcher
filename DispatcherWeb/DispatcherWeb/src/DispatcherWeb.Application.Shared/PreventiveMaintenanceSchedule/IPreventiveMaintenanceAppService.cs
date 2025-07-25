using System.Threading.Tasks;
using Abp.Application.Services.Dto;
using DispatcherWeb.PreventiveMaintenanceSchedule.Dto;

namespace DispatcherWeb.PreventiveMaintenanceSchedule
{
    public interface IPreventiveMaintenanceAppService
    {
        Task<PreventiveMaintenanceEditDto> GetForEdit(NullableIdDto input);
        Task<PreventiveMaintenanceEditDto> Save(PreventiveMaintenanceEditDto model);
    }
}