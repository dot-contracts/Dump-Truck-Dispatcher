using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Abp.Application.Features;
using Abp.Collections.Extensions;
using Abp.Configuration;
using Abp.Domain.Repositories;
using Abp.Linq.Extensions;
using Abp.Timing;
using DispatcherWeb.Orders.RevenueBreakdownByTruckReport.Dto;
using Microsoft.EntityFrameworkCore;

namespace DispatcherWeb.Orders.RevenueBreakdownByTruckReport
{
    public class RevenueBreakdownByTruckReportByTicketsDataService : IRevenueBreakdownByTruckReportByTicketsDataService
    {
        private readonly IRepository<OrderLine> _orderLineRepository;
        private readonly IFeatureChecker _featureChecker;
        private readonly ISettingManager _settingManager;

        public RevenueBreakdownByTruckReportByTicketsDataService(
            IRepository<OrderLine> orderLineRepository,
            IFeatureChecker featureChecker,
            ISettingManager settingManager
        )
        {
            _orderLineRepository = orderLineRepository;
            _featureChecker = featureChecker;
            _settingManager = settingManager;
        }

        public async Task<List<RevenueBreakdownByTruckItem>> GetRevenueBreakdownItems(RevenueBreakdownByTruckReportInput input)
        {
            var timezone = await _settingManager.GetSettingValueAsync(TimingSettingNames.TimeZone);
            var deliveryDateBegin = input.DeliveryDateBegin.ConvertTimeZoneFrom(timezone);
            var deliveryDateEnd = input.DeliveryDateEnd.ConvertTimeZoneFrom(timezone).AddDays(1);

            var items = await (await _orderLineRepository.GetQueryAsync())
                .Where(ol => ol.Tickets.Any(t => t.TruckId != null))
                .WhereIf(!input.Shifts.IsNullOrEmpty() && !input.Shifts.Contains(Shift.NoShift), ol => ol.Order.Shift.HasValue && input.Shifts.Contains(ol.Order.Shift.Value))
                .WhereIf(!input.Shifts.IsNullOrEmpty() && input.Shifts.Contains(Shift.NoShift), ol => !ol.Order.Shift.HasValue || input.Shifts.Contains(ol.Order.Shift.Value))
                .SelectMany(ol => ol.Tickets)
                .Where(t => t.TicketDateTime >= deliveryDateBegin)
                .Where(t => t.TicketDateTime < deliveryDateEnd)
                .WhereIf(input.OfficeId.HasValue, t => t.OfficeId == input.OfficeId.Value)
                .WhereIf(!input.TruckIds.IsNullOrEmpty(), t => input.TruckIds.Contains(t.TruckId.Value))
                .Select(t => new RevenueBreakdownByTruckItemRaw
                {
                    TicketDateTime = t.TicketDateTime,
                    Shift = t.OrderLine.Order.Shift,
                    TruckId = t.TruckId,
                    TruckCode = t.Truck.TruckCode,
                    //ReceiptFreightTotal = t.ReceiptLine == null ? (decimal?)null : t.ReceiptLine.FreightAmount,
                    //ReceiptMaterialTotal = t.ReceiptLine == null ? (decimal?)null : t.ReceiptLine.MaterialAmount,
                    FreightPricePerUnit = t.OrderLine.FreightPricePerUnit,
                    MaterialPricePerUnit = t.OrderLine.MaterialPricePerUnit,
                    FreightQuantity = t.FreightQuantity,
                    MaterialQuantity = t.MaterialQuantity,
                    Designation = t.OrderLine.Designation,
                    OrderLineMaterialUomId = t.OrderLine.MaterialUomId,
                    OrderLineFreightUomId = t.OrderLine.FreightUomId,
                    TicketUomId = t.FreightUomId,
                    FreightPriceOriginal = t.OrderLine.FreightPrice,
                    MaterialPriceOriginal = t.OrderLine.MaterialPrice,
                    IsFreightPriceOverridden = t.OrderLine.IsFreightPriceOverridden,
                    IsMaterialPriceOverridden = t.OrderLine.IsMaterialPriceOverridden,
                    OrderLineTicketsFreightQuantitySum = t.OrderLine == null ? 0 : t.OrderLine.Tickets.Select(x => x.FreightQuantity).Sum(),
                    OrderLineTicketsMaterialQuantitySum = t.OrderLine == null ? 0 : t.OrderLine.Tickets.Select(x => x.MaterialQuantity).Sum(),
                    FuelSurcharge = t.FuelSurcharge,
                })
                .ToListAsync();

            return items
                .GroupBy(olt => new { TicketDate = olt.TicketDateTime?.ConvertTimeZoneTo(timezone).Date, olt.Shift, olt.TruckId, olt.TruckCode })
                .Select(g => new RevenueBreakdownByTruckItem
                {
                    TicketDate = g.Key.TicketDate,
                    Shift = g.Key.Shift,
                    Truck = g.Key.TruckCode,
                    TruckId = g.Key.TruckId,
                    MaterialRevenue = g.Sum(t => t.IsMaterialPriceOverridden
                        ? decimal.Round(t.MaterialPriceOriginal * t.PercentMaterialQtyForTicket, 2)
                        : decimal.Round((t.MaterialPricePerUnit ?? 0) * t.ActualMaterialQuantity, 2)),
                    FreightRevenue = g.Sum(t => t.IsFreightPriceOverridden
                        ? decimal.Round(t.FreightPriceOriginal * t.PercentFreightQtyForTicket, 2)
                        : decimal.Round((t.FreightPricePerUnit ?? 0) * t.ActualFreightQuantity, 2)),
                    FuelSurcharge = g.Sum(t => t.FuelSurcharge),
                })
                .ToList();
        }
    }
}
