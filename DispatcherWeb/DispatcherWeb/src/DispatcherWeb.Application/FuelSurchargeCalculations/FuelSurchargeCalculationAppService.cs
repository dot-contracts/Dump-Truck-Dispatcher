using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Abp.Application.Services.Dto;
using Abp.Authorization;
using Abp.Configuration;
using Abp.Domain.Repositories;
using Abp.Linq.Extensions;
using Abp.Runtime.Session;
using Abp.UI;
using DispatcherWeb.Authorization;
using DispatcherWeb.Configuration;
using DispatcherWeb.Dto;
using DispatcherWeb.FuelSurchargeCalculations.Dto;
using Microsoft.EntityFrameworkCore;

namespace DispatcherWeb.FuelSurchargeCalculations
{
    [AbpAuthorize]
    public class FuelSurchargeCalculationAppService : DispatcherWebAppServiceBase, IFuelSurchargeCalculationAppService
    {
        private readonly IRepository<FuelSurchargeCalculation> _fuelSurchargeCalculationRepository;
        public FuelSurchargeCalculationAppService(
            IRepository<FuelSurchargeCalculation> fuelSurchargeCalculationRepository
            )
        {
            _fuelSurchargeCalculationRepository = fuelSurchargeCalculationRepository;
        }

        [AbpAuthorize(AppPermissions.Pages_FuelSurchargeCalculations_Edit)]
        public async Task<List<FuelSurchargeCalculationEditDto>> GetFuelSurchargeCalculations()
        {
            var items = await (await _fuelSurchargeCalculationRepository.GetQueryAsync())
                .Select(x => new FuelSurchargeCalculationEditDto
                {
                    Id = x.Id,
                    Name = x.Name,
                    Type = x.Type,
                    BaseFuelCost = x.BaseFuelCost,
                    CanChangeBaseFuelCost = x.CanChangeBaseFuelCost,
                    Increment = x.Increment,
                    FreightRatePercent = x.FreightRatePercent,
                    Credit = x.Credit,
                }).ToListAsync();

            return items;
        }

        [AbpAuthorize(AppPermissions.Pages_FuelSurchargeCalculations_Edit)]
        public async Task<FuelSurchargeCalculationEditDto> EditFuelSurchargeCalculation(FuelSurchargeCalculationEditDto model)
        {
            var entity = model.Id == 0 ? new FuelSurchargeCalculation() : await _fuelSurchargeCalculationRepository.GetAsync(model.Id);

            entity.Name = model.Name;
            entity.Type = model.Type;
            entity.BaseFuelCost = model.BaseFuelCost;
            entity.CanChangeBaseFuelCost = model.CanChangeBaseFuelCost;
            entity.Increment = model.Increment;
            entity.FreightRatePercent = model.FreightRatePercent;
            entity.Credit = model.Credit;

            if (entity.Id == 0)
            {
                model.Id = await _fuelSurchargeCalculationRepository.InsertAndGetIdAsync(entity);
            }

            return model;
        }

        [AbpAuthorize(AppPermissions.Pages_FuelSurchargeCalculations_Edit)]
        public async Task DeleteFuelSurchargeCalculation(EntityDto model)
        {
            if (model.Id == await SettingManager.GetSettingValueAsync<int>(AppSettings.Fuel.DefaultFuelSurchargeCalculationId))
            {
                throw new UserFriendlyException(L("CannotDeleteDefaultFuelSurchargeCalculation"));
            }

            var item = await (await _fuelSurchargeCalculationRepository.GetQueryAsync())
                .Where(x => x.Id == model.Id)
                .Select(x => new
                {
                    HasQuotes = x.Quotes.Any(),
                    HasOrders = x.Orders.Any(),
                }).FirstAsync();

            if (item.HasQuotes || item.HasOrders)
            {
                throw new UserFriendlyException(L("CannotDeleteFuelSurchargeCalculation"));
            }

            await _fuelSurchargeCalculationRepository.DeleteAsync(model.Id);
        }

        [AbpAuthorize(AppPermissions.Pages_FuelSurchargeCalculations_Edit)]
        public async Task SetDefaultFuelSurchargeCalculationId(int? id)
        {
            id ??= 0;
            await SettingManager.ChangeSettingForTenantAsync(await Session.GetTenantIdAsync(), AppSettings.Fuel.DefaultFuelSurchargeCalculationId, id.ToString());
        }

        [AbpAuthorize(AppPermissions.Pages_Misc_SelectLists_FuelSurchargeCalculations)]
        public async Task<PagedResultDto<SelectListDto>> GetFuelSurchargeCalculationsSelectList(GetFuelSurchargeCalculationsSelectListInput input)
        {
            var query = (await _fuelSurchargeCalculationRepository.GetQueryAsync())
                .WhereIf(input.CanChangeBaseFuelCost.HasValue, x => x.CanChangeBaseFuelCost == input.CanChangeBaseFuelCost)
                .Select(x => new SelectListDto<FuelSurchargeCalculationSelectListInfoDto>
                {
                    Id = x.Id.ToString(),
                    Name = x.Name,
                    Item = new FuelSurchargeCalculationSelectListInfoDto
                    {
                        CanChangeBaseFuelCost = x.CanChangeBaseFuelCost,
                        BaseFuelCost = x.BaseFuelCost,
                    },
                });

            var result = await query.GetSelectListResult(input);

            return result;
        }
    }
}
