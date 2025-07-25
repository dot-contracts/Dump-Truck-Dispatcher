using System.Threading.Tasks;
using Abp.Application.Services.Dto;
using DispatcherWeb.FuelPurchases.Dto;

namespace DispatcherWeb.FuelPurchases
{
    public interface IFuelPurchaseAppService
    {
        Task<PagedResultDto<FuelPurchaseDto>> GetFuelPurchasePagedList(GetFuelPurchasePagedListInput input);
        Task<FuelPurchaseEditDto> SaveFuelPurchase(FuelPurchaseEditDto model);
        Task<FuelPurchaseEditDto> GetFuelPurchaseForEdit(NullableIdDto input);
        Task DeleteFuelPurchase(EntityDto input);
    }
}