using System;
using System.Linq;
using System.Threading.Tasks;
using Abp.Application.Features;
using Abp.Configuration;
using Abp.Dependency;
using Abp.Domain.Repositories;
using DispatcherWeb.Configuration;
using DispatcherWeb.Features;
using DispatcherWeb.Orders;
using DispatcherWeb.Tickets.Dto;
using Microsoft.EntityFrameworkCore;

namespace DispatcherWeb.Tickets
{
    public class TicketQuantityHelper : ITicketQuantityHelper, ITransientDependency
    {
        private readonly IRepository<OrderLine> _orderLineRepository;
        private readonly ISettingManager _settingManager;
        private readonly IFeatureChecker _featureChecker;

        public TicketQuantityHelper(
            IRepository<OrderLine> orderLineRepository,
            ISettingManager settingManager,
            IFeatureChecker featureChecker
        )
        {
            _orderLineRepository = orderLineRepository;
            _settingManager = settingManager;
            _featureChecker = featureChecker;
        }

        public async Task<TicketOrderLineDetailsDto> GetTicketOrderLineDetails(int orderLineId)
        {
            var orderLine = await (await _orderLineRepository.GetQueryAsync())
                .Where(x => x.Id == orderLineId)
                .Select(x => new TicketOrderLineDetailsDto
                {
                    Designation = x.Designation,
                    FreightQuantity = x.FreightQuantity,
                    MaterialQuantity = x.MaterialQuantity,
                    FreightUomId = x.FreightUomId,
                    FreightUomBaseId = (UnitOfMeasureBaseEnum?)x.FreightUom.UnitOfMeasureBaseId,
                    MaterialUomId = x.MaterialUomId,
                    MaterialItemId = x.MaterialItemId,
                    FreightItemId = x.FreightItemId,
                })
                .FirstAsync();

            orderLine.CalculateMinimumFreightAmount = await _settingManager.GetSettingValueAsync<bool>(AppSettings.Invoice.CalculateMinimumFreightAmount);
            orderLine.MinimumFreightAmount = await GetMinimumFreightAmount(orderLine.FreightUomBaseId);

            return orderLine;
        }

        public async Task<decimal> GetMinimumFreightAmount(UnitOfMeasureBaseEnum? freightUomBaseId)
        {
            switch (freightUomBaseId)
            {
                case UnitOfMeasureBaseEnum.Tons:
                    return await _settingManager.GetSettingValueAsync<decimal>(AppSettings.Invoice.MinimumFreightAmountForTons);

                case UnitOfMeasureBaseEnum.Hours:
                    return await _settingManager.GetSettingValueAsync<decimal>(AppSettings.Invoice.MinimumFreightAmountForHours);
            }

            return 0;
        }

        public async Task<TicketControlVisibilityDto> GetVisibleTicketControls(int orderLineId)
        {
            var separateItems = await _featureChecker.IsEnabledAsync(AppFeatures.SeparateMaterialAndFreightItems);
            var orderLine = await GetTicketOrderLineDetails(orderLineId);
            return GetVisibleTicketControls(separateItems, orderLine);
        }

        public static TicketControlVisibilityDto GetVisibleTicketControls(bool separateItems, TicketOrderLineDetailsDto orderLine)
        {
            var visibility = new TicketControlVisibilityDto
            {
                Quantity = false, //deprecated
                FreightItem = !separateItems, //if separateItems == true, this will always be populated from order line, otherwise should always be visible
                MaterialItem = separateItems && orderLine.Designation == DesignationEnum.FreightOnly && orderLine.MaterialItemId == null, //only visible when material item is optional and is not specified
                FreightQuantity = orderLine.Designation != DesignationEnum.MaterialOnly && orderLine.FreightUomId != orderLine.MaterialUomId && orderLine.MaterialUomId.HasValue, //only visible when UOMs don't match (and freight is available)
                MaterialQuantity = true, //always visible
            };

            visibility.MaterialUom = visibility.MaterialQuantity; //UOM visibility will always match the visibility of the respective quantity control
            visibility.FreightUom = visibility.FreightQuantity;

            return visibility;
        }

        public async Task SetTicketQuantity(Ticket ticket, ITicketEditQuantity model)
        {
            var separateItems = await _featureChecker.IsEnabledAsync(AppFeatures.SeparateMaterialAndFreightItems);
            if (ticket.OrderLineId == null)
            {
                throw new ArgumentNullException(nameof(ticket.OrderLineId));
            }
            var orderLine = await GetTicketOrderLineDetails(ticket.OrderLineId.Value);
            SetTicketQuantity(ticket, model, orderLine, separateItems);
        }

        public static void SetTicketQuantity(Ticket ticket, ITicketEditQuantity model, TicketOrderLineDetailsDto orderLine, bool separateItems)
        {
            var visibleControls = GetVisibleTicketControls(separateItems, orderLine);

            if (!separateItems)
            {
                ticket.FreightItemId = model.FreightItemId ?? orderLine.FreightItemId;
            }
            else
            {
                ticket.FreightItemId = orderLine.FreightItemId;
                ticket.MaterialItemId = model.MaterialItemId ?? orderLine.MaterialItemId;
            }


            //ticket.Quantity = 0;

            if (visibleControls.FreightQuantity)
            {
                ticket.FreightQuantity = model.FreightQuantity ?? 0;
            }
            else if (orderLine.Designation == DesignationEnum.MaterialOnly)
            {
                ticket.FreightQuantity = 0;
            }
            else if (orderLine.CalculateMinimumFreightAmount && orderLine.FreightQuantity is 0 or null && orderLine.MinimumFreightAmount > 0)
            {
                ticket.FreightQuantity = Math.Max(orderLine.MinimumFreightAmount, model.MaterialQuantity ?? 0);
            }
            else if (orderLine.CalculateMinimumFreightAmount && orderLine.MaterialQuantity > 0 && orderLine.MaterialQuantity < orderLine.FreightQuantity)
            {
                ticket.FreightQuantity = Math.Max(orderLine.FreightQuantity ?? 0, model.MaterialQuantity ?? 0);
            }
            else
            {
                ticket.FreightQuantity = model.MaterialQuantity ?? 0;
            }

            if (orderLine.CalculateMinimumFreightAmount
                && orderLine.Designation != DesignationEnum.MaterialOnly
                && orderLine.FreightUomId == orderLine.MaterialUomId)
            {
                ticket.FreightQuantity = Math.Max(ticket.FreightQuantity ?? 0, orderLine.MinimumFreightAmount);
            }

            ticket.MaterialQuantity = model.MaterialQuantity ?? 0;

            ticket.FreightUomId = orderLine.FreightUomId;
            ticket.MaterialUomId = orderLine.MaterialUomId;
        }
    }
}
