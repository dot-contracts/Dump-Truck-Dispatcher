using System;
using System.Linq;
using System.Threading.Tasks;
using Abp.Domain.Repositories;
using Abp.Linq.Extensions;
using DispatcherWeb.Drivers;

namespace DispatcherWeb.Infrastructure.RepositoryExtensions
{
    public static class DriverAssignmentRepositoryExtensions
    {
        public static async Task<IQueryable<DriverAssignment>> GetQueryAsync(
            this IRepository<DriverAssignment> driverAssignmentRepository,
            DateTime date,
            Shift? shift,
            int? officeId
        )
        {
            return (await driverAssignmentRepository.GetQueryAsync())
                .WhereIf(officeId.HasValue, da => da.OfficeId == officeId)
                .Where(da => da.Date == date && da.Shift == shift);
        }
    }
}
