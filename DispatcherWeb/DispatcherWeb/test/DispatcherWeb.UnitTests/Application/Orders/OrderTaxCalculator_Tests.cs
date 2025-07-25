using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Abp.Application.Features;
using Abp.Configuration;
using Abp.Domain.Repositories;
using DispatcherWeb.Configuration;
using DispatcherWeb.Features;
using DispatcherWeb.Orders;
using DispatcherWeb.Orders.TaxDetails;
using NSubstitute;
using Shouldly;
using Xunit;

namespace DispatcherWeb.UnitTests.Application.Orders
{
    public class OrderTaxCalculator_Tests
    {
        private readonly OrderTaxCalculator _orderTaxCalculator;
        private readonly ISettingManager _settingManager;
        private readonly IFeatureChecker _featureChecker;

        public OrderTaxCalculator_Tests()
        {
            _settingManager = Substitute.For<ISettingManager>();
            _featureChecker = Substitute.For<IFeatureChecker>();
            var orderRepository = Substitute.For<IRepository<Order>>();
            var orderLineRepository = Substitute.For<IRepository<OrderLine>>();
            var receiptRepository = Substitute.For<IRepository<Receipt>>();
            var receiptLineRepository = Substitute.For<IRepository<ReceiptLine>>();

            _orderTaxCalculator = new OrderTaxCalculator(
                _settingManager,
                orderRepository,
                orderLineRepository,
                receiptRepository,
                receiptLineRepository,
                _featureChecker
            );
        }

        private IOrderTaxDetails GetSampleOrder()
        {
            return new OrderTaxDetailsDto
            {
                Id = 1,
                SalesTaxRate = 10,
            };
        }

        private IEnumerable<IOrderLineTaxDetails> GetSampleOrderLines()
        {
            return new List<OrderLineTaxDetailsDto>
            {
                new OrderLineTaxDetailsDto { FreightPrice = 30, MaterialPrice = 220, IsTaxable = true, IsMaterialTaxable = true, IsFreightTaxable = true },
                new OrderLineTaxDetailsDto { FreightPrice = 600, MaterialPrice = 0, IsTaxable = true, IsMaterialTaxable = true, IsFreightTaxable = true },
                new OrderLineTaxDetailsDto { FreightPrice = 0, MaterialPrice = 400, IsTaxable = true, IsMaterialTaxable = true, IsFreightTaxable = true },
            };
        }

        [Fact]
        public async Task Test_GetTaxCalculationTypeAsync()
        {
            // Arrange
            _settingManager.GetSettingValueAsync(Arg.Is(AppSettings.Invoice.TaxCalculationType))
                .Returns("1", "3");

            // Act
            var a = await _orderTaxCalculator.GetTaxCalculationTypeAsync();
            var b = await _orderTaxCalculator.GetTaxCalculationTypeAsync();

            // Assert
            a.ShouldBe(TaxCalculationType.FreightAndMaterialTotal);
            b.ShouldBe(TaxCalculationType.MaterialTotal);
        }

        [Fact]
        public async Task Test_CalculateTotals_should_not_round_input_values()
        {
            // Arrange
            var order = GetSampleOrder();
            var isSeparateMaterialAndFreightItemsEnabled = await _featureChecker.IsEnabledAsync(AppFeatures.SeparateMaterialAndFreightItems);
            order.SalesTaxRate = 10.12345M;

            // Act
            OrderTaxCalculator.CalculateTotals(TaxCalculationType.FreightAndMaterialTotal, order, GetSampleOrderLines(), isSeparateMaterialAndFreightItemsEnabled);

            // Assert
            order.SalesTaxRate.ShouldBe(10.12345M);
        }

        [Fact]
        public async Task Test_CalculateTotals_for_FreightAndMaterialTotal()
        {
            // Arrange
            var order = GetSampleOrder();
            var isSeparateMaterialAndFreightItemsEnabled = await _featureChecker.IsEnabledAsync(AppFeatures.SeparateMaterialAndFreightItems);

            // Act
            OrderTaxCalculator.CalculateTotals(TaxCalculationType.FreightAndMaterialTotal, order, GetSampleOrderLines(), isSeparateMaterialAndFreightItemsEnabled);

            // Assert
            //specific for this calculation type
            order.SalesTax.ShouldBe(125);
            order.CODTotal.ShouldBe(1375.0M);

            //common for all types of calculation
            order.FreightTotal.ShouldBe(630);
            order.MaterialTotal.ShouldBe(620);

            //should remain unchanged
            order.Id.ShouldBe(1);
            order.SalesTaxRate.ShouldBe(10);
        }

