using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Abp.Application.Features;
using Abp.Configuration;
using Abp.Dependency;
using Abp.Domain.Repositories;
using Abp.UI;
using DispatcherWeb.Configuration;
using DispatcherWeb.Features;
using DispatcherWeb.Orders.TaxDetails;
using Microsoft.EntityFrameworkCore;

namespace DispatcherWeb.Orders
{
    public class OrderTaxCalculator : ITransientDependency
    {
        private readonly ISettingManager _settingManager;
        private readonly IRepository<Order> _orderRepository;
        private readonly IRepository<OrderLine> _orderLineRepository;
        private readonly IRepository<Receipt> _receiptRepository;
        private readonly IRepository<ReceiptLine> _receiptLineRepository;
        private readonly IFeatureChecker _featureChecker;

        public OrderTaxCalculator(
            ISettingManager settingManager,
            IRepository<Order> orderRepository,
            IRepository<OrderLine> orderLineRepository,
            IRepository<Receipt> receiptRepository,
            IRepository<ReceiptLine> receiptLineRepository,
            IFeatureChecker featureChecker)
        {
            _settingManager = settingManager;
            _orderRepository = orderRepository;
            _orderLineRepository = orderLineRepository;
            _receiptRepository = receiptRepository;
            _receiptLineRepository = receiptLineRepository;
            _featureChecker = featureChecker;
        }

        public async Task<IOrderTaxDetails> CalculateTotalsForReceiptLineAsync(int receiptLineId)
        {
            var receiptDetails = await (await _receiptLineRepository.GetQueryAsync())
                .Where(x => x.Id == receiptLineId)
                .Select(x => new
                {
                    x.ReceiptId,
                })
                .FirstOrDefaultAsync();

            return await CalculateReceiptTotalsAsync(receiptDetails.ReceiptId);
        }

        public async Task<IOrderTaxDetails> CalculateReceiptTotalsAsync(int receiptId)
        {
            var receipt = await (await _receiptRepository.GetQueryAsync())
                .Where(x => x.Id == receiptId)
                .FirstOrDefaultAsync();

            var receiptLines = await (await _receiptLineRepository.GetQueryAsync())
                .Where(x => x.ReceiptId == receiptId)
                .Select(x => new OrderLineTaxDetailsDto
                {
                    IsTaxable = x.FreightItem.IsTaxable,
                    IsMaterialTaxable = x.MaterialItem.IsTaxable,
                    IsFreightTaxable = x.FreightItem.IsTaxable,
                    FreightPrice = x.FreightAmount,
                    MaterialPrice = x.MaterialAmount,
                })
                .ToListAsync();

            await CalculateTotalsAsync(receipt, receiptLines);

            return receipt;
        }

        public async Task<IOrderTaxDetails> CalculateTotalsForOrderLineAsync(int orderLineId)
        {
            var orderDetails = await (await _orderLineRepository.GetQueryAsync())
                .Where(x => x.Id == orderLineId)
                .Select(x => new
                {
                    x.OrderId,
                })
                .FirstOrDefaultAsync();

            return await CalculateTotalsAsync(orderDetails.OrderId);
        }

        public async Task<IOrderTaxDetails> CalculateTotalsAsync(int orderId)
        {
            var order = await (await _orderRepository.GetQueryAsync())
                .Where(x => x.Id == orderId)
                .FirstOrDefaultAsync();

            var orderLineDtos = await (await _orderLineRepository.GetQueryAsync())
                .Where(x => x.OrderId == orderId)
                .Select(x => new OrderLineTaxDetailsDto
                {
                    IsTaxable = x.FreightItem.IsTaxable,
                    IsFreightTaxable = x.FreightItem.IsTaxable,
                    IsMaterialTaxable = x.MaterialItem.IsTaxable,
                    FreightPrice = x.FreightPrice,
                    MaterialPrice = x.MaterialPrice,
                })
                .ToListAsync();

            await CalculateTotalsAsync(order, orderLineDtos);

            return order;
        }

        public async Task<TaxCalculationType> GetTaxCalculationTypeAsync(int tenantId)
        {
            return (TaxCalculationType)await _settingManager.GetSettingValueForTenantAsync<int>(AppSettings.Invoice.TaxCalculationType, tenantId);
        }

        public async Task<TaxCalculationType> GetTaxCalculationTypeAsync()
        {
            return (TaxCalculationType)await _settingManager.GetSettingValueAsync<int>(AppSettings.Invoice.TaxCalculationType);
        }

        public async Task CalculateTotalsAsync(IOrderTaxDetails order, IEnumerable<IOrderLineTaxDetails> orderLines)
        {
            var taxCalculationType = await GetTaxCalculationTypeAsync();
            var separateItems = await _featureChecker.IsEnabledAsync(AppFeatures.SeparateMaterialAndFreightItems);

            CalculateTotals(taxCalculationType, order, orderLines, separateItems);
        }


