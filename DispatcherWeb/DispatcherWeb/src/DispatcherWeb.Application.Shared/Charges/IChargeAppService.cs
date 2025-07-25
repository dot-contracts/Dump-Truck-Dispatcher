using System.Collections.Generic;
using System.Threading.Tasks;
using Abp.Application.Services;
using Abp.Application.Services.Dto;
using DispatcherWeb.Charges.Dto;

namespace DispatcherWeb.Charges
{
    public interface IChargeAppService : IApplicationService
    {
        Task<List<ChargeEditDto>> GetChargesForOrderLine(GetChargesForOrderLineInput input);
        Task<ChargeEditDto> EditCharge(ChargeEditDto model);
        Task DeleteCharge(EntityDto model);
        Task<ChargeOrderLineDetailsDto> GetChargeOrderLineDetails(GetChargeOrderLineDetailsInput input);
    }
}