        [Fact]
        public async Task Test_CalculateTotals_for_FreightAndMaterialTotal_ShouldUseBankersRounding_Down()
        {
            // Arrange
            var order = GetSampleOrder();
            order.SalesTaxRate = 5.5M;
            var isSeparateMaterialAndFreightItemsEnabled = await _featureChecker.IsEnabledAsync(AppFeatures.SeparateMaterialAndFreightItems);

            var orderLines = new List<OrderLineTaxDetailsDto>
            {
                new OrderLineTaxDetailsDto { MaterialPrice = 0, FreightPrice = 300, IsTaxable = true, IsMaterialTaxable = true, IsFreightTaxable = true },
                new OrderLineTaxDetailsDto { MaterialPrice = 40, FreightPrice = 0, IsTaxable = true, IsMaterialTaxable = true, IsFreightTaxable = true },
                new OrderLineTaxDetailsDto { MaterialPrice = 154, FreightPrice = 21, IsTaxable = true, IsMaterialTaxable = true, IsFreightTaxable = true },
            };

            // Act
            OrderTaxCalculator.CalculateTotals(TaxCalculationType.FreightAndMaterialTotal, order, orderLines, isSeparateMaterialAndFreightItemsEnabled);

            // Assert
            order.FreightTotal.ShouldBe(321);
            order.MaterialTotal.ShouldBe(194);
            order.SalesTax.ShouldBe(28.32M); //28.325 -> 28.32 (with banker's rounding)
            order.CODTotal.ShouldBe(543.32M); //543.325 -> 543.32 (with banker's rounding)
        }

        [Fact]
        public async Task Test_CalculateTotals_for_FreightAndMaterialTotal_ShouldUseBankersRounding_Up()
        {
            // Arrange
            var order = GetSampleOrder();
            order.SalesTaxRate = 5.7M;
            var isSeparateMaterialAndFreightItemsEnabled = await _featureChecker.IsEnabledAsync(AppFeatures.SeparateMaterialAndFreightItems);

            var orderLines = new List<OrderLineTaxDetailsDto>
            {
                new OrderLineTaxDetailsDto { MaterialPrice = 0, FreightPrice = 300, IsTaxable = true, IsMaterialTaxable = true, IsFreightTaxable = true },
                new OrderLineTaxDetailsDto { MaterialPrice = 40, FreightPrice = 0, IsTaxable = true, IsMaterialTaxable = true, IsFreightTaxable = true },
                new OrderLineTaxDetailsDto { MaterialPrice = 154, FreightPrice = 21, IsTaxable = true, IsMaterialTaxable = true, IsFreightTaxable = true },
            };

            // Act
            OrderTaxCalculator.CalculateTotals(TaxCalculationType.FreightAndMaterialTotal, order, orderLines, isSeparateMaterialAndFreightItemsEnabled);

            // Assert
            order.FreightTotal.ShouldBe(321);
            order.MaterialTotal.ShouldBe(194);
            order.SalesTax.ShouldBe(29.36M); //29.355 -> 29.36 (with banker's rounding Up)
            order.CODTotal.ShouldBe(544.36M); //544.355 -> 544.36 (with banker's rounding Up)
        }


        [Fact]
        public async Task Test_CalculateTotals_for_MaterialLineItemsTotal()
        {
            // Arrange
            var order = GetSampleOrder();
            var isSeparateMaterialAndFreightItemsEnabled = await _featureChecker.IsEnabledAsync(AppFeatures.SeparateMaterialAndFreightItems);

            // Act
            OrderTaxCalculator.CalculateTotals(TaxCalculationType.MaterialLineItemsTotal, order, GetSampleOrderLines(), isSeparateMaterialAndFreightItemsEnabled);

            // Assert
            //specific for this calculation type
            order.SalesTax.ShouldBe(65);
            order.CODTotal.ShouldBe(1315.0M);

            //common for all types of calculation
            order.FreightTotal.ShouldBe(630);
            order.MaterialTotal.ShouldBe(620);

            //should remain unchanged
            order.Id.ShouldBe(1);
            order.SalesTaxRate.ShouldBe(10);
        }

