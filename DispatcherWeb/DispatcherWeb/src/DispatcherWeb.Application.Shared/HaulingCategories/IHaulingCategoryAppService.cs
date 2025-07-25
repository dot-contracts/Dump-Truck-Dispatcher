using System.Collections.Generic;
using System.Threading.Tasks;
using Abp.Application.Services;
using Abp.Application.Services.Dto;
using DispatcherWeb.Items.Dto;

namespace DispatcherWeb.HaulingCategories
{
    public interface IHaulingCategoryAppService : IApplicationService
    {
        Task<HaulingCategoryEditDto> GetHaulingCategoryForEdit(NullableIdDto input);

        Task<List<HaulingCategoryPriceDto>> GetEmptyHaulingCategoryPrices();

        Task DeleteHaulingCategory(EntityDto input);

        Task<HaulZonePricingDto> GetFreightRateForJob(GetRateForJobInput input);
    }
}
