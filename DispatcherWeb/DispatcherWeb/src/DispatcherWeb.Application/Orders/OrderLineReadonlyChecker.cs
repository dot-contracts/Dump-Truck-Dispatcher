using System.Linq;
using System.Threading.Tasks;
using Abp.Domain.Repositories;
using Abp.Localization;
using DispatcherWeb.Dispatching;
using DispatcherWeb.Infrastructure.EntityReadonlyCheckers;
using DispatcherWeb.Runtime.Session;
using DispatcherWeb.Sessions;
using Microsoft.EntityFrameworkCore;

namespace DispatcherWeb.Orders
{
    public class OrderLineReadonlyChecker : ReadonlyChecker<OrderLine>
    {
        private readonly int _orderLineId;
        private readonly IRepository<OrderLine> _orderLineRepository;
        private readonly IRepository<Dispatch> _dispatchRepository;
        private readonly IRepository<Drivers.EmployeeTime> _employeeTimeRepository;
        private readonly IRepository<Ticket> _ticketRepository;

        public IExtendedAbpSession Session { get; }

        public OrderLineReadonlyChecker(
            int orderLineId,
            ILocalizationManager localizationManager,
            IExtendedAbpSession session,
            IRepository<OrderLine> orderLineRepository,
            IRepository<Dispatch> dispatchRepository,
            IRepository<Drivers.EmployeeTime> employeeTimeRepository,
            IRepository<Ticket> ticketRepository
            ) : base(
                localizationManager
                )
        {
            _orderLineId = orderLineId;
            Session = session;
            _orderLineRepository = orderLineRepository;
            _dispatchRepository = dispatchRepository;
            _employeeTimeRepository = employeeTimeRepository;
            _ticketRepository = ticketRepository;
        }

        public override async Task<string> GetReadOnlyReasonForFieldAsync(string fieldName)
        {
            if (_orderLineId == 0)
            {
                //new order lines are not readonly
                return null;
            }

            OrderLine orderLine;
            switch (fieldName)
            {
                case nameof(orderLine.LoadAtId):
                case nameof(orderLine.FreightItemId):
                case nameof(orderLine.MaterialItemId):
                    if (await HasLoadedDispatchesAsync())
                    {
                        return L("CantEditOrderLineFieldsWithLoadedDispatches");
                    }
                    if (await HasManualTicketsAsync())
                    {
                        return L("OrderLine_Edit_Error_HasTickets");
                    }
                    break;
                case nameof(orderLine.DeliverToId):
                    if (await HasDeliveredDispatchesAsync())
                    {
                        return L("CantEditOrderLineFieldsWithDeliveredDispatches");
                    }
                    if (await HasManualTicketsAsync())
                    {
                        return L("OrderLine_Edit_Error_HasTickets");
                    }
                    break;
                case nameof(orderLine.MaterialUomId):
                case nameof(orderLine.FreightUomId):
                case nameof(orderLine.Designation):
                    if (await HasTicketsAsync() || await HasOpenDispatchesAsync())
                    {
                        return L("OrderLine_Edit_Error_HasTickets");
                    }
                    break;
                case nameof(orderLine.Note):
                case nameof(orderLine.FreightQuantity):
                case nameof(orderLine.MaterialQuantity):
                case nameof(orderLine.NumberOfTrucks):
                case nameof(orderLine.ScheduledTrucks):
                    //The quantities, number of trucks sched, number of trucks requested, can be changed at any time with no impact on the dispatches
                    //The notes can also be changed at any time. Notes will be updated on any dispatch that hasn't been delivered.
                    break;
            }

            switch (fieldName)
            {
                case nameof(orderLine.IsFreightPricePerUnitOverridden):
                case nameof(orderLine.FreightPricePerUnit):
                case nameof(orderLine.IsFreightRateToPayDriversOverridden):
                case nameof(orderLine.FreightRateToPayDrivers):
                case nameof(orderLine.IsFreightPriceOverridden):
                case nameof(orderLine.DriverPayTimeClassificationId):
                case nameof(orderLine.HourlyDriverPayRate):
                case nameof(orderLine.FreightPrice):
                case nameof(orderLine.IsMaterialPricePerUnitOverridden):
                case nameof(orderLine.MaterialPricePerUnit):
                case nameof(orderLine.IsMaterialPriceOverridden):
                case nameof(orderLine.MaterialPrice):
                    if (await IsOrderPaidAsync())
                    {
                        return L("CannotChangeRatesAndAmountBecauseOrderIsPaid");
                    }
                    break;
            }

            switch (fieldName)
            {
                case nameof(orderLine.LeaseHaulerRate):
                case nameof(orderLine.MaterialPrice):
                case nameof(orderLine.FreightPrice):
                case nameof(orderLine.FreightPricePerUnit):
                case nameof(orderLine.FreightRateToPayDrivers):
                case nameof(orderLine.MaterialPricePerUnit):
                    if (await HasTicketsWithStatementsAsync())
                    {
                        return L("CannotChangeRateBecauseTicketsAreAssociatedWithPayStatementOrInvoice");
                    }
                    break;
            }

            switch (fieldName)
            {
                case nameof(orderLine.DriverPayTimeClassificationId):
                case nameof(orderLine.HourlyDriverPayRate):
                    if (await HasEmployeeTimeRecordsAsync())
                    {
                        return L("CannotChangeDriverPayRateBecauseEmployeeTimeRecordsExist");
                    }
                    break;
            }

            //switch (fieldName)
            //{
            //    case nameof(orderLine.IsFreightPriceOverridden):
            //        orderLine = await GetEntityAsync();
            //        if (!orderLine.IsFreightPriceOverridden)
            //        {
            //            if (await HasMultipleTicketsOrDispatchesOrTrucks() || orderLine.IsMultipleLoads)
            //            {
            //                return L("OrderLineWithOverriddenTotalCanOnlyHaveSingleTicketError");
            //            }
            //        }
            //        break;

            return null;
        }

