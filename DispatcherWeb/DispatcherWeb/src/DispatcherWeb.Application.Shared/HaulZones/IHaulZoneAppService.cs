using System.Threading.Tasks;
using Abp.Application.Services;
using Abp.Application.Services.Dto;
using DispatcherWeb.Dto;
using DispatcherWeb.HaulZones.Dto;

namespace DispatcherWeb.HaulZones
{
    public interface IHaulZoneAppService : IApplicationService
    {
        Task DeleteHaulZone(EntityDto input);
        Task EditHaulZone(HaulZoneEditDto model);
        Task<HaulZoneEditDto> GetHaulZoneForEdit(NullableIdNameDto input);
        Task<PagedResultDto<HaulZoneDto>> GetHaulZones(GetHaulZonesInput input);
    }
}
