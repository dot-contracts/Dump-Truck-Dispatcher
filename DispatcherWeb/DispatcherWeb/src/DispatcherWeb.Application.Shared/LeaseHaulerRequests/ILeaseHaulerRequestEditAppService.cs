using System;
using System.Threading.Tasks;
using DispatcherWeb.Dto;
using DispatcherWeb.LeaseHaulerRequests.Dto;

namespace DispatcherWeb.LeaseHaulerRequests
{
    public interface ILeaseHaulerRequestEditAppService
    {
        Task<LeaseHaulerRequestEditDto> GetLeaseHaulerRequestForEdit(GetLeaseHaulerRequestForEditInput input);
        Task<LeaseHaulerRequestEditModel> EditLeaseHaulerRequest(LeaseHaulerRequestEditModel model);
        Task UpdateAvailable(IdValueInput<int?> input);
        Task UpdateApproved(IdValueInput<int?> input);
        Task<AvailableTrucksEditDto> GetAvailableTrucksEditDto(Guid leaseHaulerRequestShortGuid);
        Task EditAvailableTrucks(AvailableTrucksEditModel model);
        Task RejectJob(RejectLeaseHaulerRequestDto input);
    }
}