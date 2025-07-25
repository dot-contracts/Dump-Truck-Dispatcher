using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Abp.Authorization;
using DispatcherWeb.Dto;
using DispatcherWeb.Features;

namespace DispatcherWeb.Designations
{
    [AbpAuthorize]
    public class DesignationAppService : DispatcherWebAppServiceBase, IDesignationAppService
    {
        public DesignationAppService()
        {
        }

        public async Task<List<SelectListDto>> GetDesignationSelectListItemsAsync(DesignationEnum? selectedDesignation = null)
        {
            var separateItems = await FeatureChecker.IsEnabledAsync(AppFeatures.SeparateMaterialAndFreightItems);

            var designationList = new List<SelectListDto>
            {
                GetSelectListItemFromEnum(DesignationEnum.FreightOnly),
                GetSelectListItemFromEnum(DesignationEnum.MaterialOnly),
                GetSelectListItemFromEnum(DesignationEnum.FreightAndMaterial),
            };

            //if (selectedDesignation == DesignationEnum.CounterSale
            //    || await SettingManager.GetSettingValueAsync<bool>(AppSettings.DispatchingAndMessaging.AllowCounterSales))
            //{
            //    designationList.Add(GetSelectListItemFromEnum(DesignationEnum.CounterSale));
            //}

            var historicalItems = new[]
            {
                DesignationEnum.BackhaulFreightOnly,
                DesignationEnum.BackhaulFreightAndMaterial,
                DesignationEnum.Disposal,
                DesignationEnum.BackHaulFreightAndDisposal,
                DesignationEnum.StraightHaulFreightAndDisposal,
            };

            designationList.AddRange(
                historicalItems
                    .Where(historicalItem => !separateItems || selectedDesignation == historicalItem)
                    .Select(GetSelectListItemFromEnum)
            );

            return designationList;
        }

        public static SelectListDto GetSelectListItemFromEnum(DesignationEnum designation)
        {
            return new SelectListDto
            {
                Id = ((int)designation).ToString(),
                Name = designation.GetDisplayName(),
            };
        }
    }
}
