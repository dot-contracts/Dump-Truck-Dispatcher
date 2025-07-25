using System.Linq;
using System.Linq.Dynamic.Core;
using System.Threading.Tasks;
using Abp.Application.Services;
using Abp.Application.Services.Dto;
using Abp.Authorization;
using Abp.Domain.Repositories;
using Abp.Extensions;
using Abp.Linq.Extensions;
using Abp.UI;
using DispatcherWeb;
using DispatcherWeb.Authorization;
using DispatcherWeb.Dto;
using DispatcherWeb.HaulZones;
using DispatcherWeb.HaulZones.Dto;
using DispatcherWeb.Items;
using Microsoft.EntityFrameworkCore;

[AbpAuthorize]
public class HaulZoneAppService : ApplicationService, IHaulZoneAppService
{
    private readonly IRepository<HaulZone> _haulZoneRepository;
    private readonly IRepository<Item> _itemRepository;

    public HaulZoneAppService(
        IRepository<HaulZone> haulZoneRepository,
        IRepository<Item> itemRepository
    )
    {
        _haulZoneRepository = haulZoneRepository;
        _itemRepository = itemRepository;
    }

    [AbpAuthorize(AppPermissions.Pages_Items_HaulZones)]
    public async Task<PagedResultDto<HaulZoneDto>> GetHaulZones(GetHaulZonesInput input)
    {
        var query = (await _haulZoneRepository.GetQueryAsync())
            .WhereIf(!input.Name.IsNullOrWhiteSpace(), x => x.Name.Contains(input.Name))
            .WhereIf(input.Status == FilterActiveStatus.Active, x => x.IsActive)
            .WhereIf(input.Status == FilterActiveStatus.Inactive, x => !x.IsActive);

        var totalCount = await query.CountAsync();
        var items = await query
            .Select(item => new HaulZoneDto
            {
                Id = item.Id,
                Name = item.Name,
                UnitOfMeasureName = item.UnitOfMeasure.Name,
                Quantity = item.Quantity,
                BillRatePerTon = item.BillRatePerTon,
                MinPerLoad = item.MinPerLoad,
                PayRatePerTon = item.PayRatePerTon,
                IsActive = item.IsActive,
            })
            .OrderBy(input.Sorting)
            .PageBy(input)
            .ToListAsync();

        return new PagedResultDto<HaulZoneDto>(
            totalCount,
            items
        );
    }

    [AbpAuthorize(AppPermissions.Pages_Items_HaulZones)]
    public async Task<HaulZoneEditDto> GetHaulZoneForEdit(NullableIdNameDto input)
    {
        HaulZoneEditDto haulZoneEditDto;

        if (input.Id.HasValue)
        {
            haulZoneEditDto = await (await _haulZoneRepository.GetQueryAsync())
                .Where(x => x.Id == input.Id.Value)
                .Select(haulZone => new HaulZoneEditDto
                {
                    Id = haulZone.Id,
                    Name = haulZone.Name,
                    UnitOfMeasureId = haulZone.UnitOfMeasureId,
                    UnitOfMeasureName = haulZone.UnitOfMeasure.Name,
                    Quantity = haulZone.Quantity,
                    BillRatePerTon = haulZone.BillRatePerTon,
                    MinPerLoad = haulZone.MinPerLoad,
                    PayRatePerTon = haulZone.PayRatePerTon,
                    IsActive = haulZone.IsActive,
                })
                .FirstOrDefaultAsync();

            if (haulZoneEditDto == null)
            {
                throw new UserFriendlyException("HaulZone not found.");
            }
        }
        else
        {
            haulZoneEditDto = new HaulZoneEditDto
            {
                Name = input.Name,
                IsActive = true,
            };
        }

        return haulZoneEditDto;
    }

    [AbpAuthorize(AppPermissions.Pages_Items_HaulZones)]
    public async Task EditHaulZone(HaulZoneEditDto model)
    {
        var haulZone = model.Id.HasValue ? await _haulZoneRepository.GetAsync(model.Id.Value) : new HaulZone();

        haulZone.Name = model.Name;
        haulZone.UnitOfMeasureId = model.UnitOfMeasureId;
        haulZone.Quantity = model.Quantity;
        haulZone.BillRatePerTon = model.BillRatePerTon;
        haulZone.MinPerLoad = model.MinPerLoad;
        haulZone.PayRatePerTon = model.PayRatePerTon;
        haulZone.IsActive = model.IsActive;

        if (!model.Id.HasValue)
        {
            await _haulZoneRepository.InsertAsync(haulZone);
        }
    }

    [AbpAuthorize(AppPermissions.Pages_Items_HaulZones)]
    public async Task DeleteHaulZone(EntityDto input)
    {
        await _haulZoneRepository.DeleteAsync(input.Id);
    }
}
