using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Abp.Domain.Entities.Auditing;
using Abp.Timing;
using AutoFixture;
using DispatcherWeb.Customers;
using DispatcherWeb.DailyHistory;
using DispatcherWeb.MultiTenancy;
using Microsoft.EntityFrameworkCore;
using Shouldly;
using Xunit;

namespace DispatcherWeb.Tests.DailyHistory
{
    public class DailyHistoryAppService_FillTenantDailyHistory_Tests : AppTestBase, IAsyncLifetime
    {
        private IDailyHistoryAppService _dailyHistoryAppService;
        private DateTime _todayUtc;
        private DateTime _yesterdayUtc;
        private int _tenant2Id;

        public async Task InitializeAsync()
        {
            var office = await CreateOfficeAndAssignUserToIt();
            _dailyHistoryAppService = Resolve<IDailyHistoryAppService>();
            _todayUtc = Clock.Now.Date;
            _yesterdayUtc = _todayUtc.AddDays(-1);
            var yesterdayUtc = _yesterdayUtc;
            var tenant2 = await UsingDbContextAsync(async context =>
            {
                var tenant = new Tenant("tenant2", "tenant2");
                await context.Tenants.AddAsync(tenant);
                return tenant;
            });
            _tenant2Id = tenant2.Id;
        }

        [Fact]
        public async Task Test_FillTenantDailyHistory_should_fill_TenantIdAndDate()
        {
            // Act
            await _dailyHistoryAppService.FillTenantDailyHistoryAsync(_todayUtc);

            // Assert
            var tenantDailyHistories = await GetTenantDailyHistories();
            tenantDailyHistories.Count.ShouldBe(2);
            tenantDailyHistories.Count(x => x.Date == _yesterdayUtc).ShouldBe(2);
            tenantDailyHistories.Count(x => x.TenantId == 1).ShouldBe(1);
            tenantDailyHistories.Count(x => x.TenantId == _tenant2Id).ShouldBe(1);
        }

        [Fact]
        public async Task Test_FillTenantDailyHistory_should_fill_zeros_for_empty_tenant()
        {
            // Act
            await _dailyHistoryAppService.FillTenantDailyHistoryAsync(_todayUtc);

            // Assert 
            var tenantDailyHistories = await GetTenantDailyHistories();
            tenantDailyHistories.Count.ShouldBe(2);
            var tenantDailyHistory2 = tenantDailyHistories.First(x => x.TenantId == _tenant2Id);
            tenantDailyHistory2.ActiveTrucks.ShouldBe(0);
            tenantDailyHistory2.ActiveUsers.ShouldBe(0);
            tenantDailyHistory2.UsersWithActivity.ShouldBe(0);
            tenantDailyHistory2.OrderLinesScheduled.ShouldBe(0);
            tenantDailyHistory2.ActiveCustomers.ShouldBe(0);
            tenantDailyHistory2.InternalTrucksScheduled.ShouldBe(0);
            tenantDailyHistory2.InternalScheduledDeliveries.ShouldBe(0);
            tenantDailyHistory2.LeaseHaulerScheduledDeliveries.ShouldBe(0);
        }

        [Fact]
        public async Task Test_FillTenantDailyHistory_should_fill_ActiveTrucks()
        {
            // Arrange
            var dumpTrucks = await CreateVehicleCategory();
            var trailers = await CreateTrailerVehicleCategory();
            var tractors = await CreateTractorVehicleCategory();
            var truck11 = await CreateTruck("11", dumpTrucks);
            await UpdateCreationTime(truck11);
            var truck12 = await CreateTruck("12", trailers);
            await UpdateCreationTime(truck12);
            var truck13 = await CreateTruck("13", tractors, canPullTrailer: true);
            await UpdateCreationTime(truck13);
            var truck14 = await CreateTruck("14", dumpTrucks, alwaysShowOnSchedule: true);
            await UpdateCreationTime(truck14);
            var truck15 = await CreateTruck("15", tractors, canPullTrailer: true, alwaysShowOnSchedule: true);
            await UpdateCreationTime(truck15);

            var truck16 = await CreateTruck("16", dumpTrucks);
            await UpdateCreationTime(truck16);
            await UpdateEntity(truck16, t => t.IsActive = false);

            var truck21 = await CreateTruck(tenantId: _tenant2Id);
            await UpdateCreationTime(truck21);

            // Act
            await _dailyHistoryAppService.FillTenantDailyHistoryAsync(_todayUtc);

            // Assert 
            var tenantDailyHistories = await GetTenantDailyHistories();
            tenantDailyHistories.Count.ShouldBe(2);
            tenantDailyHistories.First(x => x.TenantId == 1).ActiveTrucks.ShouldBe(2);
            tenantDailyHistories.First(x => x.TenantId == _tenant2Id).ActiveTrucks.ShouldBe(1);
        }

