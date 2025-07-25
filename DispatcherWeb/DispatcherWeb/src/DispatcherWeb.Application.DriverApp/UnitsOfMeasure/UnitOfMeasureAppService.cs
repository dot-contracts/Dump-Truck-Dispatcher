using Abp.Application.Services.Dto;
using Abp.Authorization;
using Abp.Runtime.Session;
using DispatcherWeb.Authorization;
using DispatcherWeb.Caching;
using DispatcherWeb.DriverApp.UnitsOfWork.Dto;
using DispatcherWeb.UnitOfMeasures;

namespace DispatcherWeb.DriverApp.UnitsOfWork
{
    [AbpAuthorize(AppPermissions.Pages_DriverApplication_ReactNativeDriverApp)]
    public class UnitOfMeasureAppService : DispatcherWebDriverAppAppServiceBase, IUnitOfMeasureAppService
    {
        private readonly IUnitOfMeasureListCache _uomListCache;

        public UnitOfMeasureAppService(
            IUnitOfMeasureListCache uomListCache
            )
        {
            _uomListCache = uomListCache;
        }

        public async Task<IListResult<UnitOfMeasureDto>> Get()
        {
            var uoms = await _uomListCache.GetList(new ListCacheTenantKey(await Session.GetTenantIdAsync()));
            var result = uoms.Items
                .Select(x => new UnitOfMeasureDto
                {
                    Id = x.Id,
                    Name = x.Name,
                })
                .OrderBy(x => x.Id)
                .ToList();

            return new ListResultDto<UnitOfMeasureDto>(result);
        }
    }
}
