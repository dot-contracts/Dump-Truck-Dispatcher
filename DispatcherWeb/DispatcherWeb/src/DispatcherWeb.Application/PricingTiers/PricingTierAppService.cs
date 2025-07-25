using System.Linq;
using System.Linq.Dynamic.Core;
using System.Threading.Tasks;
using Abp.Application.Services.Dto;
using Abp.Authorization;
using Abp.Domain.Repositories;
using Abp.Linq.Extensions;
using Abp.Runtime.Session;
using Abp.UI;
using DispatcherWeb.Authorization;
using DispatcherWeb.Dto;
using DispatcherWeb.Items;
using DispatcherWeb.PricingTiers.Dto;
using Microsoft.EntityFrameworkCore;

namespace DispatcherWeb.PricingTiers;

[AbpAuthorize]
public class PricingTierAppService : DispatcherWebAppServiceBase, IPricingTierAppService
{
    private readonly IRepository<PricingTier> _pricingTierRepository;

    public PricingTierAppService(IRepository<PricingTier> pricingTierRepository)
    {
        _pricingTierRepository = pricingTierRepository;
    }

    [AbpAuthorize(AppPermissions.Pages_Items_PricingTiers)]
    public async Task<PagedResultDto<PricingTierDto>> GetPricingTiers(GetPricingTiersInput input)
    {
        var query = await _pricingTierRepository.GetQueryAsync();
        var totalCount = await query.CountAsync();

        var items = await query
            .Select(x => new PricingTierDto
            {
                Id = x.Id,
                Name = x.Name,
            })
            .OrderBy(input.Sorting)
            .PageBy(input)
            .ToListAsync();

        return new PagedResultDto<PricingTierDto>(
            totalCount,
            items);
    }

    [AbpAuthorize(AppPermissions.Pages_Items_PricingTiers)]
    public async Task DeletePricingTier(EntityDto input)
    {
        var pricingTier = await (await _pricingTierRepository.GetQueryAsync())
            .Where(x => x.Id == input.Id)
            .Select(x => new
            {
                HasCustomers = x.Customers.Any(),
            }).FirstAsync();

        if (pricingTier.HasCustomers)
        {
            throw new UserFriendlyException(L("UnableToDeleteSelectedRowWithAssociatedData"));
        }

        await _pricingTierRepository.DeleteAsync(input.Id);
    }

    [AbpAuthorize(AppPermissions.Pages_Items_PricingTiers)]
    public async Task<PricingTierEditDto> GetPricingTierForEdit(NullableIdDto input)
    {
        PricingTierEditDto pricingTierDto;
        if (input.Id.HasValue)
        {
            pricingTierDto = await (await _pricingTierRepository.GetQueryAsync())
                .Where(x => x.Id == input.Id)
                .Select(x => new PricingTierEditDto
                {
                    Id = x.Id,
                    Name = x.Name,
                    IsDefault = x.IsDefault,
                }).FirstAsync();
        }
        else
        {
            pricingTierDto = new PricingTierEditDto();
        }

        return pricingTierDto;
    }

    [AbpAuthorize(AppPermissions.Pages_Items_PricingTiers)]
    public async Task EditPricingTier(PricingTierEditDto input)
    {
        var entity = input.Id.HasValue ? await _pricingTierRepository.GetAsync(input.Id.Value) : new PricingTier();

        entity.Name = input.Name;
        entity.TenantId = await Session.GetTenantIdAsync();
        if (input.IsDefault)
        {
            await UnsetDefaultPricingTier(entity.Id);
        }
        entity.IsDefault = input.IsDefault;

        await ValidatePricingTier(input);
        await _pricingTierRepository.InsertOrUpdateAndGetIdAsync(entity);
    }

    [AbpAuthorize(AppPermissions.Pages_Misc_SelectLists_PricingTiers)]
    public async Task<PagedResultDto<PricingTierSelectListDto>> GetPricingTiersSelectList(GetSelectListInput input)
    {
        var query = (await _pricingTierRepository.GetQueryAsync())
            .Select(p => new PricingTierSelectListDto
            {
                Name = p.Name,
                Id = p.Id.ToString(),
                IsDefault = p.IsDefault,
            });
        return await query.GetExtendedSelectListResult(input);
    }

    private async Task ValidatePricingTier(PricingTierEditDto input)
    {
        if (await (await _pricingTierRepository.GetQueryAsync())
                .WhereIf(input.Id.HasValue, x => x.Id != input.Id)
                .AnyAsync(x => x.Name == input.Name))
        {
            throw new UserFriendlyException($"Pricing Tier with name '{input.Name}' already exists!");
        }

        if (!input.Id.HasValue && await (await _pricingTierRepository.GetQueryAsync()).CountAsync() >= 5)
        {
            throw new UserFriendlyException(L("MaximumNumberOfPricingTiersError"));
        }
    }

    private async Task UnsetDefaultPricingTier(int newDefaultPricingTierId)
    {
        var pricingTiers = await (await _pricingTierRepository.GetQueryAsync())
            .Where(x => x.Id != newDefaultPricingTierId && x.IsDefault)
            .ToListAsync();

        foreach (var item in pricingTiers)
        {
            item.IsDefault = false;
        }
    }
}
