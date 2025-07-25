using System.Threading.Tasks;
using DispatcherWeb.Distance.Dto;

namespace DispatcherWeb.Distance
{
    public interface IDistanceCalculator
    {
        Task PopulateDistancesAsync(PopulateDistancesInput input);
    }
}
