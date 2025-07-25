using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Abp.Application.Features;
using Abp.Configuration;
using Abp.Domain.Repositories;
using Abp.Linq.Extensions;
using Abp.Runtime.Session;
using Abp.Timing;
using DispatcherWeb.Authorization.Users;
using DispatcherWeb.Authorization.Users.Cache;
using DispatcherWeb.Configuration;
using DispatcherWeb.Dashboard.Dto;
using DispatcherWeb.Dashboard.RevenueGraph.Dto;
using DispatcherWeb.Features;
using DispatcherWeb.Orders;
using DispatcherWeb.Runtime.Session;
using Microsoft.EntityFrameworkCore;

namespace DispatcherWeb.Dashboard.RevenueGraph.DataItemsQueryServices
{
    public class RevenueGraphByTicketsDataItemsQueryService : IRevenueGraphByTicketsDataItemsQueryService
    {
        private readonly IRepository<OrderLine> _orderLineRepository;
        private readonly UserManager _userManager;
        private readonly IExtendedAbpSession _session;
        private readonly IFeatureChecker _featureChecker;
        private readonly ISettingManager _settingManager;

        public IUserOrganizationUnitCache UserOrganizationUnitCache { get; set; }

        public RevenueGraphByTicketsDataItemsQueryService(
            IRepository<OrderLine> orderLineRepository,
            UserManager userManager,
            IExtendedAbpSession session,
            IFeatureChecker featureChecker,
            ISettingManager settingManager
        )
        {
            _orderLineRepository = orderLineRepository;
            _userManager = userManager;
            _session = session;
            _featureChecker = featureChecker;
            _settingManager = settingManager;
        }

        public async Task<IEnumerable<RevenueGraphDataItem>> GetRevenueGraphDataItemsAsync(PeriodInput input)
        {
            bool showFuelSurcharge = await _settingManager.GetSettingValueAsync<bool>(AppSettings.Fuel.ShowFuelSurcharge);
            var timeZone = await _settingManager.GetSettingValueAsync(TimingSettingNames.TimeZone);
            var periodBeginUtc = input.PeriodBegin.ConvertTimeZoneFrom(timeZone);
            var periodEndUtc = input.PeriodEnd.ConvertTimeZoneFrom(timeZone).AddDays(1);
            var multiOfficeFeature = await _featureChecker.IsEnabledAsync(AppFeatures.AllowMultiOfficeFeature);

            List<long?> organizationUnitIds = null;

            if (input.OfficeId == null && multiOfficeFeature)
            {
                var organizationUnits = await UserOrganizationUnitCache.GetUserOrganizationUnitsAsync(_session.GetUserId());
                organizationUnitIds = organizationUnits.Select(x => (long?)x.OrganizationUnitId).ToList();

                //if (!organizationUnitIds.Any())
                //{
                //    throw new UserFriendlyException("You must have an assigned Office in User Details to use that function");
                //}
            }

            var items = await (await _orderLineRepository.GetQueryAsync())
                    .SelectMany(ol => ol.Tickets)
                    .Where(t => t.TicketDateTime >= periodBeginUtc)
                    .Where(t => t.TicketDateTime < periodEndUtc)
                    .WhereIf(input.TicketType == TicketType.InternalTrucks, t => t.CarrierId == null)
                    .WhereIf(input.TicketType == TicketType.LeaseHaulers, t => t.CarrierId != null)
                    .WhereIf(input.OfficeId == null && multiOfficeFeature, x => organizationUnitIds.Contains(x.Office.OrganizationUnitId))
                    .WhereIf(input.OfficeId != null && multiOfficeFeature, x => x.OfficeId == input.OfficeId)
                    .Select(t => new RevenueGraphDataItemFromTickets
                    {
                        DeliveryDate = t.TicketDateTime,
                        FreightPricePerUnit = t.OrderLine.FreightPricePerUnit,
                        MaterialPricePerUnit = t.OrderLine.MaterialPricePerUnit,
                        Designation = t.OrderLine.Designation,
                        OrderLineMaterialUomId = t.OrderLine.MaterialUomId,
                        OrderLineFreightUomId = t.OrderLine.FreightUomId,
                        TicketUomId = t.FreightUomId,
                        TicketMaterialQuantity = t.MaterialQuantity,
                        TicketFreightQuantity = t.FreightQuantity,
                        OrderLineId = t.OrderLineId,
                        FreightPriceOriginal = t.OrderLine.FreightPrice,
                        MaterialPriceOriginal = t.OrderLine.MaterialPrice,
                        IsFreightPriceOverridden = t.OrderLine.IsFreightPriceOverridden,
                        IsMaterialPriceOverridden = t.OrderLine.IsMaterialPriceOverridden,
                        FuelSurcharge = showFuelSurcharge ? t.FuelSurcharge : 0,
                        CarrierId = t.CarrierId,
                        CustomerName = t.Customer.Name,
                        TruckCode = t.Truck.TruckCode,
                        DriverName = t.Driver == null ? null : t.Driver.FirstName + " " + t.Driver.LastName,
                    }).ToListAsync();

            items.ForEach(x => x.DeliveryDate = x.DeliveryDate?.ConvertTimeZoneTo(timeZone).Date);

            var orderLineGroups = items.Where(x => x.OrderLineId.HasValue).GroupBy(x => x.OrderLineId);
            foreach (var group in orderLineGroups.ToList())
            {
                var count = group.Count();
                if (count == 1)
                {
                    continue;
                }
                if (group.First().IsMaterialPriceOverridden)
                {
                    foreach (var item in group)
                    {
                        item.MaterialPriceOriginal /= count;
                    }
                }
                if (group.First().IsFreightPriceOverridden)
                {
                    foreach (var item in group)
                    {
                        item.FreightPriceOriginal /= count;
                    }
                }
            }

            return items;
        }
    }
}
