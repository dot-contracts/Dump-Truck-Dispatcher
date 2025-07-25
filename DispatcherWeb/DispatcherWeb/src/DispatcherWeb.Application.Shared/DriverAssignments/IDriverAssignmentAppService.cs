using System.Threading.Tasks;
using Abp.Application.Services;
using Abp.Application.Services.Dto;
using DispatcherWeb.DriverAssignments.Dto;

namespace DispatcherWeb.DriverAssignments
{
    public interface IDriverAssignmentAppService : IApplicationService
    {
        Task<ListResultDto<DriverAssignmentDto>> GetDriverAssignments(GetDriverAssignmentsInput input);
        Task SetNoDriverForTruck(SetNoDriverForTruckInput input);
        Task<EditDriverAssignmentResult> EditDriverAssignment(DriverAssignmentEditDto input);
        Task SetDefaultDriverForTruck(SetDefaultDriverForTruckInput input);
        Task<byte[]> GetDriverAssignmentReport(GetDriverAssignmentsInput input);
        Task SetDriverForTruck(SetDriverForTruckInput input);
        Task<ThereAreOpenDispatchesForTruckOnDateResult> ThereAreOpenDispatchesForTruckOnDate(ThereAreOpenDispatchesForTruckOnDateInput input);
        Task<int> AddUnscheduledTrucks(AddUnscheduledTrucksInput input);
        Task AddDefaultStartTime(AddDefaultStartTimeInput input);
        Task<HasOrderLineTrucksResult> HasOrderLineTrucks(HasOrderLineTrucksInput input);
    }
}
