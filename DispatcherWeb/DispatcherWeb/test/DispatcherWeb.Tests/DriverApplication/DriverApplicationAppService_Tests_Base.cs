using System;
using System.Linq;
using System.Threading.Tasks;
using Abp.Application.Services;
using Abp.Runtime.Session;
using DispatcherWeb.Authorization.Users;
using DispatcherWeb.DriverApplication;
using DispatcherWeb.Drivers;
using DispatcherWeb.Orders;
using DispatcherWeb.Trucks;
using NSubstitute;
using Xunit;

namespace DispatcherWeb.Tests.DriverApplication
{
    public class DriverApplicationAppService_Tests_Base : AppTestBase, IAsyncLifetime
    {
        protected int _officeId;
        protected IDriverApplicationAppService _driverApplicationAppService;
        protected Driver _driver;
        protected User _driverUser;

        public async Task InitializeAsync()
        {
            var office = await CreateOfficeAndAssignUserToIt();
            _officeId = office.Id;
            _driverApplicationAppService = Resolve<IDriverApplicationAppService>();
            _driver = await CreateDriver();
            _driverUser = await CreateUser(1);
            await UpdateEntity(_driver, d => d.UserId = _driverUser.Id);

            var abpSession = Substitute.For<IAbpSession>();
            abpSession.UserId.Returns(_driverUser.Id);
            abpSession.GetTenantIdOrNullAsync().Returns(1);
            ((ApplicationService)_driverApplicationAppService).AbpSession = abpSession;
        }

        public Task DisposeAsync()
        {
            return Task.CompletedTask;
        }

        protected async Task<Order> CreateOrderAndAssociateWithDriver(DateTime date, Shift? shift, DateTime startTime, Truck truck, Driver driver)
        {
            var order = await CreateOrderWithOrderLines(date, shift);
            var orderLine1 = order.OrderLines.First();
            //await UpdateEntity(order, o => o.StartTime = startTime);
            var driverAssignment = await CreateDriverAssignmentForTruck(_officeId, truck.Id, date, shift, driver.Id);
            await UpdateEntity(driverAssignment, da => da.StartTime = startTime);
            await CreateOrderLineTruck(truck.Id, driver.Id, orderLine1.Id, 1);
            return order;
        }

        protected async Task<DispatcherWeb.Drivers.EmployeeTime> CreateEmployeeTime(DateTime startDateTime, DateTime? endDateTime, int truckId)
        {
            return await base.CreateEmployeeTime(_driverUser.Id, startDateTime, endDateTime, truckId);
        }
    }
}
