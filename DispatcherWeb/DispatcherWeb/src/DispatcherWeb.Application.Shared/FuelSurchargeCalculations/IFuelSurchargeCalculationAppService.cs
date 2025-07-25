using System.Collections.Generic;
using System.Threading.Tasks;
using Abp.Application.Services;
using Abp.Application.Services.Dto;
using DispatcherWeb.FuelSurchargeCalculations.Dto;

namespace DispatcherWeb.FuelSurchargeCalculations
{
    public interface IFuelSurchargeCalculationAppService : IApplicationService
    {
        Task DeleteFuelSurchargeCalculation(EntityDto model);
        Task<FuelSurchargeCalculationEditDto> EditFuelSurchargeCalculation(FuelSurchargeCalculationEditDto model);
        Task<List<FuelSurchargeCalculationEditDto>> GetFuelSurchargeCalculations();
    }
}
