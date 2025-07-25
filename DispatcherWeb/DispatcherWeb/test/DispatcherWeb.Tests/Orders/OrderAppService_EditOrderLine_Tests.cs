using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Abp.UI;
using DispatcherWeb.Locations;
using DispatcherWeb.Orders;
using DispatcherWeb.Orders.Dto;
using Shouldly;
using Xunit;

namespace DispatcherWeb.Tests.Orders
{
    public class OrderAppService_EditOrderLine_Tests : OrderAppServiceTestBase
    {
        [Fact]
        public async Task Test_EditOrderLine_should_update_existing_OrderLine()
        {
            // Arrange
            var model = CreateOrderLineEditDto(_orderLine);
            var location = await ChangeLocation(model);

            // Act
            var output = await _orderAppService.EditOrderLine(model);

            // Assert
            var updatedOrderLine = await UsingDbContextAsync(async context => await context.OrderLines.FindAsync(_orderLineId));
            updatedOrderLine.LoadAtId.ShouldBe(location.Id);
        }

        [Fact]
        public async Task Test_EditOrderLine_should_throw_UserFriendlyException_when_changing_Location_and_there_is_Ticket()
        {
            // Arrange
            var model = CreateOrderLineEditDto(_orderLine);
            var location = await ChangeLocation(model);
            var truck = await CreateTruck();
            await CreateTicket(_orderLine, truck);

            // Act, Assert
            await _orderAppService.EditOrderLine(model).ShouldThrowAsync(typeof(UserFriendlyException));
        }

        [Fact(Skip = "#14648 Historically failing")]
        public async Task Test_EditOrderLine_should_throw_UserFriendlyException_when_changing_Quantity_and_Price_and_Order_is_paid()
        {
            // Arrange
            await UpdateEntity(_payment, o => o.AuthorizationCaptureDateTime = DateTime.UtcNow);
            var model = CreateOrderLineEditDto(_orderLine);
            var newQuantity = 123.45m;
            model.MaterialQuantity = newQuantity;
            model.FreightQuantity = newQuantity;
            model.FreightPrice = model.FreightQuantity * model.FreightPricePerUnit ?? model.FreightPrice;
            model.MaterialPrice = model.MaterialQuantity * model.MaterialPricePerUnit ?? model.MaterialPrice;

            // Act, Assert
            await _orderAppService.EditOrderLine(model).ShouldThrowAsync(typeof(UserFriendlyException));
        }

        [Fact(Skip = "#14648 Historically failing")]
        public async Task Test_EditOrderLine_should_update_Quantity_with_Price_and_there_is_Ticket()
        {
            // Arrange
            var model = CreateOrderLineEditDto(_orderLine);
            var newQuantity = 123.45m;
            model.MaterialQuantity = newQuantity;
            model.FreightQuantity = newQuantity;
            model.FreightPrice = model.FreightQuantity * model.FreightPricePerUnit ?? model.FreightPrice;
            model.MaterialPrice = model.MaterialQuantity * model.MaterialPricePerUnit ?? model.MaterialPrice;
            var truck = await CreateTruck();
            await CreateTicket(_orderLine, truck);

            // Act
            await _orderAppService.EditOrderLine(model);

            // Assert
            var updatedOrderLine = await UsingDbContextAsync(async context => await context.OrderLines.FindAsync(_orderLineId));
            updatedOrderLine.MaterialQuantity.ShouldBe(newQuantity);
            updatedOrderLine.FreightQuantity.ShouldBe(newQuantity);
        }

        [Fact(Skip = "#14648 Historically failing")]
        public async Task Test_EditOrderLine_should_update_Freight_and_Material_Rates_when_there_is_Ticket()
        {
            // Arrange
            var model = CreateOrderLineEditDto(_orderLine);
            var newQuantity = 123.45m;
            model.MaterialQuantity = newQuantity;
            model.FreightQuantity = newQuantity;
            model.FreightPricePerUnit = (model.FreightPricePerUnit ?? 1) * 2;
            model.MaterialPricePerUnit = (model.MaterialPricePerUnit ?? 1) * 2;
            var truck = await CreateTruck();
            await CreateTicket(_orderLine, truck);

            // Act
            await _orderAppService.EditOrderLine(model);

            // Assert
            var updatedOrderLine = await UsingDbContextAsync(async context => await context.OrderLines.FindAsync(_orderLineId));
            updatedOrderLine.FreightPricePerUnit.ShouldBe(model.FreightPricePerUnit);
            updatedOrderLine.MaterialPricePerUnit.ShouldBe(model.MaterialPricePerUnit);
        }

