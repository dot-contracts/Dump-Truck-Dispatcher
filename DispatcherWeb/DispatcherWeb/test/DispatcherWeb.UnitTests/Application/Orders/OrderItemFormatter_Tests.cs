using DispatcherWeb.Orders;
using DispatcherWeb.Orders.Dto;
using Shouldly;
using Xunit;

namespace DispatcherWeb.UnitTests.Application.Orders
{
    public class OrderItemFormatter_Tests
    {

        [Fact]
        public void Test_OrderItem_Formatting_Without_Material_Quantity_and_UOM()
        {
            // Arrange
            var orderLine = new OrderLineItemWithQuantityDto { FreightItemName = "Hauling", MaterialItemName = "Sand", FreightUomName = "Tons", MaterialUomName = "", FreightQuantity = 20M };

            // Act
            var itemName = OrderItemFormatter.GetItemWithQuantityFormatted(orderLine);

            // Assert
            itemName.ShouldBe("Hauling of Sand - 20 Tons");
        }

        [Fact]
        public void Test_OrderItem_Formatting_Without_Freight_UOM_and_Material_Quantity_and_UOM()
        {
            // Arrange
            var orderLine = new OrderLineItemWithQuantityDto { FreightItemName = "Hauling", MaterialItemName = "Sand", FreightUomName = "Tons" };

            // Act
            var itemName = OrderItemFormatter.GetItemWithQuantityFormatted(orderLine);

            // Assert
            itemName.ShouldBe("Hauling of Sand - Tons");
        }

        [Fact]
        public void Test_OrderItem_Formatting_With_Material_and_Freight_Quantity_and_UOM()
        {
            // Arrange
            var orderLine = new OrderLineItemWithQuantityDto { FreightItemName = "Hauling", MaterialItemName = "Sand", FreightUomName = "Tons", MaterialUomName = "Tons", FreightQuantity = 20M, MaterialQuantity = 200M };

            // Act
            var itemName = OrderItemFormatter.GetItemWithQuantityFormatted(orderLine);

            // Assert
            itemName.ShouldBe("Hauling of Sand - 200 Tons");
        }

        [Fact]
        public void Test_OrderItem_Formatting_Without_Material_and_Freight_Quantity()
        {
            // Arrange
            var orderLine = new OrderLineItemWithQuantityDto { FreightItemName = "Hauling", MaterialItemName = "Sand", FreightUomName = "Tons", MaterialUomName = "Tons" };

            // Act
            var itemName = OrderItemFormatter.GetItemWithQuantityFormatted(orderLine);

            // Assert
            itemName.ShouldBe("Hauling of Sand - Tons");
        }

        [Fact]
        public void Test_OrderItem_Formatting_Without_MaterialItem_and_Quantity_and_UOM()
        {
            // Arrange
            var orderLine = new OrderLineItemWithQuantityDto { FreightItemName = "Hauling", FreightUomName = "Tons", FreightQuantity = 200M };

            // Act
            var itemName = OrderItemFormatter.GetItemWithQuantityFormatted(orderLine);

            // Assert
            itemName.ShouldBe("Hauling - 200 Tons");
        }

        [Fact]
        public void Test_OrderItem_Formatting_Without_Freight_Quantity_and_MaterialItem_and_Quantity_and_UOM()
        {
            // Arrange
            var orderLine = new OrderLineItemWithQuantityDto { FreightItemName = "Hauling", FreightUomName = "Tons" };

            // Act
            var itemName = OrderItemFormatter.GetItemWithQuantityFormatted(orderLine);

            // Assert
            itemName.ShouldBe("Hauling - Tons");
        }

        [Fact]
        public void Test_OrderItem_Formatting_Without_Material_Quantity()
        {
            // Arrange
            var orderLine = new OrderLineItemWithQuantityDto { FreightItemName = "Hourly Hauling", MaterialItemName = "Sand", FreightUomName = "Hours", MaterialUomName = "Loads", FreightQuantity = 10M };

            // Act
            var itemName = OrderItemFormatter.GetItemWithQuantityFormatted(orderLine);

            // Assert
            itemName.ShouldBe("Hourly Hauling of Sand - 10 Hours");
        }

