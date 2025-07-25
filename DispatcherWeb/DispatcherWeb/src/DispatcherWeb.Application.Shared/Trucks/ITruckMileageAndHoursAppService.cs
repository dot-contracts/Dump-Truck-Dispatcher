using System.Threading.Tasks;

namespace DispatcherWeb.Trucks
{
    public interface ITruckMileageAndHoursAppService
    {
        Task AddTrucksMileageAndHourForDayBeforeTickets();
    }
}