        [Fact]
        public async Task Test_FillTenantDailyHistory_should_fill_ActiveUsers()
        {
            // Arrange
            await CreateUser(1);
            await CreateUser(1, false);
            await CreateUser(_tenant2Id);

            // Act
            await _dailyHistoryAppService.FillTenantDailyHistoryAsync(_todayUtc);

            // Assert
            var tenantDailyHistories = await GetTenantDailyHistories();
            tenantDailyHistories.Count.ShouldBe(2);
            tenantDailyHistories.First(x => x.TenantId == 1).ActiveUsers.ShouldBe(1);
            tenantDailyHistories.First(x => x.TenantId == _tenant2Id).ActiveUsers.ShouldBe(1);

        }

        [Fact]
        public async Task Test_FillTenantDailyHistory_should_fill_UsersWithActivity()
        {
            // Arrange
            var user1 = await CreateUser(1);
            await CreateAuditLog(1, user1.Id, _yesterdayUtc);
            var user2 = await CreateUser(1);
            await CreateAuditLog(1, user2.Id, _todayUtc.AddMilliseconds(-1));

            // Act
            await _dailyHistoryAppService.FillTenantDailyHistoryAsync(_todayUtc);

            // Assert
            var tenantDailyHistories = await GetTenantDailyHistories();
            tenantDailyHistories.Count.ShouldBe(2);
            tenantDailyHistories.First(x => x.TenantId == 1).UsersWithActivity.ShouldBe(2);
            tenantDailyHistories.First(x => x.TenantId == _tenant2Id).UsersWithActivity.ShouldBe(0);

        }

        [Fact]
        public async Task Test_FillTenantDailyHistory_should_fill_OrderLinesScheduled()
        {
            // Arrange
            var order = await CreateOrderWithOrderLines(_yesterdayUtc);

            // Act
            await _dailyHistoryAppService.FillTenantDailyHistoryAsync(_todayUtc);

            // Assert
            var tenantDailyHistories = await GetTenantDailyHistories();
            tenantDailyHistories.Count.ShouldBe(2);
            tenantDailyHistories.First(x => x.TenantId == 1).OrderLinesScheduled.ShouldBe(2);
            tenantDailyHistories.First(x => x.TenantId == _tenant2Id).OrderLinesScheduled.ShouldBe(0);
        }

        [Fact]
        public async Task Test_FillTenantDailyHistory_should_fill_ActiveCustomers()
        {
            // Arrange
            await CreateCustomer(1);
            await CreateCustomer(1);
            await CreateCustomer(1, false);

            // Act
            await _dailyHistoryAppService.FillTenantDailyHistoryAsync(_todayUtc);

            // Assert
            var tenantDailyHistories = await GetTenantDailyHistories();
            tenantDailyHistories.Count.ShouldBe(2);
            tenantDailyHistories.First(x => x.TenantId == 1).ActiveCustomers.ShouldBe(2);
            tenantDailyHistories.First(x => x.TenantId == _tenant2Id).ActiveCustomers.ShouldBe(0);

            // Local functions
            async Task CreateCustomer(int tenantId, bool isActive = true)
            {
                var fixture = new Fixture();
                await UsingDbContextAsync(async context =>
                {
                    var customer = fixture.Build<Customer>()
                        .OmitAutoProperties()
                        .With(x => x.TenantId, tenantId)
                        .With(x => x.CreationTime, new DateTime(2019, 1, 1))
                        .With(x => x.Name, fixture.Create<string>())
                        .With(x => x.IsActive, isActive)
                        .Create();
                    await context.Customers.AddAsync(customer);
                });
            }
        }

