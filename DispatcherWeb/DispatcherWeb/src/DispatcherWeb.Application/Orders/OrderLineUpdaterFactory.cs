using System;
using Abp.Dependency;
using Abp.Domain.Repositories;
using Abp.Domain.Uow;
using Abp.Localization;
using Abp.Runtime.Session;
using DispatcherWeb.Dispatching;
using DispatcherWeb.FuelSurchargeCalculations;
using DispatcherWeb.Infrastructure.EntityReadonlyCheckers;
using DispatcherWeb.Infrastructure.EntityUpdaters;
using DispatcherWeb.LeaseHaulers;
using DispatcherWeb.SyncRequests;

namespace DispatcherWeb.Orders
{
    public class OrderLineUpdaterFactory : IOrderLineUpdaterFactory, IEntityUpdaterFactory<OrderLine>, ITransientDependency
    {
        private readonly IReadonlyCheckerFactory<OrderLine> _orderLineReadonlyCheckerFactory;
        private readonly ILocalizationManager _localizationManager;
        private readonly IUnitOfWorkManager _unitOfWorkManager;
        private readonly IAbpSession _abpSession;
        private readonly ISyncRequestSender _syncRequestSender;
        private readonly IOrderLineScheduledTrucksUpdater _orderLineScheduledTrucksUpdater;
        private readonly IRepository<OrderLine> _orderLineRepository;
        private readonly IRepository<Order> _orderRepository;
        private readonly IRepository<Dispatch> _dispatchRepository;
        private readonly IRepository<OrderLineTruck> _orderLineTruckRepository;
        private readonly IFuelSurchargeCalculator _fuelSurchargeCalculator;
        private readonly ICrossTenantOrderSender _crossTenantOrderSender;

        public OrderLineUpdaterFactory(
            IReadonlyCheckerFactory<OrderLine> orderLineReadonlyCheckerFactory,
            ILocalizationManager localizationManager,
            IUnitOfWorkManager unitOfWorkManager,
            IAbpSession abpSession,
            ISyncRequestSender syncRequestSender,
            IOrderLineScheduledTrucksUpdater orderLineScheduledTrucksUpdater,
            IRepository<OrderLine> orderLineRepository,
            IRepository<Order> orderRepository,
            IRepository<Dispatch> dispatchRepository,
            IRepository<OrderLineTruck> orderLineTruckRepository,
            IFuelSurchargeCalculator fuelSurchargeCalculator,
            ICrossTenantOrderSender crossTenantOrderSender
            )
        {
            _orderLineReadonlyCheckerFactory = orderLineReadonlyCheckerFactory;
            _localizationManager = localizationManager;
            _unitOfWorkManager = unitOfWorkManager;
            _abpSession = abpSession;
            _syncRequestSender = syncRequestSender;
            _orderLineScheduledTrucksUpdater = orderLineScheduledTrucksUpdater;
            _orderLineRepository = orderLineRepository;
            _orderRepository = orderRepository;
            _dispatchRepository = dispatchRepository;
            _orderLineTruckRepository = orderLineTruckRepository;
            _fuelSurchargeCalculator = fuelSurchargeCalculator;
            _crossTenantOrderSender = crossTenantOrderSender;
        }

        IEntityUpdater<OrderLine> IEntityUpdaterFactory<OrderLine>.Create(int entityId)
        {
            return Create(entityId);
        }

        public IOrderLineUpdater Create(int entityId, int[] alreadySyncedOrderLineIds = null)
        {
            return new OrderLineUpdater(
                entityId,
                alreadySyncedOrderLineIds ?? Array.Empty<int>(),
                this,
                _orderLineReadonlyCheckerFactory,
                _localizationManager,
                _unitOfWorkManager,
                _abpSession,
                _syncRequestSender,
                _orderLineScheduledTrucksUpdater,
                _orderLineRepository,
                _orderRepository,
                _dispatchRepository,
                _orderLineTruckRepository,
                _fuelSurchargeCalculator,
                _crossTenantOrderSender
                );
        }
    }
}
