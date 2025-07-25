using System.Linq;
using System.Threading.Tasks;
using Abp.Application.Features;
using Abp.Timing;
using DispatcherWeb.Configuration;
using DispatcherWeb.Items;
using DispatcherWeb.Orders;
using DispatcherWeb.Orders.Dto;
using Microsoft.EntityFrameworkCore;
using NSubstitute;
using Shouldly;
using Xunit;

namespace DispatcherWeb.Tests.Orders
{
    public class OrderAppService_CopyOrder_Tests : AppTestBase, IAsyncLifetime
    {
        private IOrderAppService _orderAppService;
        private int _officeId;

        public async Task InitializeAsync()
        {
            var office = await CreateOfficeAndAssignUserToIt();
            _officeId = office.Id;
            _orderAppService = Resolve<IOrderAppService>();
            SubstituteServiceDependencies(_orderAppService);
        }

        [Fact]
        public async Task Test_CopyOrder_should_copy_ScheduledTrucks_when_CopyTrucks_is_true()
        {
            // Arrange
            var today = Clock.Now.Date;
            var order = await CreateOrderWithOrderLines(today);
            var orderLine = order.OrderLines.First();

            // Act
            await _orderAppService.CopyOrder(new CopyOrderInput
            {
                DateBegin = today.AddDays(1),
                DateEnd = today.AddDays(1),
                OrderId = order.Id,
                OrderLineId = orderLine.Id,
                CopyTrucks = true,
            });

            // Assert
            var newOrders = await UsingDbContext(async context => await context.Orders.Include(o => o.OrderLines).Where(o => o.Id != order.Id).ToListAsync());
            newOrders.Count.ShouldBe(1);
            newOrders[0].OrderLines.Count.ShouldBe(1);
            newOrders[0].OrderLines.First().ScheduledTrucks.ShouldBe(orderLine.ScheduledTrucks);
        }

        [Fact]
        public async Task Test_CopyOrder_should_not_copy_ScheduledTrucks_when_CopyTrucks_is_false()
        {
            // Arrange
            var today = Clock.Now.Date;
            var order = await CreateOrderWithOrderLines(today);
            var orderLine = order.OrderLines.First();

            // Act
            await _orderAppService.CopyOrder(new CopyOrderInput
            {
                DateBegin = today.AddDays(1),
                DateEnd = today.AddDays(1),
                OrderId = order.Id,
                OrderLineId = orderLine.Id,
                CopyTrucks = false,
            });

            // Assert
            var newOrders = await UsingDbContext(async context => await context.Orders.Include(o => o.OrderLines).Where(o => o.Id != order.Id).ToListAsync());
            newOrders.Count.ShouldBe(1);
            newOrders[0].OrderLines.Count.ShouldBe(1);
            newOrders[0].OrderLines.First().ScheduledTrucks.ShouldBeNull();
        }

        [Fact(Skip = "#14648 Historically failing")]
        public async Task Test_CopyOrder_should_copy_the_entire_order_for_each_date()
        {
            // Arrange
            var today = Clock.Now.Date;
            var order = await CreateOrderWithOrderLines(today);
            await UpdateEntity(order.OrderLines.First(ol => ol.LineNumber == 1), UpdateOrderLine);

            // Act
            await _orderAppService.CopyOrder(new CopyOrderInput
            {
                DateBegin = today.AddDays(1),
                DateEnd = today.AddDays(2),
                OrderId = order.Id,
            });

            // Assert
            var newOrders = await UsingDbContext(async context => await context.Orders.Include(o => o.OrderLines).Where(o => o.Id != order.Id).ToListAsync());
            newOrders.Count.ShouldBe(2);
            AssertOrderLinesHaveSameProperties(newOrders[0].OrderLines.First(ol => ol.LineNumber == 1), order.OrderLines.First(ol => ol.LineNumber == 1));
            AssertOrderLinesHaveSameProperties(newOrders[1].OrderLines.First(ol => ol.LineNumber == 1), order.OrderLines.First(ol => ol.LineNumber == 1));
            AssertOrderLinesHaveSameProperties(newOrders[0].OrderLines.First(ol => ol.LineNumber == 2), order.OrderLines.First(ol => ol.LineNumber == 2));
            AssertOrderLinesHaveSameProperties(newOrders[1].OrderLines.First(ol => ol.LineNumber == 2), order.OrderLines.First(ol => ol.LineNumber == 2));

            // Local functions
            void UpdateOrderLine(OrderLine ol)
            {
                ol.TimeOnJob = Clock.Now;
                ol.Note = "note";
                ol.IsMultipleLoads = true;
            }
        }

        [Fact]
        public async Task Test_CopyOrder_should_copy_the_entire_order_for_each_date_and_each_Shift()
        {
            // Arrange
            var today = Clock.Now.Date;
            var order = await CreateOrderWithOrderLines(today);
            await UpdateEntity(order, o => o.Shift = Shift.Shift1);
            SubstituteSetting(AppSettings.General.UseShifts, "true");

            // Act
            await _orderAppService.CopyOrder(new CopyOrderInput
            {
                DateBegin = today.AddDays(1),
                DateEnd = today.AddDays(2),
                OrderId = order.Id,
                Shifts = new Shift[] { Shift.Shift1, Shift.Shift2 },
            });

            // Assert
            var newOrders = await UsingDbContext(async context => await context.Orders.Where(o => o.Id != order.Id).ToListAsync());
            newOrders.Count.ShouldBe(4);
            newOrders.Count(o => o.Shift == Shift.Shift1).ShouldBe(2);
            newOrders.Count(o => o.Shift == Shift.Shift2).ShouldBe(2);
        }

