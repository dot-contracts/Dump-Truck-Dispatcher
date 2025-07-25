using System;
using System.Threading.Tasks;
using Abp.Application.Services;
using Abp.Application.Services.Dto;
using DispatcherWeb.Dto;
using DispatcherWeb.Trucks.Dto;

namespace DispatcherWeb.Trucks
{
    public interface ITruckAppService : IApplicationService
    {
        Task<PagedResultDto<TruckDto>> GetTrucks(GetTrucksInput input);
        Task<PagedResultDto<SelectListDto>> GetTrucksSelectList(GetTrucksSelectListInput input);
        Task<TruckEditDto> GetTruckForEdit(GetTruckForEditInput input);
        Task<EditTruckResult> EditTruck(TruckEditDto model);
        Task<SetTruckIsOutOfServiceResult> SetTruckIsOutOfService(SetTruckIsOutOfServiceInput input);
        Task<bool> CanDeleteTruck(EntityDto input);
        Task DeleteTruck(EntityDto input);

        Task<TruckFileEditDto> SaveFile(TruckFileEditDto truckFileEditDto);
        Task<TruckFileEditDto> GetTruckFileEditDto(int id);
        Task RemoveDefaultDriver(int truckId, int driverId);
        Task RemoveTodayAndFutureDriverAssignmentsForTruckAndDriver(int truckId, int driverId, DateTime today);
        Task CompleteTodayAndRemoveFutureOrderLineTrucksWithoutDriverAssignmentForTruck(int truckId, int driverId, DateTime today);
        Task UpdateMaxNumberOfTrucksFeatureAndNotifyAdmins(UpdateMaxNumberOfTrucksFeatureAndNotifyAdminsInput input);
        Task<string> GetTruckCodeByTruckId(int truckId);
        Task<string> GetTractorWithCurrentTrailer(TrailerIsSetAsCurrentTrailerForAnotherTractorInput input);
    }
}
