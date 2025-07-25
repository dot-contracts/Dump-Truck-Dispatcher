using System.Threading.Tasks;
using Abp.Application.Services;
using Abp.Application.Services.Dto;
using DispatcherWeb.Dto;
using DispatcherWeb.PricingTiers.Dto;

namespace DispatcherWeb.PricingTiers
{
    public interface IPricingTierAppService : IApplicationService
    {
        Task<PagedResultDto<PricingTierDto>> GetPricingTiers(GetPricingTiersInput input);

        Task DeletePricingTier(EntityDto input);

        Task<PricingTierEditDto> GetPricingTierForEdit(NullableIdDto input);

        Task EditPricingTier(PricingTierEditDto input);

        Task<PagedResultDto<PricingTierSelectListDto>> GetPricingTiersSelectList(GetSelectListInput input);
    }
}
