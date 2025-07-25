using System.Threading.Tasks;
using Abp.Application.Services.Dto;
using DispatcherWeb.TaxRates.Dto;

namespace DispatcherWeb.TaxRates
{
    public interface ITaxRateAppService
    {
        Task<TaxRateEditDto> GetTaxRateForEdit(NullableIdDto nullableIdDto);
        Task<PagedResultDto<TaxRateDto>> GetTaxRates(GetTaxRatesInput input);

    }
}
