using System.Linq;
using System.Linq.Dynamic.Core;
using System.Threading.Tasks;
using Abp.Application.Services.Dto;
using Abp.Authorization;
using Abp.Domain.Repositories;
using Abp.Linq.Extensions;
using Abp.UI;
using DispatcherWeb.Authorization;
using DispatcherWeb.Dto;
using DispatcherWeb.Invoices;
using DispatcherWeb.Orders;
using DispatcherWeb.TaxRates.Dto;
using Microsoft.EntityFrameworkCore;

namespace DispatcherWeb.TaxRates
{
    [AbpAuthorize]
    public class TaxRateAppService : DispatcherWebAppServiceBase, ITaxRateAppService
    {
        private readonly IRepository<TaxRate> _taxRateRepository;
        private readonly IRepository<Order> _orderRepository;
        private readonly IRepository<Invoice> _invoiceRepository;

        public TaxRateAppService(
            IRepository<TaxRate> taxRateRepository,
            IRepository<Order> orderRepository,
            IRepository<Invoice> invoiceRepository
            )
        {
            _taxRateRepository = taxRateRepository;
            _orderRepository = orderRepository;
            _invoiceRepository = invoiceRepository;
        }

        [AbpAuthorize(AppPermissions.Pages_Items_TaxRates_Edit)]
        public async Task<PagedResultDto<TaxRateDto>> GetTaxRates(GetTaxRatesInput input)
        {
            var query = await _taxRateRepository.GetQueryAsync();

            var totalCount = await query.CountAsync();

            var items = await query
                .Select(x => new TaxRateDto
                {
                    Id = x.Id,
                    Name = x.Name,
                    Rate = x.Rate,
                })
                .OrderBy(input.Sorting)
                .PageBy(input)
                .ToListAsync();

            return new PagedResultDto<TaxRateDto>(
                totalCount,
                items);
        }

        [AbpAuthorize(AppPermissions.Pages_Items_TaxRates_Edit)]
        public async Task<TaxRateEditDto> GetTaxRateForEdit(NullableIdDto input)
        {
            TaxRateEditDto taxRateEditDto;

            if (input.Id.HasValue)
            {
                taxRateEditDto = await (await _taxRateRepository.GetQueryAsync())
                    .Where(x => x.Id == input.Id.Value)
                    .Select(x => new TaxRateEditDto
                    {
                        Id = x.Id,
                        Name = x.Name,
                        Rate = x.Rate,
                    }).FirstAsync();
            }
            else
            {
                taxRateEditDto = new TaxRateEditDto();
            }
            return taxRateEditDto;
        }

        [AbpAuthorize(AppPermissions.Pages_Items_TaxRates_Edit)]
        public async Task EditTaxRate(TaxRateEditDto model)
        {
            var entity = model.Id.HasValue ? await _taxRateRepository.GetAsync(model.Id.Value) : new TaxRate();
            entity.Name = model.Name;
            entity.Rate = model.Rate.Value;

            if (await (await _taxRateRepository.GetQueryAsync())
                .WhereIf(model.Id != 0, x => x.Id != model.Id)
                .AnyAsync(x => x.Name == model.Name)
            )
            {
                throw new UserFriendlyException($"TaxRate with name '{model.Name}' already exists!");
            }

            await _taxRateRepository.InsertOrUpdateAndGetIdAsync(entity);
        }

        [AbpAuthorize(AppPermissions.Pages_Items_TaxRates_Edit)]
        public async Task DeleteTaxRate(EntityDto input)
        {
            var hasOrderSalesTaxEntityRecords = await (await _orderRepository.GetQueryAsync())
                .Where(x => x.SalesTaxEntityId == input.Id)
                .AnyAsync();

            var hasInvoiceSalesTaxEntityRecords = await (await _invoiceRepository.GetQueryAsync())
                .Where(x => x.SalesTaxEntityId == input.Id)
                .AnyAsync();

            if (hasOrderSalesTaxEntityRecords || hasInvoiceSalesTaxEntityRecords)
            {
                throw new UserFriendlyException("You can't delete selected row because it is associated with any order or invoice.");
            }

            await _taxRateRepository.DeleteAsync(input.Id);
        }

        public async Task<PagedResultDto<SelectListDto>> GetTaxRatesSelectList(GetSelectListInput input)
        {
            var query = (await _taxRateRepository.GetQueryAsync())
                .Select(x => new SelectListDto<TaxRateSelectListInfo>
                {
                    Id = x.Id.ToString(),
                    Name = x.Name,
                    Item = new TaxRateSelectListInfo
                    {
                        Rate = x.Rate,
                    },
                });

            return await query.GetSelectListResult(input);
        }
    }
}