        [Fact(Skip = "#14648 Historically failing")]
        public async Task Test_FillTenantDailyHistory_should_fill_InternalTrucksScheduled()
        {
            // Arrange
            var order = await CreateOrderWithOrderLines(_yesterdayUtc);
            var orderLine1 = order.OrderLines.ToList()[0];
            var orderLine2 = order.OrderLines.ToList()[1];
            var dumpTrucks = await CreateVehicleCategory();
            var trailers = await CreateTrailerVehicleCategory();
            var tractors = await CreateTractorVehicleCategory();
            var truck1 = await CreateTruck(vehicleCategory: dumpTrucks);
            var driver1 = await CreateDriver();
            await CreateOrderLineTruck(truck1.Id, driver1.Id, orderLine1.Id, .1m); // 1
            await CreateOrderLineTruck(truck1.Id, driver1.Id, orderLine2.Id, .1m);
            var truck2 = await CreateTruck(vehicleCategory: trailers);
            await CreateOrderLineTruck(truck2.Id, null, orderLine1.Id, .1m);
            var truck3 = await CreateTruck(vehicleCategory: tractors, canPullTrailer: true);
            var driver3 = await CreateDriver();
            await CreateOrderLineTruck(truck3.Id, driver3.Id, orderLine1.Id, .1m); // 2
            var truck4 = await CreateTruck(vehicleCategory: dumpTrucks, alwaysShowOnSchedule: true);
            var driver4 = await CreateDriver();
            await CreateOrderLineTruck(truck4.Id, driver4.Id, orderLine1.Id, .1m);
            var truck5 = await CreateTruck(vehicleCategory: tractors, alwaysShowOnSchedule: true, canPullTrailer: true);
            var driver5 = await CreateDriver();
            await CreateOrderLineTruck(truck5.Id, driver5.Id, orderLine1.Id, .1m);

            var truck6 = await CreateTruck(vehicleCategory: dumpTrucks);
            var driver6 = await CreateDriver();
            await CreateOrderLineTruck(truck6.Id, driver6.Id, orderLine2.Id, 1); // 3

            // Act
            await _dailyHistoryAppService.FillTenantDailyHistoryAsync(_todayUtc);

            // Assert
            var tenantDailyHistories = await GetTenantDailyHistories();
            tenantDailyHistories.Count.ShouldBe(2);
            tenantDailyHistories.First(x => x.TenantId == 1).InternalTrucksScheduled.ShouldBe(3);
            tenantDailyHistories.First(x => x.TenantId == _tenant2Id).InternalTrucksScheduled.ShouldBe(0);
        }

        [Fact]
        public async Task Test_FillTenantDailyHistory_should_fill_InternalScheduledDeliveries()
        {
            // Arrange
            var order = await CreateOrderWithOrderLines(_yesterdayUtc);
            var orderLine1 = order.OrderLines.ToList()[0];
            var orderLine2 = order.OrderLines.ToList()[1];
            var dumpTrucks = await CreateVehicleCategory();
            var trailers = await CreateTrailerVehicleCategory();
            var tractors = await CreateTractorVehicleCategory();
            var truck1 = await CreateTruck(vehicleCategory: dumpTrucks);
            var driver1 = await CreateDriver();
            await CreateOrderLineTruck(truck1.Id, driver1.Id, orderLine1.Id, .1m); // 1
            await CreateOrderLineTruck(truck1.Id, driver1.Id, orderLine2.Id, .1m); // 2
            var truck2 = await CreateTruck(vehicleCategory: trailers);
            await CreateOrderLineTruck(truck2.Id, null, orderLine1.Id, .1m);
            var truck3 = await CreateTruck(vehicleCategory: tractors, canPullTrailer: true);
            var driver3 = await CreateDriver();
            await CreateOrderLineTruck(truck3.Id, driver3.Id, orderLine1.Id, .1m); // 3
            var truck4 = await CreateTruck(vehicleCategory: dumpTrucks, alwaysShowOnSchedule: true);
            var driver4 = await CreateDriver();
            await CreateOrderLineTruck(truck4.Id, driver4.Id, orderLine1.Id, .1m);
            var truck5 = await CreateTruck(vehicleCategory: tractors, alwaysShowOnSchedule: true, canPullTrailer: true);
            var driver5 = await CreateDriver();
            await CreateOrderLineTruck(truck5.Id, driver5.Id, orderLine1.Id, .1m);

            var truck6 = await CreateTruck(vehicleCategory: dumpTrucks);
            var driver6 = await CreateDriver();
            await CreateOrderLineTruck(truck6.Id, driver6.Id, orderLine2.Id, 1); // 4

            // Act
            await _dailyHistoryAppService.FillTenantDailyHistoryAsync(_todayUtc);

            // Assert
            var tenantDailyHistories = await GetTenantDailyHistories();
            tenantDailyHistories.Count.ShouldBe(2);
            tenantDailyHistories.First(x => x.TenantId == 1).InternalScheduledDeliveries.ShouldBe(4);
            tenantDailyHistories.First(x => x.TenantId == _tenant2Id).InternalScheduledDeliveries.ShouldBe(0);
        }

