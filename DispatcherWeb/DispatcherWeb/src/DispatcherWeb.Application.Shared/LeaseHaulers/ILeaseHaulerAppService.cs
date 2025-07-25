using System.Threading.Tasks;
using Abp.Application.Services;
using Abp.Application.Services.Dto;
using DispatcherWeb.Dto;
using DispatcherWeb.LeaseHaulers.Dto;

namespace DispatcherWeb.LeaseHaulers
{
    public interface ILeaseHaulerAppService : IApplicationService
    {
        Task<PagedResultDto<LeaseHaulerDto>> GetLeaseHaulers(GetLeaseHaulersInput input);
        Task<PagedResultDto<SelectListDto>> GetLeaseHaulersSelectList(GetLeaseHaulersSelectListInput input);
        Task<LeaseHaulerEditDto> GetLeaseHaulerForEdit(NullableIdDto input);
        Task<int> EditLeaseHauler(LeaseHaulerEditDto model);

        Task<PagedResultDto<LeaseHaulerContactDto>> GetLeaseHaulerContacts(GetLeaseHaulerContactsInput input);
        Task<LeaseHaulerContactEditDto> GetLeaseHaulerContactForEdit(NullableIdDto input);
        Task<LeaseHaulerTruckEditDto> GetLeaseHaulerTruckForEdit(GetLeaseHaulerTruckForEditInput input);
        Task<LeaseHaulerDriverEditDto> GetLeaseHaulerDriverForEdit(NullableIdDto input);
        Task<string> GetLeaseHaulerCompanyName();
        Task<int> EditLeaseHaulerContact(LeaseHaulerContactEditDto model);
        Task<EditLeaseHaulerTruckResult> EditLeaseHaulerTruck(LeaseHaulerTruckEditDto model);
        Task EditLeaseHaulerDriver(LeaseHaulerDriverEditDto model);
        Task DeleteLeaseHaulerContact(EntityDto input);
        Task DeleteLeaseHaulerTruck(EntityDto input);
        Task DeleteLeaseHaulerDriver(EntityDto input);
        Task<ListResultDto<LeaseHaulerContactSelectListDto>> GetLeaseHaulerContactSelectList(int leaseHaulerId, int? leaseHaulerContactId, LeaseHaulerMessageType messageType);
        Task DeleteLeaseHauler(EntityDto input);
    }
}