        public static void CalculateSingleOrderLineTotals(TaxCalculationType taxCalculationType, IOrderLineTaxTotalDetails orderLine, decimal salesTaxRate, bool separateItems)
        {
            var freightTotal = Math.Round(orderLine.FreightPrice, 2);
            var materialTotal = Math.Round(orderLine.MaterialPrice, 2);
            var taxableTotal = 0M;
            var taxRate = salesTaxRate / 100;
            var salesTax = 0M;
            var orderLineTotal = 0M;
            var subtotal = materialTotal + freightTotal;

            if (!separateItems)
            {
                switch (taxCalculationType)
                {
                    case TaxCalculationType.FreightAndMaterialTotal:
                        taxableTotal = materialTotal + freightTotal;
                        break;

                    case TaxCalculationType.MaterialLineItemsTotal:
                        taxableTotal = materialTotal > 0 ? materialTotal + freightTotal : 0;
                        break;

                    case TaxCalculationType.MaterialTotal:
                        taxableTotal = materialTotal;
                        break;

                    case TaxCalculationType.NoCalculation:
                        taxRate = 0;
                        salesTax = 0;
                        break;
                }

                if (orderLine.IsTaxable != true || taxableTotal < 0)
                {
                    taxableTotal = 0;
                }

                switch (taxCalculationType)
                {
                    case TaxCalculationType.FreightAndMaterialTotal:
                    case TaxCalculationType.MaterialLineItemsTotal:
                    case TaxCalculationType.MaterialTotal:
                        salesTax = taxableTotal * taxRate;
                        orderLineTotal = subtotal + taxableTotal * taxRate;
                        break;

                    case TaxCalculationType.NoCalculation:
                        //salesTax = Math.Round(salesTax, 2);
                        //orderLineTotal = Math.Round(subtotal + salesTax, 2);
                        orderLineTotal = subtotal;
                        break;
                }

                //var totalsToCheck = new[] { order.FreightTotal, order.MaterialTotal, order.SalesTax, order.CODTotal };
                //var maxValue = AppConsts.MaxDecimalDatabaseLength;
                //if (totalsToCheck.Any(x => x > maxValue))
                //{
                //    throw new UserFriendlyException("The value is too big", "One or more totals exceeded the maximum allowed value. Please decrease some of the values so that the total doesn't exceed " + maxValue);
                //}

                orderLine.Subtotal = subtotal;
                orderLine.TotalAmount = orderLineTotal;
                orderLine.Tax = salesTax;
            }
            else
            {
                if (orderLine.IsMaterialTaxable == true && materialTotal > 0)
                {
                    taxableTotal += materialTotal;
                }
                if (orderLine.IsFreightTaxable == true && freightTotal > 0)
                {
                    taxableTotal += freightTotal;
                }

                orderLine.Subtotal = subtotal;
                orderLine.TotalAmount = subtotal + taxableTotal * taxRate;
                orderLine.Tax = taxableTotal * taxRate;
            }
        }

        public static void CalculateTotals(TaxCalculationType taxCalculationType, IOrderTaxDetails order, IEnumerable<IOrderLineTaxDetails> orderLines, bool separateItems)
        {
            order.FreightTotal = Math.Round(orderLines.Sum(ol => ol.FreightPrice), 2);
            order.MaterialTotal = Math.Round(orderLines.Sum(ol => ol.MaterialPrice), 2);
            //order.FuelSurcharge = Math.Round(order.FreightTotal * (order.FuelSurchargeRate / 100), 2);

            var subtotal = order.MaterialTotal + order.FreightTotal; //+ order.FuelSurcharge;
            var taxableTotal = 0M;
            var taxRate = order.SalesTaxRate / 100;

            if (!separateItems)
            {
                switch (taxCalculationType)
                {
                    case TaxCalculationType.FreightAndMaterialTotal:
                        taxableTotal = orderLines.Where(x => x.IsTaxable == true).Sum(x => x.MaterialPrice + x.FreightPrice);
                        break;

                    case TaxCalculationType.MaterialLineItemsTotal:
                        taxableTotal = orderLines.Where(x => x.MaterialPrice > 0 && x.IsTaxable == true).Sum(x => x.MaterialPrice + x.FreightPrice);
                        break;

                    case TaxCalculationType.MaterialTotal:
                        taxableTotal = orderLines.Where(x => x.IsTaxable == true).Sum(x => x.MaterialPrice);
                        break;

                    case TaxCalculationType.NoCalculation:
                        taxRate = 0;
                        order.SalesTaxRate = 0;
                        break;
                }

                if (taxableTotal < 0)
                {
                    taxableTotal = 0;
                }

                switch (taxCalculationType)
                {
                    case TaxCalculationType.FreightAndMaterialTotal:
                    case TaxCalculationType.MaterialLineItemsTotal:
                    case TaxCalculationType.MaterialTotal:
                        order.SalesTax = Math.Round(taxableTotal * taxRate, 2);
                        order.CODTotal = Math.Round(subtotal + taxableTotal * taxRate, 2);
                        break;

                    case TaxCalculationType.NoCalculation:
                        order.SalesTax = Math.Round(order.SalesTax, 2);
                        order.CODTotal = Math.Round(subtotal + order.SalesTax, 2);
                        break;
                }
            }
            else
            {
                taxableTotal = orderLines.Sum(x => (x.IsMaterialTaxable == true ? x.MaterialPrice : 0)
                                                   + (x.IsFreightTaxable == true ? x.FreightPrice : 0));
                order.SalesTax = Math.Round(taxableTotal * taxRate, 2);
                order.CODTotal = Math.Round(subtotal + taxableTotal * taxRate, 2);
            }

            var totalsToCheck = new[]
            {
                order.FreightTotal,
                order.MaterialTotal,
                order.SalesTax,
                order.CODTotal,
            };
            var maxValue = AppConsts.MaxDecimalDatabaseLength;
            if (totalsToCheck.Any(x => x > maxValue))
            {
                throw new UserFriendlyException("The value is too big",
                    "One or more totals exceeded the maximum allowed value. Please decrease some of the values so that the total doesn't exceed " +
                    maxValue);
            }
        }
    }
}
