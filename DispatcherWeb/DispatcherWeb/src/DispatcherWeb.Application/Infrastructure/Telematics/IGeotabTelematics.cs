using System.Threading.Tasks;

namespace DispatcherWeb.Infrastructure.Telematics
{
    public interface IGeotabTelematics : ITruckTelematicsService
    {
        Task<string[]> GetDeviceIdsByTruckCodesAsync(string[] truckCodes);
        Task CheckCredentialsAsync();
    }
}