        [Fact]
        public async Task Test_CalculateTotals_for_MaterialTotal()
        {
            // Arrange
            var order = GetSampleOrder();
            var isSeparateMaterialAndFreightItemsEnabled = await _featureChecker.IsEnabledAsync(AppFeatures.SeparateMaterialAndFreightItems);

            // Act
            OrderTaxCalculator.CalculateTotals(TaxCalculationType.MaterialTotal, order, GetSampleOrderLines(), isSeparateMaterialAndFreightItemsEnabled);

            // Assert
            //specific for this calculation type
            order.SalesTax.ShouldBe(62);
            order.CODTotal.ShouldBe(1312.0M);

            //common for all types of calculation
            order.FreightTotal.ShouldBe(630);
            order.MaterialTotal.ShouldBe(620);

            //should remain unchanged
            order.Id.ShouldBe(1);
            order.SalesTaxRate.ShouldBe(10);
        }

        [Fact]
        public async Task Test_CalculateTotals_for_NoCalculation()
        {
            // Arrange
            var order = GetSampleOrder();
            order.SalesTax = 30;
            var isSeparateMaterialAndFreightItemsEnabled = await _featureChecker.IsEnabledAsync(AppFeatures.SeparateMaterialAndFreightItems);

            // Act
            OrderTaxCalculator.CalculateTotals(TaxCalculationType.NoCalculation, order, GetSampleOrderLines(), isSeparateMaterialAndFreightItemsEnabled);

            // Assert
            //specific for this calculation type
            order.CODTotal.ShouldBe(1280M);
            order.SalesTaxRate.ShouldBe(0);
            order.SalesTax.ShouldBe(30);

            //common for all types of calculation
            order.FreightTotal.ShouldBe(630);
            order.MaterialTotal.ShouldBe(620);

            //should remain unchanged
            order.Id.ShouldBe(1);
        }

        [Fact]
        public async Task Test_CalculateTotals_NoCalculation_should_round_SalesTax()
        {
            // Arrange
            var order = GetSampleOrder();
            order.SalesTax = 30.12345M;
            var isSeparateMaterialAndFreightItemsEnabled = await _featureChecker.IsEnabledAsync(AppFeatures.SeparateMaterialAndFreightItems);

            // Act
            OrderTaxCalculator.CalculateTotals(TaxCalculationType.NoCalculation, order, GetSampleOrderLines(), isSeparateMaterialAndFreightItemsEnabled);

            // Assert
            order.SalesTax.ShouldBe(30.12M);
        }

        [Fact]
        public async Task Test_CalculateTotals_for_FreightAndMaterialTotal_should_not_round_rates()
        {
            // Arrange
            var order = GetSampleOrder();
            order.SalesTaxRate = 23.678M;
            var isSeparateMaterialAndFreightItemsEnabled = await _featureChecker.IsEnabledAsync(AppFeatures.SeparateMaterialAndFreightItems);

            var orderLines = new List<OrderLineTaxDetailsDto>
            {
                new OrderLineTaxDetailsDto { FreightPrice = 1936.2M, IsTaxable = true, IsMaterialTaxable = true, IsFreightTaxable = true },
            };

            // Act
            OrderTaxCalculator.CalculateTotals(TaxCalculationType.FreightAndMaterialTotal, order, orderLines, isSeparateMaterialAndFreightItemsEnabled);

            // Assert
            //specific for this calculation type
            order.SalesTax.ShouldBe(458.45M);
            //order.CODTotal.ShouldBe(1406.5M);

            ////common for all types of calculation
            //order.FreightTotal.ShouldBe(630);
            //order.MaterialTotal.ShouldBe(620);

            ////should remain unchanged
            //order.Id.ShouldBe(1);
            //order.SalesTaxRate.ShouldBe(10);
        }

        [Fact]
        public void Test_CalculateTax_for_SeparateFreightAndMaterial_Enabled()
        {
            // Arrange
            var order = GetSampleOrder();
            var isSeparateMaterialAndFreightItemsEnabled = true;

            // Act
            OrderTaxCalculator.CalculateTotals(TaxCalculationType.NoCalculation, order, GetSampleOrderLines(), isSeparateMaterialAndFreightItemsEnabled);

            // Assert
            //If isSeparateMaterialAndFreightItemsEnabled do not consider any calculation type
            order.SalesTaxRate.ShouldBe(10);
            order.SalesTax.ShouldBe(125);
        }

