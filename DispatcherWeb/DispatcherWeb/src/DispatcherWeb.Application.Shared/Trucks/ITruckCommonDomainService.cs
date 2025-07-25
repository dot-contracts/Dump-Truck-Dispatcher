using System.Threading.Tasks;
using Abp.Domain.Services;
using DispatcherWeb.Trucks.Dto;

namespace DispatcherWeb.Trucks
{
    public interface ITruckCommonDomainService : IDomainService
    {
        Task UpdateMaxNumberOfTrucksFeatureAndNotifyAdmins(UpdateMaxNumberOfTrucksFeatureAndNotifyAdminsInput input);
    }
}
