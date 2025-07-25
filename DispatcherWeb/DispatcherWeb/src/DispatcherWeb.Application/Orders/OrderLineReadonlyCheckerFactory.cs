using Abp.Dependency;
using Abp.Domain.Repositories;
using Abp.Localization;
using DispatcherWeb.Dispatching;
using DispatcherWeb.Infrastructure.EntityReadonlyCheckers;
using DispatcherWeb.Runtime.Session;

namespace DispatcherWeb.Orders
{
    public class OrderLineReadonlyCheckerFactory : IReadonlyCheckerFactory<OrderLine>, ITransientDependency
    {
        private readonly ILocalizationManager _localizationManager;
        private readonly IExtendedAbpSession _session;
        private readonly IRepository<OrderLine> _orderLineRepository;
        private readonly IRepository<Dispatch> _dispatchRepository;
        private readonly IRepository<Drivers.EmployeeTime> _employeeTimeRepository;
        private readonly IRepository<Ticket> _ticketRepository;

        public OrderLineReadonlyCheckerFactory(
            ILocalizationManager localizationManager,
            IExtendedAbpSession session,
            IRepository<OrderLine> orderLineRepository,
            IRepository<Dispatch> dispatchRepository,
            IRepository<Drivers.EmployeeTime> employeeTimeRepository,
            IRepository<Ticket> ticketRepository
            )
        {
            _localizationManager = localizationManager;
            _session = session;
            _orderLineRepository = orderLineRepository;
            _dispatchRepository = dispatchRepository;
            _employeeTimeRepository = employeeTimeRepository;
            _ticketRepository = ticketRepository;
        }

        public IReadonlyChecker<OrderLine> Create(int entityId)
        {
            return new OrderLineReadonlyChecker(
                entityId,
                _localizationManager,
                _session,
                _orderLineRepository,
                _dispatchRepository,
                _employeeTimeRepository,
                _ticketRepository
                );
        }
    }
}