        [Fact]
        public async Task Test_CalculateTax_for_TaxExempt_Order()
        {
            // Arrange
            var order = GetSampleOrder();
            order.SalesTaxRate = 0; //Set as Tax Exempt bv setting Sales Tax Rate to Zero (Quote, Order, Customer)
            var isSeparateMaterialAndFreightItemsEnabled = await _featureChecker.IsEnabledAsync(AppFeatures.SeparateMaterialAndFreightItems);

            // Act
            OrderTaxCalculator.CalculateTotals(TaxCalculationType.FreightAndMaterialTotal, order, GetSampleOrderLines(), isSeparateMaterialAndFreightItemsEnabled);

            // Assert
            //Regardless of the “Separate Material and Freight Items” feature, Sales Tax value is Zero
            order.SalesTaxRate.ShouldBe(0);
            order.SalesTax.ShouldBe(0);
        }

        [Fact]
        public void Test_CalculateTax_for_SeparateFreightAndMaterial_Enabled_and_Material_Freight_Taxable()
        {
            // Arrange
            var order = GetSampleOrder();
            var isSeparateMaterialAndFreightItemsEnabled = true;
            var taxRate = order.SalesTaxRate / 100;

            // Act
            OrderTaxCalculator.CalculateTotals(TaxCalculationType.FreightAndMaterialTotal, order, GetSampleOrderLines(), isSeparateMaterialAndFreightItemsEnabled);
            var taxableTotal = order.FreightTotal + order.MaterialTotal;
            var totalSalesTax = Math.Round(taxableTotal * taxRate, 2);

            // Assert
            order.SalesTaxRate.ShouldBe(10);
            order.SalesTax.ShouldBe(totalSalesTax);
        }

        [Fact]
        public void Test_CalculateTax_for_SeparateFreightAndMaterial_Enabled_and_Material_Taxable()
        {
            // Arrange
            var order = GetSampleOrder();
            var isSeparateMaterialAndFreightItemsEnabled = true;
            var taxRate = order.SalesTaxRate / 100;
            var orderLines = new List<OrderLineTaxDetailsDto>
            {
                new OrderLineTaxDetailsDto { FreightPrice = 30, MaterialPrice = 220, IsTaxable = true, IsMaterialTaxable = true, IsFreightTaxable = false },
                new OrderLineTaxDetailsDto { FreightPrice = 600, MaterialPrice = 0, IsTaxable = true, IsMaterialTaxable = true, IsFreightTaxable = false },
                new OrderLineTaxDetailsDto { FreightPrice = 0, MaterialPrice = 400, IsTaxable = true, IsMaterialTaxable = true, IsFreightTaxable = false },
            };

            // Act
            OrderTaxCalculator.CalculateTotals(TaxCalculationType.FreightAndMaterialTotal, order, orderLines, isSeparateMaterialAndFreightItemsEnabled);
            var totalSalesTax = Math.Round(order.MaterialTotal * taxRate, 2);

            // Assert
            order.SalesTaxRate.ShouldBe(10);
            order.SalesTax.ShouldBe(totalSalesTax);
        }

        [Fact]
        public void Test_CalculateTax_for_SeparateFreightAndMaterial_Enabled_and_Freight_Taxable()
        {
            // Arrange
            var order = GetSampleOrder();
            var isSeparateMaterialAndFreightItemsEnabled = true;
            var taxRate = order.SalesTaxRate / 100;
            var orderLines = new List<OrderLineTaxDetailsDto>
            {
                new OrderLineTaxDetailsDto { FreightPrice = 30, MaterialPrice = 220, IsTaxable = true, IsMaterialTaxable = false, IsFreightTaxable = true },
                new OrderLineTaxDetailsDto { FreightPrice = 600, MaterialPrice = 0, IsTaxable = true, IsMaterialTaxable = false, IsFreightTaxable = true },
                new OrderLineTaxDetailsDto { FreightPrice = 0, MaterialPrice = 400, IsTaxable = true, IsMaterialTaxable = false, IsFreightTaxable = true },
            };

            // Act
            OrderTaxCalculator.CalculateTotals(TaxCalculationType.FreightAndMaterialTotal, order, orderLines, isSeparateMaterialAndFreightItemsEnabled);
            var totalSalesTax = Math.Round(order.FreightTotal * taxRate, 2);

            // Assert
            order.SalesTaxRate.ShouldBe(10);
            order.SalesTax.ShouldBe(totalSalesTax);
        }

