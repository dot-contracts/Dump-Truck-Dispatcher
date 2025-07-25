using System.Collections.Generic;
using System.Threading.Tasks;
using Abp.Application.Services;
using Abp.Application.Services.Dto;
using DispatcherWeb.Dto;
using DispatcherWeb.Insurances.Dto;

namespace DispatcherWeb.Insurances
{
    public interface IInsuranceAppService : IApplicationService
    {
        Task<List<InsuranceTypeDto>> GetInsuranceTypes();
        Task<PagedResultDto<SelectListDto>> GetInsuranceTypesSelectList(GetSelectListInput input);
        Task<List<InsuranceEditDto>> GetInsurances(int leaseHaulerId);
        Task<InsuranceEditDto> EditInsurance(InsuranceEditDto model);
        Task DeleteInsurance(EntityDto model);
        Task<AddInsurancePhotoResult> AddInsurancePhoto(AddInsurancePhotoInput input);
        Task DeleteInsurancePhoto(DeleteInsurancePhotoInput input);
        Task<InsurancePhotoDto> GetInsurancePhoto(int insuranceId);
    }
}
