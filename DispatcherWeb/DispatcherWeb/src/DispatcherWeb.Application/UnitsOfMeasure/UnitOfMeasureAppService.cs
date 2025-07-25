using System.Linq;
using System.Threading.Tasks;
using Abp.Application.Services.Dto;
using Abp.Authorization;
using Abp.Domain.Repositories;
using Abp.Linq.Extensions;
using DispatcherWeb.Dto;
using DispatcherWeb.UnitsOfMeasure.Dto;
using Microsoft.AspNetCore.Mvc;

namespace DispatcherWeb.UnitsOfMeasure
{
    [AbpAuthorize]
    public class UnitOfMeasureAppService : DispatcherWebAppServiceBase, IUnitOfMeasureAppService
    {
        private readonly IRepository<UnitOfMeasure> _unitOfMeasureRepository;

        public UnitOfMeasureAppService(
            IRepository<UnitOfMeasure> unitOfMeasureRepository
            )
        {
            _unitOfMeasureRepository = unitOfMeasureRepository;
        }

        [HttpPost]
        public async Task<PagedResultDto<SelectListDto>> GetUnitsOfMeasureSelectList(GetUnitsOfMeasureSelectListInput input)
        {
            var query = (await _unitOfMeasureRepository.GetQueryAsync())
                .WhereIf(input.GetUomBaseId, x => x.UnitOfMeasureBase != null)
                .WhereIf(input.UomBaseIds?.Any() == true, x => x.UnitOfMeasureBaseId.HasValue && input.UomBaseIds.Contains((UnitOfMeasureBaseEnum)x.UnitOfMeasureBaseId))
                .Select(x => new SelectListDto<UnitOfMeasureSelectListInfoDto>
                {
                    Id = input.GetUomBaseId ? x.UnitOfMeasureBaseId.ToString() : x.Id.ToString(),
                    Name = x.Name,
                    Item = new UnitOfMeasureSelectListInfoDto
                    {
                        UomBaseId = x.UnitOfMeasureBaseId,
                    },
                });

            return await query.GetSelectListResult(input);
        }
    }
}
