using System.Threading.Tasks;

namespace DispatcherWeb.Imports.Services
{
    public interface IUpdateTruckFromImportAppService
    {
        Task UpdateMileageAndHoursAsync(
            int tenantId,
            long userId
        );
    }
}