        [Fact]
        public async Task Test_EditOrderLine_should_remove_OrderLineTruck_when_setting_NumberOfTrucks_0_and_Quantity_0()
        {
            // Arrange
            var truck = await CreateTruck();
            var driver = await CreateDriver();
            var orderLineTruck = await CreateOrderLineTruck(truck.Id, driver.Id, _orderLine.Id, 1);

            var model = CreateOrderLineEditDto(_orderLine);
            model.MaterialQuantity = 0;
            model.FreightQuantity = 0;
            model.NumberOfTrucks = 0;

            // Act
            var result = await _orderAppService.EditOrderLine(model);

            // Assert
            var updatedOrderLine = await UsingDbContextAsync(async context => await context.OrderLines.FindAsync(_orderLine.Id));
            updatedOrderLine.NumberOfTrucks.ShouldBe(0);
            var updatedOrderLineTruck = await UsingDbContextAsync(async context => await context.OrderLineTrucks.FindAsync(orderLineTruck.Id));
            updatedOrderLineTruck.IsDeleted.ShouldBeTrue();

        }


        [Fact(Skip = "NumberOfTrucks shouldn't throw the exception any more. It should be SharedTrucks if it is added to the OrderLine modal")]
        public async Task Test_EditOrderLine_should_throw_ApplicationException_when_NumberOfTrucks_is_less_than_Utilization()
        {
            // Arrange
            var model = CreateOrderLineEditDto(_orderLine);
            model.NumberOfTrucks = 0;
            var truck = await CreateTruck();
            var driver = await CreateDriver();
            await CreateOrderLineTruck(truck.Id, driver.Id, _orderLineId, 1);

            // Act, Assert
            await _orderAppService.EditOrderLine(model).ShouldThrowAsync(typeof(ApplicationException));
        }

        private async Task<Location> ChangeLocation(OrderLineEditDto model)
        {
            var location = await UsingDbContextAsync(async context =>
            {
                var s = new Location
                {
                    TenantId = 1,
                    Name = "Location2",
                };
                await context.Locations.AddAsync(s);
                return s;
            });
            model.LoadAtId = location.Id;
            return location;
        }

        private OrderLineEditDto CreateOrderLineEditDto(OrderLine orderLine)
        {
            return new OrderLineEditDto
            {
                Id = orderLine.Id,

                Designation = orderLine.Designation,
                FreightPrice = orderLine.FreightPrice,
                FreightPricePerUnit = orderLine.FreightPricePerUnit,
                IsFreightPricePerUnitOverridden = orderLine.IsFreightPricePerUnitOverridden,
                IsFreightRateToPayDriversOverridden = orderLine.IsFreightRateToPayDriversOverridden,
                IsMaterialPricePerUnitOverridden = orderLine.IsMaterialPricePerUnitOverridden,
                IsLeaseHaulerPriceOverridden = orderLine.IsLeaseHaulerPriceOverridden,
                LineNumber = orderLine.LineNumber,
                MaterialPrice = orderLine.MaterialPrice,
                MaterialPricePerUnit = orderLine.MaterialPricePerUnit,
                IsMaterialPriceOverridden = orderLine.IsMaterialPriceOverridden,
                IsFreightPriceOverridden = orderLine.IsFreightPriceOverridden,
                Note = orderLine.Note,
                OrderId = orderLine.OrderId,
                //Quantity = orderLine.Quantity,
                MaterialQuantity = orderLine.MaterialQuantity,
                FreightQuantity = orderLine.FreightQuantity,
                MaterialUomId = orderLine.MaterialUomId,
                MaterialUomName = null,
                FreightUomId = orderLine.FreightUomId,
                FreightUomName = null,
                QuoteId = null,
                FreightItemId = orderLine.FreightItemId,
                FreightItemName = null,
                LoadAtId = orderLine.LoadAtId,
                //LoadAtName = null,
                //UnitOfMeasureId = orderLine.UnitOfMeasureId,
                //UnitOfMeasureName = null,
                VehicleCategories = new List<OrderLineVehicleCategoryDto>(),
            };
        }
    }
}