        protected override async Task<OrderLine> GetEntityAsync()
        {
            return _entity ??= _orderLineId == 0 ? new OrderLine() : await _orderLineRepository.GetAsync(_orderLineId);
        }

        private bool? _hasOpenDispatches = null;
        private async Task<bool> HasOpenDispatchesAsync()
        {
            return _hasOpenDispatches ??= await (await _dispatchRepository.GetQueryAsync())
                .AnyAsync(x => x.OrderLineId == _orderLineId
                    && Dispatch.OpenStatuses.Contains(x.Status));
        }

        private bool? _hasLoadedDispatches = null;
        private async Task<bool> HasLoadedDispatchesAsync()
        {
            return _hasLoadedDispatches ??= await (await _dispatchRepository.GetQueryAsync())
                .AnyAsync(x => x.OrderLineId == _orderLineId
                    && (x.Status == DispatchStatus.Loaded || x.Status == DispatchStatus.Completed));
        }

        private bool? _hasDeliveredDispatches = null;
        private async Task<bool> HasDeliveredDispatchesAsync()
        {
            return _hasDeliveredDispatches ??= await (await _dispatchRepository.GetQueryAsync())
                .AnyAsync(x => x.OrderLineId == _orderLineId
                    && x.Status == DispatchStatus.Completed);
        }

        private bool? _hasManualTickets = null;
        private async Task<bool> HasManualTicketsAsync()
        {
            return _hasManualTickets ??= await (await _ticketRepository.GetQueryAsync())
                .AnyAsync(x => x.OrderLineId == _orderLineId
                    && x.Load == null
                );
        }

        private bool? _hasTickets = null;
        private async Task<bool> HasTicketsAsync()
        {
            return _hasTickets ??= await (await _ticketRepository.GetQueryAsync())
                .AnyAsync(x => x.OrderLineId == _orderLineId);
        }


        private bool? _isOrderPaid = null;
        private async Task<bool> IsOrderPaidAsync()
        {
            return _isOrderPaid ??= await (await _orderLineRepository.GetQueryAsync())
                .Where(x => x.Id == _orderLineId)
                .Select(x => x.Order)
                .AnyAsync(o => o.OrderPayments
                                .Where(op => op.OfficeId == Session.GetOfficeIdOrThrow())
                                .Select(x => x.Payment)
                                .Any(x => !x.IsCancelledOrRefunded && x.AuthorizationCaptureDateTime.HasValue));
        }

        private bool? _hasTicketsWithStatements = null;
        private async Task<bool> HasTicketsWithStatementsAsync()
        {
            return _hasTicketsWithStatements ??= await (await _ticketRepository.GetQueryAsync())
                .AnyAsync(t => t.OrderLineId == _orderLineId
                        && (t.LeaseHaulerStatementTicket != null
                            || t.InvoiceLine != null
                            || t.PayStatementTickets.Any()));
        }

        private bool? _hasEmployeeTimeRecords = null;
        private async Task<bool> HasEmployeeTimeRecordsAsync()
        {
            return _hasEmployeeTimeRecords ??= await (await _employeeTimeRepository.GetQueryAsync())
                .AnyAsync(x => x.OrderLineId == _orderLineId);
        }
    }
}
