using System;
using System.Threading.Tasks;
using Abp.Domain.Repositories;
using DispatcherWeb.VehicleMaintenance;

namespace DispatcherWeb.Infrastructure.Extensions
{
    public static class OutOfServiceHistoryRepositoryExtensions
    {
        public static async Task SetInServiceDate(
            this IRepository<OutOfServiceHistory> outOfServiceHistoryRepository,
            int truckId,
            DateTime inServiceDate
        )
        {
            var outOfServiceHistory = await outOfServiceHistoryRepository
                .FirstOrDefaultAsync(x => x.TruckId == truckId && x.InServiceDate == null);
            if (outOfServiceHistory != null)
            {
                outOfServiceHistory.InServiceDate = inServiceDate;
            }

        }
    }
}