        [Fact(Skip = "#14648 Historically failing")]
        public async Task Test_FillTenantDailyHistory_should_fill_LeaseHaulerScheduledDeliveries()
        {
            // Arrange
            var order = await CreateOrderWithOrderLines(_yesterdayUtc);
            var orderLine1 = order.OrderLines.ToList()[0];
            var orderLine2 = order.OrderLines.ToList()[1];
            var dumpTrucks = await CreateVehicleCategory();
            var trailers = await CreateTrailerVehicleCategory();
            var tractors = await CreateTractorVehicleCategory();
            var truck1 = await CreateTruck(vehicleCategory: dumpTrucks);
            var driver1 = await CreateDriver();
            await CreateOrderLineTruck(truck1.Id, driver1.Id, orderLine1.Id, .1m);
            await CreateOrderLineTruck(truck1.Id, driver1.Id, orderLine2.Id, .1m);
            var truck2 = await CreateTruck(vehicleCategory: trailers);
            await CreateOrderLineTruck(truck2.Id, null, orderLine1.Id, .1m);
            var truck3 = await CreateTruck(vehicleCategory: tractors, canPullTrailer: true);
            var driver3 = await CreateDriver();
            await CreateOrderLineTruck(truck3.Id, driver3.Id, orderLine1.Id, .1m);
            var truck4 = await CreateTruck(vehicleCategory: dumpTrucks, alwaysShowOnSchedule: true);
            var driver4 = await CreateDriver();
            await CreateOrderLineTruck(truck4.Id, driver4.Id, orderLine1.Id, .1m); // 1
            var truck5 = await CreateTruck(vehicleCategory: tractors, alwaysShowOnSchedule: true, canPullTrailer: true);
            var driver5 = await CreateDriver();
            await CreateOrderLineTruck(truck5.Id, driver5.Id, orderLine1.Id, .1m); // 2

            var truck6 = await CreateTruck(vehicleCategory: dumpTrucks, alwaysShowOnSchedule: true);
            var driver6 = await CreateDriver();
            await CreateOrderLineTruck(truck6.Id, driver6.Id, orderLine2.Id, 1); // 3

            // Act
            await _dailyHistoryAppService.FillTenantDailyHistoryAsync(_todayUtc);

            // Assert
            var tenantDailyHistories = await GetTenantDailyHistories();
            tenantDailyHistories.Count.ShouldBe(2);
            tenantDailyHistories.First(x => x.TenantId == 1).LeaseHaulerScheduledDeliveries.ShouldBe(3);
            tenantDailyHistories.First(x => x.TenantId == _tenant2Id).LeaseHaulerScheduledDeliveries.ShouldBe(0);
        }

        private async Task<List<TenantDailyHistory>> GetTenantDailyHistories()
        {
            var tenantDailyHistory = await UsingDbContextAsync(async context => await context.TenantDailyHistory.ToListAsync());
            return tenantDailyHistory;
        }

        private async Task UpdateCreationTime(FullAuditedEntity entity, DateTime? creationTime = null)
        {
            await UsingDbContextAsync(context =>
            {
                entity.CreationTime = creationTime ?? new DateTime(2019, 1, 1);
                context.Update(entity);
                return Task.CompletedTask;
            });
        }

        public Task DisposeAsync() => Task.CompletedTask;
    }
}
