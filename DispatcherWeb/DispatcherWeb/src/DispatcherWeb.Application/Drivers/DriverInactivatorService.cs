using System.Linq;
using System.Threading.Tasks;
using Abp.Domain.Repositories;
using Abp.UI;
using DispatcherWeb.Dispatching;
using DispatcherWeb.Trucks;
using Microsoft.EntityFrameworkCore;

namespace DispatcherWeb.Drivers
{
    public class DriverInactivatorService : DispatcherWebDomainServiceBase, IDriverInactivatorService
    {
        private readonly IRepository<Driver> _driverRepository;
        private readonly IRepository<Truck> _truckRepository;
        private readonly IRepository<DriverAssignment> _driverAssignmentRepository;
        private readonly ITruckAppService _truckAppService;

        public DriverInactivatorService(
            IRepository<Driver> driverRepository,
            IRepository<Truck> truckRepository,
            IRepository<DriverAssignment> driverAssignmentRepository,
            ITruckAppService truckAppService
            )
        {
            _driverRepository = driverRepository;
            _truckRepository = truckRepository;
            _driverAssignmentRepository = driverAssignmentRepository;
            _truckAppService = truckAppService;
        }

        public async Task InactivateDriverAsync(Driver driver, int? leaseHaulerId)
        {
            if (driver.Id == 0)
            {
                return;
            }

            driver.IsInactive = true;
            if (leaseHaulerId.HasValue)
            {
                driver.TerminationDate = await GetToday();
            }
            else
            {
                await EnsureCanInactivateDriver(driver);
                await RemoveDriverAsDefaultDriver(driver.Id);
                await SetInactiveDriverToNullInDriverAssignments(driver);
            }

        }

        private async Task EnsureCanInactivateDriver(Driver driver)
        {
            if (await (await _driverRepository.GetQueryAsync())
                .AnyAsync(d => d.Id == driver.Id
                    && d.Dispatches.Any(di => Dispatch.OpenStatuses.Contains(di.Status)))
            )
            {
                throw new UserFriendlyException($"Driver {driver.FirstName} {driver.LastName} has open dispatches and can't be inactivated until the dispatches are either completed or canceled.");
            }
        }

        private async Task RemoveDriverAsDefaultDriver(int driverId)
        {
            var associatedTrucks = await (await _truckRepository.GetQueryAsync())
                .Where(t =>
                    t.OfficeId != null
                    && t.LeaseHaulerTruck.AlwaysShowOnSchedule != true
                    && t.DefaultDriverId == driverId)
                .ToListAsync();
            foreach (var truck in associatedTrucks)
            {
                //await _truckAppService.RemoveDefaultDriver(truck.Id, driver.Id);
                truck.DefaultDriverId = null;
            }
        }

        private async Task SetInactiveDriverToNullInDriverAssignments(Driver driver)
        {
            var today = await GetToday();
            var driverAssignmentsToNullQuery = (await _driverAssignmentRepository.GetQueryAsync())
                .Where(da => da.DriverId == driver.Id && da.Date >= today);

            var officeId = OfficeId;
            var otherOfficeDriverAssignments = await driverAssignmentsToNullQuery
                .Where(x => x.OfficeId != officeId)
                .Select(x => new
                {
                    x.Date,
                    x.Truck.TruckCode,
                    OfficeName = x.Office.Name,
                }).ToListAsync();
            if (otherOfficeDriverAssignments.Any())
            {
                var da = otherOfficeDriverAssignments.First();
                throw new UserFriendlyException($"Driver {driver.FirstName} {driver.LastName} cannot be inactivated because it is already assigned to truck {da.TruckCode} for office {da.OfficeName} on {da.Date:d}.");
            }

            var driverAssignments = await driverAssignmentsToNullQuery.ToListAsync();
            foreach (var driverAssignment in driverAssignments)
            {
                driverAssignment.DriverId = null;
            }
            await CurrentUnitOfWork.SaveChangesAsync();

            foreach (var driverAssignment in driverAssignments)
            {
                await _truckAppService.CompleteTodayAndRemoveFutureOrderLineTrucksWithoutDriverAssignmentForTruck(driverAssignment.TruckId, driver.Id, today);
            }
        }
    }
}
