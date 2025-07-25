using System.Linq;
using System.Threading.Tasks;
using Abp.Domain.Repositories;
using Abp.UI;
using DispatcherWeb.Drivers;
using DispatcherWeb.Dto;
using Microsoft.EntityFrameworkCore;

namespace DispatcherWeb.Infrastructure.Extensions
{
    public static class DriverExtensions
    {
        public static IQueryable<SelectListDto> SelectIdName(this IQueryable<Driver> driversQuery)
        {
            return driversQuery.Select(x => new SelectListDto
            {
                Id = x.Id.ToString(),
                Name = x.FirstName + " " + x.LastName,
            });
        }

        public static async Task<int> GetDriverIdByUserIdOrThrow(this IRepository<Driver> driverRepository, long userId)
        {
            int driverId = await driverRepository.GetDriverIdByUserIdOrDefault(userId);
            if (driverId == 0)
            {
                throw new UserFriendlyException("The current user isn't a driver!");
            }

            return driverId;
        }

        public static async Task<int> GetDriverIdByUserIdOrDefault(this IRepository<Driver> driverRepository, long userId)
        {
            return await (await driverRepository.GetQueryAsync())
                .Where(d => d.UserId == userId)
                .OrderByDescending(d => !d.IsInactive)
                .Select(d => d.Id)
                .FirstOrDefaultAsync();
        }
    }
}