        [Fact]
        public void Test_OrderItem_Formatting_Without_FreightItem_Quantity_and_UOM()
        {
            // Arrange
            var orderLine = new OrderLineItemWithQuantityDto { MaterialItemName = "Sand", MaterialUomName = "Cu Yds", MaterialQuantity = 20M };

            // Act
            var itemName = OrderItemFormatter.GetItemWithQuantityFormatted(orderLine);

            // Assert
            itemName.ShouldBe("Sand - 20 Cu Yds");
        }

        [Fact]
        public void Test_OrderItem_Formatting_for_SeparateFreightAndMaterial_Disabled_Without_Material_UOM_and_Quantity()
        {
            // Arrange
            var orderLine = new OrderLineItemWithQuantityDto { FreightItemName = "Sand", FreightUomName = "Tons", FreightQuantity = 20M };

            // Act
            var itemName = OrderItemFormatter.GetItemWithQuantityFormatted(orderLine);

            // Assert
            itemName.ShouldBe("Sand - 20 Tons");
        }

        [Fact]
        public void Test_OrderItem_Formatting_for_SeparateFreightAndMaterial_Disabled_Without_Material_UOM_and_Quantity_and_Freight_Quantity()
        {
            // Arrange
            var orderLine = new OrderLineItemWithQuantityDto { FreightItemName = "Sand", FreightUomName = "Tons" };

            // Act
            var itemName = OrderItemFormatter.GetItemWithQuantityFormatted(orderLine);

            // Assert
            itemName.ShouldBe("Sand - Tons");
        }

        [Fact]
        public void Test_OrderItem_Formatting_for_SeparateFreightAndMaterial_Disabled_With_Material_and_Freight_UOM_and_Quantity()
        {
            // Arrange
            var orderLine = new OrderLineItemWithQuantityDto { FreightItemName = "Sand", FreightUomName = "Tons", MaterialUomName = "Tons", FreightQuantity = 20M, MaterialQuantity = 10M };

            // Act
            var itemName = OrderItemFormatter.GetItemWithQuantityFormatted(orderLine);

            // Assert
            itemName.ShouldBe("Sand - 10 Tons");
        }

        [Fact]
        public void Test_OrderItem_Formatting_for_SeparateFreightAndMaterial_Disabled_Without_Material_and_Freight_Quantity()
        {
            // Arrange
            var orderLine = new OrderLineItemWithQuantityDto { FreightItemName = "Sand", FreightUomName = "Tons", MaterialUomName = "Tons" };

            // Act
            var itemName = OrderItemFormatter.GetItemWithQuantityFormatted(orderLine);

            // Assert
            itemName.ShouldBe("Sand - Tons");
        }

        [Fact]
        public void Test_OrderItem_Formatting_for_SeparateFreightAndMaterial_Disabled_Without_Material_Quantity()
        {
            // Arrange
            var orderLine = new OrderLineItemWithQuantityDto { FreightItemName = "Hourly Hauling", FreightUomName = "Hours", MaterialUomName = "Loads", FreightQuantity = 10M };

            // Act
            var itemName = OrderItemFormatter.GetItemWithQuantityFormatted(orderLine);

            // Assert
            itemName.ShouldBe("Hourly Hauling - 10 Hours");
        }

        [Fact]
        public void Test_OrderItem_Formatting_for_Quantity_With_1_Decimal_point()
        {
            // Arrange
            var orderLine = new OrderLineItemWithQuantityDto { FreightItemName = "Hourly Hauling", FreightUomName = "Hours", MaterialUomName = "Loads", FreightQuantity = 3.5M };

            // Act
            var itemName = OrderItemFormatter.GetItemWithQuantityFormatted(orderLine);

            // Assert
            itemName.ShouldBe("Hourly Hauling - 3.5 Hours");
        }

        [Fact]
        public void Test_OrderItem_Formatting_for_Quantity_With_2_Decimal_points()
        {
            // Arrange
            var orderLine = new OrderLineItemWithQuantityDto { FreightItemName = "Hauling", MaterialItemName = "Sand", FreightUomName = "Tons", MaterialUomName = "Tons", FreightQuantity = 20M, MaterialQuantity = 200.52M };

            // Act
            var itemName = OrderItemFormatter.GetItemWithQuantityFormatted(orderLine);

            // Assert
            itemName.ShouldBe("Hauling of Sand - 200.52 Tons");
        }
    }
}