        [Fact]
        public void Test_CalculateTax_for_TaxCalculationType_FreightAndMaterial()
        {
            // Arrange
            var order = GetSampleOrder();
            var isSeparateMaterialAndFreightItemsEnabled = false;

            // Act
            OrderTaxCalculator.CalculateTotals(TaxCalculationType.FreightAndMaterialTotal, order, GetSampleOrderLines(), isSeparateMaterialAndFreightItemsEnabled);

            // Assert
            order.SalesTaxRate.ShouldBe(10);
            order.SalesTax.ShouldBe(125.0M);
        }

        [Fact]
        public void Test_CalculateTax_for_TaxCalculationType_MaterialTotal()
        {
            // Arrange
            var order = GetSampleOrder();
            var isSeparateMaterialAndFreightItemsEnabled = false;

            // Act
            OrderTaxCalculator.CalculateTotals(TaxCalculationType.MaterialTotal, order, GetSampleOrderLines(), isSeparateMaterialAndFreightItemsEnabled);

            // Assert
            order.SalesTaxRate.ShouldBe(10);
            order.SalesTax.ShouldBe(62.0M);
        }

        [Fact]
        public void Test_CalculateTax_for_TaxCalculationType_MaterialLineItemsTotal()
        {
            // Arrange
            var order = GetSampleOrder();
            var isSeparateMaterialAndFreightItemsEnabled = false;

            // Act
            OrderTaxCalculator.CalculateTotals(TaxCalculationType.MaterialLineItemsTotal, order, GetSampleOrderLines(), isSeparateMaterialAndFreightItemsEnabled);

            // Assert
            order.SalesTaxRate.ShouldBe(10);
            order.SalesTax.ShouldBe(65.0M);
        }

        [Fact]
        public void Test_CalculateTax_for_NonTaxable_items()
        {
            // Arrange
            var order = GetSampleOrder();
            var isSeparateMaterialAndFreightItemsEnabled = false;
            var orderLines = new List<OrderLineTaxDetailsDto>
            {
                new OrderLineTaxDetailsDto { FreightPrice = 30, MaterialPrice = 220, IsTaxable = false, IsMaterialTaxable = true, IsFreightTaxable = true },
                new OrderLineTaxDetailsDto { FreightPrice = 600, MaterialPrice = 0, IsTaxable = false, IsMaterialTaxable = true, IsFreightTaxable = true },
                new OrderLineTaxDetailsDto { FreightPrice = 0, MaterialPrice = 400, IsTaxable = false, IsMaterialTaxable = true, IsFreightTaxable = true },
            };

            // Act
            OrderTaxCalculator.CalculateTotals(TaxCalculationType.FreightAndMaterialTotal, order, orderLines, isSeparateMaterialAndFreightItemsEnabled);

            // Assert
            order.SalesTaxRate.ShouldBe(10);
            order.SalesTax.ShouldBe(0);
        }

        [Fact]
        public void Test_CalculateTax_for_NonTaxable_Material_items()
        {
            // Arrange
            var order = GetSampleOrder();
            var isSeparateMaterialAndFreightItemsEnabled = false;
            var orderLines = new List<OrderLineTaxDetailsDto>
            {
                new OrderLineTaxDetailsDto { FreightPrice = 30, MaterialPrice = 220, IsTaxable = true, IsMaterialTaxable = true, IsFreightTaxable = true },
                new OrderLineTaxDetailsDto { FreightPrice = 600, MaterialPrice = 0, IsTaxable = true, IsMaterialTaxable = true, IsFreightTaxable = true },
                new OrderLineTaxDetailsDto { FreightPrice = 0, MaterialPrice = 400, IsTaxable = false, IsMaterialTaxable = true, IsFreightTaxable = true },
            };

            // Act
            OrderTaxCalculator.CalculateTotals(TaxCalculationType.FreightAndMaterialTotal, order, orderLines, isSeparateMaterialAndFreightItemsEnabled);

            // Assert
            order.SalesTaxRate.ShouldBe(10);
            order.SalesTax.ShouldBe(85.0M);
        }
    }
}
