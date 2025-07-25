using System.Linq;
using System.Threading.Tasks;
using Abp.Timing;
using DispatcherWeb.DriverAssignments;
using DispatcherWeb.Drivers;
using Xunit;

namespace DispatcherWeb.Tests.DriverAssignments
{
    public class DriverAssignmentAppService_Tests_Base : AppTestBase, IAsyncLifetime
    {
        protected int _officeId;
        protected IDriverAssignmentAppService _driverAssignmentAppService;

        public async Task InitializeAsync()
        {
            var office = await CreateOfficeAndAssignUserToIt();
            _officeId = office.Id;
            _driverAssignmentAppService = Resolve<IDriverAssignmentAppService>();
        }

        public Task DisposeAsync()
        {
            return Task.CompletedTask;
        }

        protected async Task<DriverAssignment> CreateTestDriverAssignment()
        {
            var date = Clock.Now.Date;
            var order = await CreateOrderWithOrderLines(date, Shift.Shift1);
            var orderLine1 = order.OrderLines.First();
            var truck = await CreateTruck();
            var driver = await CreateDriver(officeId: _officeId);
            await CreateOrderLineTruck(truck.Id, driver.Id, orderLine1.Id, 1);
            var driverAssignment = await CreateDriverAssignmentForTruck(_officeId, truck.Id, date, Shift.Shift1, driver.Id);
            return driverAssignment;
        }
    }
}