        [Fact(Skip = "#14648 Historically failing")]
        public async Task Test_CopyOrder_should_not_copy_OrderLines_with_0_MaterialQuantity()
        {
            // Arrange
            var today = Clock.Now.Date;
            var order = await CreateOrderWithOrderLines(today);
            order = await UpdateEntity(order, o =>
            {
                o.OrderLines.Add(new OrderLine
                {
                    TenantId = 1,
                    LineNumber = 1,
                    Designation = DesignationEnum.FreightAndMaterial,
                    MaterialQuantity = 0,
                    FreightPricePerUnit = 2,
                    MaterialPricePerUnit = 3,
                    FreightItem = new Item
                    {
                        TenantId = 1,
                        Name = "sss",
                    },
                    MaterialUomId = 1,
                    NumberOfTrucks = 2,
                });

            });
            order.OrderLines.Count.ShouldBe(3);

            // Act
            await _orderAppService.CopyOrder(new CopyOrderInput
            {
                DateBegin = today.AddDays(1),
                DateEnd = today.AddDays(1),
                OrderId = order.Id,
            });

            // Assert
            var newOrders = await UsingDbContext(async context => await context.Orders.Include(o => o.OrderLines).Where(o => o.Id != order.Id).ToListAsync());
            newOrders.Count.ShouldBe(1);
            newOrders[0].OrderLines.Count.ShouldBe(2);
        }

        [Fact]
        public async Task Test_CopyOrder_should_copy_OrderLines_with_0_MaterialQuantity_when_AllowCopyZeroQauntity_feature_is_true()
        {
            // Arrange
            var today = Clock.Now.Date;
            var order = await CreateOrderWithOrderLines(today);
            order = await UpdateEntity(order, o =>
            {
                o.OrderLines.Add(new OrderLine
                {
                    TenantId = 1,
                    LineNumber = 1,
                    Designation = DesignationEnum.FreightAndMaterial,
                    MaterialQuantity = 0,
                    FreightPricePerUnit = 2,
                    MaterialPricePerUnit = 3,
                    FreightItem = new Item
                    {
                        TenantId = 1,
                        Name = "sss",
                    },
                    MaterialUomId = 1,
                    NumberOfTrucks = 2,
                });

            });
            var featureChecker = Substitute.For<IFeatureChecker>();
            ((DispatcherWebAppServiceBase)_orderAppService).FeatureChecker = featureChecker;

            // Act
            await _orderAppService.CopyOrder(new CopyOrderInput
            {
                DateBegin = today.AddDays(1),
                DateEnd = today.AddDays(1),
                OrderId = order.Id,
            });

            // Assert
            var newOrders = await UsingDbContext(async context => await context.Orders.Include(o => o.OrderLines).Where(o => o.Id != order.Id).ToListAsync());
            newOrders.Count.ShouldBe(1);
            newOrders[0].OrderLines.Count.ShouldBe(3);
        }


        private void AssertOrderLinesHaveSameProperties(OrderLine ol1, OrderLine ol2)
        {
            typeof(OrderLine).GetProperties().Length.ShouldBe(46, "The number of properties in the OrderLine entity is changed. Consider to add new properties to copy methods.");
            ol1.LineNumber.ShouldBe(ol2.LineNumber);
            ol1.MaterialQuantity.ShouldBe(ol2.MaterialQuantity);
            ol1.NumberOfTrucks.ShouldBe(ol2.NumberOfTrucks);
            ol1.MaterialPricePerUnit.ShouldBe(ol2.MaterialPricePerUnit);
            ol1.FreightPricePerUnit.ShouldBe(ol2.FreightPricePerUnit);
            ol1.IsMaterialPricePerUnitOverridden.ShouldBe(ol2.IsMaterialPricePerUnitOverridden);
            ol1.IsFreightPricePerUnitOverridden.ShouldBe(ol2.IsFreightPricePerUnitOverridden);
            ol1.FreightItemId.ShouldBe(ol2.FreightItemId);
            ol1.LoadAtId.ShouldBe(ol2.LoadAtId);
            ol1.MaterialUomId.ShouldBe(ol2.MaterialUomId);
            ol1.Designation.ShouldBe(ol2.Designation);
            ol1.MaterialPrice.ShouldBe(ol2.MaterialPrice);
            ol1.FreightPrice.ShouldBe(ol2.FreightPrice);
            ol1.IsMaterialPriceOverridden.ShouldBe(ol2.IsMaterialPriceOverridden);
            ol1.IsFreightPriceOverridden.ShouldBe(ol2.IsFreightPriceOverridden);
            //the timeOnJob is UTC now, so TimeOfDay won't always work
            //(ol1.TimeOnJobObsolete?.TimeOfDay ?? TimeSpan.Zero).ShouldBe(ol2.TimeOnJobObsolete?.TimeOfDay ?? TimeSpan.Zero);
            ol1.Note.ShouldBe(ol2.Note);
            ol1.IsMultipleLoads.ShouldBe(ol2.IsMultipleLoads);
        }

        public Task DisposeAsync()
        {
            return Task.CompletedTask;
        }
    }
}
