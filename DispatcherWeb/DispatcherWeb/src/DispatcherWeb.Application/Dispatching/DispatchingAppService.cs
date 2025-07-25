using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Abp.Authorization;
using Abp.BackgroundJobs;
using Abp.Domain.Repositories;
using Abp.Extensions;
using Abp.Linq.Extensions;
using Abp.Runtime.Session;
using Abp.Timing;
using Abp.UI;
using DispatcherWeb.Authorization;
using DispatcherWeb.BackgroundJobs;
using DispatcherWeb.Dispatching.Dto;
using DispatcherWeb.Dispatching.Exporting;
using DispatcherWeb.DriverApplication;
using DispatcherWeb.DriverMessages.Dto;
using DispatcherWeb.Drivers;
using DispatcherWeb.FuelSurchargeCalculations;
using DispatcherWeb.Infrastructure.Extensions;
using DispatcherWeb.Infrastructure.Sms;
using DispatcherWeb.Infrastructure.Sms.Dto;
using DispatcherWeb.Notifications;
using DispatcherWeb.Offices;
using DispatcherWeb.Orders;
using DispatcherWeb.Storage;
using DispatcherWeb.SyncRequests;
using DispatcherWeb.SyncRequests.Entities;
using DispatcherWeb.Tickets;
using DispatcherWeb.TimeClassifications;
using DispatcherWeb.Trucks;
using DispatcherWeb.Url;
using Microsoft.EntityFrameworkCore;
using Twilio.Exceptions;

namespace DispatcherWeb.Dispatching
{
    [AbpAuthorize]
    public partial class DispatchingAppService : DispatcherWebAppServiceBase, IDispatchingAppService
    {
        public const int MaxNumberOfDispatches = 99;
        private readonly IRepository<Truck> _truckRepository;
        private readonly IRepository<OrderLine> _orderLineRepository;
        private readonly IRepository<OrderLineTruck> _orderLineTruckRepository;
        private readonly IRepository<Dispatch> _dispatchRepository;
        private readonly IRepository<Load> _loadRepository;
        private readonly IRepository<Driver> _driverRepository;
        private readonly IRepository<DriverAssignment> _driverAssignmentRepository;
        private readonly IRepository<Ticket> _ticketRepository;
        private readonly IRepository<Drivers.EmployeeTime> _employeeTimeRepository;
        private readonly IRepository<Office> _officeRepository;
        private readonly IRepository<TimeClassification> _timeClassificationRepository;
        private readonly IRepository<DriverApplicationLog> _driverApplicationLogRepository;
        private readonly IRepository<DeferredBinaryObject, Guid> _deferredBinaryObjectRepository;
        private readonly ISmsSender _smsSender;
        private readonly IAppNotifier _appNotifier;
        private readonly IWebUrlService _webUrlService;
        private readonly IDispatchListCsvExporter _dispatchListCsvExporter;
        private readonly IBinaryObjectManager _binaryObjectManager;
        private readonly OrderTaxCalculator _orderTaxCalculator;
        private readonly IDriverApplicationLogger _driverApplicationLogger;
        private readonly IDriverApplicationAuthProvider _driverApplicationAuthProvider;
        private readonly ITicketQuantityHelper _ticketQuantityHelper;
        private readonly IPushSubscriptionManager _pushSubscriptionManager;
        private readonly ISyncRequestSender _syncRequestSender;
        private readonly IDispatchSender _dispatchSender;
        private readonly IBackgroundJobManager _backgroundJobManager;
        private readonly IFuelSurchargeCalculator _fuelSurchargeCalculator;

        public DispatchingAppService(
            IRepository<Truck> truckRepository,
            IRepository<OrderLine> orderLineRepository,
            IRepository<OrderLineTruck> orderLineTruckRepository,
            IRepository<Dispatch> dispatchRepository,
            IRepository<Load> loadRepository,
            IRepository<Driver> driverRepository,
            IRepository<DriverAssignment> driverAssignmentRepository,
            IRepository<Ticket> ticketRepository,
            IRepository<Drivers.EmployeeTime> employeeTimeRepository,
            IRepository<Office> officeRepository,
            IRepository<TimeClassification> timeClassificationRepository,
            IRepository<DriverApplicationLog> driverApplicationLogRepository,
            IRepository<DeferredBinaryObject, Guid> deferredBinaryObjectRepository,
            ISmsSender smsSender,
            IAppNotifier appNotifier,
            IWebUrlService webUrlService,
            IDispatchListCsvExporter dispatchListCsvExporter,
            IBinaryObjectManager binaryObjectManager,
            OrderTaxCalculator orderTaxCalculator,
            IDriverApplicationLogger driverApplicationLogger,
            IDriverApplicationAuthProvider driverApplicationAuthProvider,
            ITicketQuantityHelper ticketQuantityHelper,
            IPushSubscriptionManager pushSubscriptionManager,
            ISyncRequestSender syncRequestSender,
            IDispatchSender dispatchSender,
            IBackgroundJobManager backgroundJobManager,
            IFuelSurchargeCalculator fuelSurchargeCalculator
        )
        {
            _truckRepository = truckRepository;
            _orderLineRepository = orderLineRepository;
            _orderLineTruckRepository = orderLineTruckRepository;
            _dispatchRepository = dispatchRepository;
            _loadRepository = loadRepository;
            _driverRepository = driverRepository;
            _driverAssignmentRepository = driverAssignmentRepository;
            _ticketRepository = ticketRepository;
            _employeeTimeRepository = employeeTimeRepository;
            _officeRepository = officeRepository;
            _timeClassificationRepository = timeClassificationRepository;
            _driverApplicationLogRepository = driverApplicationLogRepository;
            _deferredBinaryObjectRepository = deferredBinaryObjectRepository;
            _smsSender = smsSender;
            _appNotifier = appNotifier;
            _webUrlService = webUrlService;
            _dispatchListCsvExporter = dispatchListCsvExporter;
            _binaryObjectManager = binaryObjectManager;
            _orderTaxCalculator = orderTaxCalculator;
            _driverApplicationLogger = driverApplicationLogger;
            _driverApplicationAuthProvider = driverApplicationAuthProvider;
            _ticketQuantityHelper = ticketQuantityHelper;
            _pushSubscriptionManager = pushSubscriptionManager;
            _syncRequestSender = syncRequestSender;
            _dispatchSender = dispatchSender;
            _backgroundJobManager = backgroundJobManager;
            _fuelSurchargeCalculator = fuelSurchargeCalculator;
        }

        [AbpAuthorize(AppPermissions.Pages_Dispatches_Edit)]
        public async Task<bool> CanAddDispatchBasedOnTime(CanAddDispatchBasedOnTimeInput input)
        {
            return await _dispatchSender.CanAddDispatchBasedOnTime(input);
        }

        [AbpAuthorize(AppPermissions.Pages_Dispatches_Edit)]
        public async Task<SendDispatchMessageDto> CreateSendDispatchMessageDto(int orderLineId, bool firstDispatchForDay = false)
        {
            return await _dispatchSender.CreateSendDispatchMessageDto(orderLineId, firstDispatchForDay);
        }

        [AbpAuthorize(AppPermissions.Pages_Dispatches_Edit)]
        [AbpAuthorize(AppPermissions.Pages_SendOrdersToDrivers)]
        public async Task SendOrdersToDrivers(SendOrdersToDriversInput input)
        {
            await _dispatchSender.SendOrdersToDrivers(input);
        }

        [AbpAuthorize(AppPermissions.Pages_Dispatches_Edit)]
        public async Task<bool> GetDispatchTruckStatus(int dispatchId)
        {
            var truckStatus = await (await _dispatchRepository.GetQueryAsync())
                .Where(d => d.Id == dispatchId)
                .Select(d => new
                {
                    d.Truck.IsOutOfService,
                    DriverAssignment = d.Truck.DriverAssignments.Where(da => da.Date == d.OrderLine.Order.DeliveryDate).FirstOrDefault(),
                })
                .FirstAsync();
            return !truckStatus.IsOutOfService
                   && (truckStatus.DriverAssignment == null || truckStatus.DriverAssignment.DriverId.HasValue);
        }

        [AbpAuthorize(AppPermissions.Pages_Dispatches_Edit)]
        public async Task DuplicateDispatch(DuplicateDispatchInput input)
        {
            DispatchSender.ValidateNumberOfDispatches(input.NumberOfDispatches);

            var dispatch = await (await _dispatchRepository.GetQueryAsync())
                .Where(d => d.Id == input.DispatchId)
                .Select(d => new
                {
                    d.TruckId,
                    d.DriverId,
                    d.UserId,
                    d.OrderLineId,
                    d.OrderLineTruckId,
                    d.PhoneNumber,
                    d.EmailAddress,
                    d.Message,
                    MultipleLoads = d.IsMultipleLoads,
                    d.OrderNotifyPreferredFormat,
                    d.TimeOnJob,
                    d.MaterialQuantity,
                    d.FreightQuantity,
                })
                .FirstAsync();

            await _dispatchSender.EnsureCanCreateDispatchAsync(dispatch.OrderLineId, 1, input.NumberOfDispatches, dispatch.MultipleLoads);

            var oldActiveDispatch = await GetFirstOpenDispatch(dispatch.DriverId);

            var affectedDispatches = new List<Dispatch>();

            for (int i = 0; i < input.NumberOfDispatches; i++)
            {
                var affectedDispatch = _dispatchSender.AddDispatch(new Dto.DispatchSender.DispatchEditDto
                {
                    TruckId = dispatch.TruckId,
                    DriverId = dispatch.DriverId,
                    UserId = dispatch.UserId,
                    OrderLineId = dispatch.OrderLineId,
                    OrderLineTruckId = dispatch.OrderLineTruckId,
                    PhoneNumber = dispatch.PhoneNumber,
                    EmailAddress = dispatch.EmailAddress,
                    OrderNotifyPreferredFormat = dispatch.OrderNotifyPreferredFormat,
                    Message = dispatch.Message,
                    IsMultipleLoads = dispatch.MultipleLoads,
                    WasMultipleLoads = dispatch.MultipleLoads,
                    Guid = Guid.NewGuid(),
                    Status = DispatchStatus.Created,
                    TimeOnJob = dispatch.TimeOnJob,
                    MaterialQuantity = dispatch.MaterialQuantity,
                    FreightQuantity = dispatch.FreightQuantity,
                });
                affectedDispatches.Add(affectedDispatch);
            }
            await _dispatchSender.CleanUp();

            var newActiveDispatch = await GetFirstOpenDispatch(dispatch.DriverId);

            await CurrentUnitOfWork.SaveChangesAsync();

            await _syncRequestSender.SendSyncRequest(new SyncRequest()
                    .AddChanges(EntityEnum.Dispatch, affectedDispatches.Select(x => x.ToChangedEntity()))
                    .AddLogMessage($"Duplicated dispatch {input.DispatchId}"));

            await _dispatchSender.SendSmsOrEmail(new SendSmsOrEmailInput
            {
                TruckId = dispatch.TruckId,
                DriverId = dispatch.DriverId,
                UserId = dispatch.UserId,
                PhoneNumber = dispatch.PhoneNumber,
                EmailAddress = dispatch.EmailAddress,
                OrderNotifyPreferredFormat = dispatch.OrderNotifyPreferredFormat,
                ActiveDispatchWasChanged = oldActiveDispatch?.Id != newActiveDispatch?.Id,
            });
        }

        [AbpAuthorize(AppPermissions.Pages_Dispatches_Edit)]
        public async Task SendDispatchMessageNonInteractive(SendDispatchMessageNonInteractiveInput input)
        {
            var dto = await CreateSendDispatchMessageDto(input.OrderLineId);
            await SendDispatchMessage(new SendDispatchMessageInput
            {
                OrderLineId = dto.OrderLineId,
                Message = dto.Message,
                OrderLineTruckIds = dto.OrderLineTrucks
                    .Where(x => input.SelectedOrderLineTruckId == null || x.OrderLineTruckId == input.SelectedOrderLineTruckId)
                    .Select(x => x.OrderLineTruckId)
                    .ToArray(),
                NumberOfDispatches = 1,
                IsMultipleLoads = false,
                AddDispatchBasedOnTime = false,
            });
        }

        [AbpAuthorize(AppPermissions.Pages_Dispatches_Edit)]
        public async Task SendDispatchMessage(SendDispatchMessageInput input)
        {
            await _dispatchSender.SendDispatchMessage(input);
        }

        private async Task<IQueryable<RawDispatchDto>> GetFirstOpenDispatchQuery(int driverId)
        {
            return (await _dispatchRepository.GetQueryAsync())
                .Where(d => d.DriverId == driverId && Dispatch.OpenStatuses.Contains(d.Status))
                .OrderByDescending(d => d.Status == DispatchStatus.Loaded)
                .ThenByDescending(d => d.Status == DispatchStatus.Acknowledged)
                .ThenByDescending(d => d.Status == DispatchStatus.Sent)
                .ThenBy(d => d.SortOrder)
                .ToRawDispatchDto();
        }

        private async Task<RawDispatchDto> GetFirstOpenDispatch(int driverId)
        {
            return await (await GetFirstOpenDispatchQuery(driverId))
                .FirstOrDefaultAsync(CancellationTokenProvider.Token);
        }

        private async Task<int?> GetFirstOpenDispatchId(int driverId)
        {
            return (await (await GetFirstOpenDispatchQuery(driverId))
                .Select(x => new
                {
                    x.Id,
                }).FirstOrDefaultAsync(CancellationTokenProvider.Token))
                ?.Id;
        }

        private async Task<Dictionary<int, int?>> GetFirstOpenDispatchIdPerDriver(IEnumerable<int> driverIds)
        {
            return await (await _dispatchRepository.GetQueryAsync())
                .Where(d => driverIds.Contains(d.DriverId) && Dispatch.OpenStatuses.Contains(d.Status))
                .GroupBy(d => d.DriverId)
                .Select(g => new
                {
                    DriverId = g.Key,
                    DispatchId = g
                        .OrderByDescending(d => d.Status == DispatchStatus.Loaded)
                        .ThenByDescending(d => d.Status == DispatchStatus.Acknowledged)
                        .ThenByDescending(d => d.Status == DispatchStatus.Sent)
                        .ThenBy(d => d.SortOrder)
                        .Select(x => (int?)x.Id)
                        .FirstOrDefault(),
                })
                .ToDictionaryAsync(g => g.DriverId, g => g.DispatchId);
        }

        private static void SetDispatchEntityStatusToCanceled(Dispatch dispatch)
        {
            dispatch.Status = DispatchStatus.Canceled;
            dispatch.Canceled = Clock.Now;
        }

        [AbpAuthorize(AppPermissions.Pages_Dispatches_Edit, AppPermissions.LeaseHaulerPortal_Schedule)]
        public async Task CancelDispatches(CancelDispatchesInput input)
        {
            var leaseHaulerIdFilter = await GetLeaseHaulerIdFilterAsync(AppPermissions.Pages_Dispatches_Edit, AppPermissions.LeaseHaulerPortal_Schedule);

            List<Dispatch> dispatchesToCancel;
            var query = (await _dispatchRepository.GetQueryAsync())
                .Where(x => Dispatch.OutstandingDispatchStatuses.Contains(x.Status)
                    || x.Status == DispatchStatus.Loaded && !x.IsMultipleLoads
                );
            if (input.OrderLineId.HasValue)
            {
                await CheckOrderLineEditPermissions(AppPermissions.Pages_Dispatches_Edit, AppPermissions.LeaseHaulerPortal_Schedule,
                    _orderLineRepository, input.OrderLineId.Value);

                dispatchesToCancel = await query
                    .Where(d => d.OrderLineId == input.OrderLineId)
                    .WhereIf(input.TruckId.HasValue, d => d.TruckId == input.TruckId.Value)
                    .WhereIf(input.TruckIds?.Any() == true, d => input.TruckIds.Contains(d.TruckId))
                    .WhereIf(leaseHaulerIdFilter.HasValue, d => d.Truck.LeaseHaulerTruck.LeaseHaulerId == leaseHaulerIdFilter)
                    .ToListAsync();
            }
            else if (input.TruckId.HasValue && input.Date.HasValue)
            {
                await CheckTruckEditPermissions(AppPermissions.Pages_Dispatches_Edit, AppPermissions.LeaseHaulerPortal_Schedule,
                    _truckRepository, input.TruckId.Value);

                dispatchesToCancel = await query
                    .Where(d => d.TruckId == input.TruckId.Value
                        && d.OrderLine.Order.DeliveryDate == input.Date
                        && d.OrderLine.Order.Shift == input.Shift)
                    .ToListAsync();
            }
            else
            {
                throw new ApplicationException("Either OrderLineId or (TruckId, Date) are required");
            }

            if (!dispatchesToCancel.Any())
            {
                return;
            }

            await CheckDispatchEditPermissions(AppPermissions.Pages_Dispatches_Edit, AppPermissions.LeaseHaulerPortal_Schedule,
                _dispatchRepository, dispatchesToCancel.Select(x => x.Id).ToArray());

            await CancelDispatches(dispatchesToCancel, "Canceled dispatch(es)");
        }

        private async Task CancelDispatches(List<Dispatch> dispatchesToCancel, string logMessage)
        {
            var affectedDriverIds = dispatchesToCancel.Select(x => x.DriverId).Distinct().ToList();
            var oldActiveDispatchesForDrivers = await GetFirstOpenDispatchIdPerDriver(affectedDriverIds);

            dispatchesToCancel.ForEach(SetDispatchEntityStatusToCanceled);
            await CurrentUnitOfWork.SaveChangesAsync();

            await _syncRequestSender.SendSyncRequest(new SyncRequest()
                    .AddChanges(EntityEnum.Dispatch, dispatchesToCancel.Select(x => x.ToChangedEntity()), ChangeType.Removed)
                    .AddLogMessage(logMessage));

            var newActiveDispatchesForDrivers = await GetFirstOpenDispatchIdPerDriver(affectedDriverIds);

            var cancelledDispatchesPerDriver = dispatchesToCancel
                .GroupBy(x => new
                {
                    x.DriverId,
                    x.UserId,
                    x.TruckId,
                    x.PhoneNumber,
                    x.EmailAddress,
                    x.OrderNotifyPreferredFormat,
                })
                .Select(x => x.Key)
                .ToList();

            var sendSmsInputs = new List<SendSmsOrEmailInput>();

            foreach (var dispatch in cancelledDispatchesPerDriver)
            {
                var oldActiveDispatch = oldActiveDispatchesForDrivers.GetValueOrDefault(dispatch.DriverId);
                var newActiveDispatch = newActiveDispatchesForDrivers.GetValueOrDefault(dispatch.DriverId);

                if (oldActiveDispatch != null && oldActiveDispatch != newActiveDispatch)
                {
                    sendSmsInputs.Add(new SendSmsOrEmailInput
                    {
                        TruckId = dispatch.TruckId,
                        DriverId = dispatch.DriverId,
                        UserId = dispatch.UserId,
                        PhoneNumber = dispatch.PhoneNumber,
                        EmailAddress = dispatch.EmailAddress,
                        OrderNotifyPreferredFormat = dispatch.OrderNotifyPreferredFormat,
                        ActiveDispatchWasChanged = true,
                    });
                }
            }

            if (sendSmsInputs.Any())
            {
                await _dispatchSender.BatchSendSmsOrEmail(sendSmsInputs.ToArray());
            }
        }

        private async Task<bool> IsAllowedToCancelDispatch(int dispatchId)
        {
            return !await (await _loadRepository.GetQueryAsync())
                .AnyAsync(l => l.DispatchId == dispatchId && l.Tickets.Any(),
                   CancellationTokenProvider.Token);
        }

        [AbpAuthorize(AppPermissions.Pages_Dispatches)]
        public async Task TestDriverDispatchSmsTemplate(TestSmsNumberInput input)
        {
            var lastOrderLine = await DispatchSender.GetOrderLineDataForDispatchMessageQuery(
                        (await _orderLineRepository.GetQueryAsync())
                            .OrderByDescending(ol => ol.Id)
                    )
                    .FirstOrDefaultAsync();

            lastOrderLine ??= new Dto.DispatchSender.OrderLineDto
            {
                Id = 0,
                DeliveryDate = await GetToday(),
                Shift = Shift.Shift1,
                OrderNumber = 0,
                Customer = "CustomerName",
                Directions = "Comments",
                Note = "Note",
                OrderLineTimeOnJobUtc = Clock.Now,
                FreightItemName = "Service",
                LoadAtName = "LoadAt",
                Designation = DesignationEnum.MaterialOnly,
                MaterialQuantity = 0,
                FreightQuantity = 0,
                MaterialUom = "Material UOM",
                FreightUom = "Freight UOM",
                DeliverToName = "DeliverTo",
            };

            string message = await _dispatchSender.CreateDispatchMessageFromTemplate(lastOrderLine);
            try
            {
                await _smsSender.SendAsync(new SmsSendInput
                {
                    ToPhoneNumber = input.FullPhoneNumber,
                    Body = message,
                    TrackStatus = false,
                });
            }
            catch (ApiException e)
            {
                Logger.Error(e.ToString());
                throw new UserFriendlyException($"An error occurred while sending the message: {e.Message}");
            }
        }

        private static void UncheckMultipleLoads(Dispatch dispatch)
        {
            dispatch.IsMultipleLoads = false;

            dispatch.NumberOfLoadsToFinish = dispatch.NumberOfAddedLoads + (IsAcknowledgedOrEarlier(dispatch.Status) ? 1 : 0);
        }

        private static bool IsAcknowledgedOrEarlier(DispatchStatus status)
        {
            return status.IsIn(DispatchStatus.Acknowledged, DispatchStatus.Sent, DispatchStatus.Created);
        }

        [AbpAuthorize(AppPermissions.Pages_Dispatches_Edit, AppPermissions.LeaseHaulerPortal_Schedule)]
        public async Task EndMultipleLoadsDispatches(int[] dispatchIds)
        {
            await CheckDispatchEditPermissions(AppPermissions.Pages_Dispatches_Edit, AppPermissions.LeaseHaulerPortal_Schedule,
                _dispatchRepository, dispatchIds);

            var dispatches = await (await _dispatchRepository.GetQueryAsync())
                .Where(x => dispatchIds.Contains(x.Id))
                .ToListAsync();

            foreach (var dispatch in dispatches)
            {
                UncheckMultipleLoads(dispatch);
            }

            await CurrentUnitOfWork.SaveChangesAsync();

            await _syncRequestSender.SendSyncRequest(new SyncRequest()
                .AddChanges(EntityEnum.Dispatch, dispatches.Select(x => x.ToChangedEntity()))
                .AddLogMessage("Ended multiple load dispatch(es)"));
        }


        [AbpAuthorize(AppPermissions.Pages_Dispatches_Edit, AppPermissions.LeaseHaulerPortal_Schedule)]
        public async Task CancelOrEndAllDispatches(CancelOrEndAllDispatchesInput input)
        {
            //CancelDispatches is public and will have its own permission check
            await CancelDispatches(new CancelDispatchesInput
            {
                OrderLineId = input.OrderLineId,
                TruckId = input.TruckId,
                TruckIds = input.TruckIds,
            });

            var leaseHaulerIdFilter = await GetLeaseHaulerIdFilterAsync(AppPermissions.Pages_Dispatches_Edit, AppPermissions.LeaseHaulerPortal_Schedule);

            var loadedDispatchIds = await (await _dispatchRepository.GetQueryAsync())
                    .Where(d => d.Status == DispatchStatus.Loaded && d.IsMultipleLoads)
                    .WhereIf(input.OrderLineId.HasValue, d => d.OrderLineId == input.OrderLineId)
                    .WhereIf(input.TruckId.HasValue, d => d.TruckId == input.TruckId.Value)
                    .WhereIf(input.TruckIds?.Any() == true, d => input.TruckIds.Contains(d.TruckId))
                    .WhereIf(leaseHaulerIdFilter.HasValue, d => d.Truck.LeaseHaulerTruck.LeaseHaulerId == leaseHaulerIdFilter)
                    .Select(x => x.Id)
                    .ToArrayAsync();

            await EndMultipleLoadsDispatches(loadedDispatchIds);
        }

        [AbpAuthorize(AppPermissions.Pages_Dispatches_ShowRemoveDispatchesButton)]
        public async Task RemoveAllDispatches()
        {
            var statusesToCancel = new[] { DispatchStatus.Created, DispatchStatus.Sent };
            var dispatchesToCancel = await (await _dispatchRepository.GetQueryAsync())
                    .Where(d => statusesToCancel.Contains(d.Status))
                    .ToListAsync();

            dispatchesToCancel.ForEach(SetDispatchEntityStatusToCanceled);

            var loadedDispatchesToEnd = await (await _dispatchRepository.GetQueryAsync())
                    .Where(d => d.Status == DispatchStatus.Loaded && d.IsMultipleLoads)
                    .ToListAsync();

            loadedDispatchesToEnd.ForEach(UncheckMultipleLoads);


            await CurrentUnitOfWork.SaveChangesAsync();

            await _syncRequestSender.SendSyncRequest(new SyncRequest()
                .AddChanges(EntityEnum.Dispatch, dispatchesToCancel.Select(x => x.ToChangedEntity()), ChangeType.Removed)
                .AddChanges(EntityEnum.Dispatch, loadedDispatchesToEnd.Select(x => x.ToChangedEntity()), ChangeType.Modified)
                .AddLogMessage("Removed all dispatches (canceled or ended multiple load)"));

        }

        [AbpAuthorize(AppPermissions.Pages_Schedule, AppPermissions.LeaseHaulerPortal_Schedule)]
        public async Task<bool> DispatchesExist(DispatchesExistInput input)
        {
            var leaseHaulerIdFilter = await GetLeaseHaulerIdFilterAsync(AppPermissions.Pages_Schedule, AppPermissions.LeaseHaulerPortal_Schedule);

            return await (await _dispatchRepository.GetQueryAsync())
                .WhereIf(input.OrderLineId.HasValue, x => x.OrderLineId == input.OrderLineId)
                .WhereIf(input.IsMultipleLoads.HasValue, x => x.IsMultipleLoads == input.IsMultipleLoads)
                .WhereIf(input.DispatchStatuses?.Any() == true, x => input.DispatchStatuses.Contains(x.Status))
                .WhereIf(leaseHaulerIdFilter.HasValue, x => x.Truck.LeaseHaulerTruck.LeaseHaulerId == leaseHaulerIdFilter)
                .AnyAsync();
        }

        [AbpAuthorize(AppPermissions.Pages_Dispatches_SendSyncRequest)]
        public async Task SendSilentSyncPushToDrivers(SendSilentSyncPushToDriversInput input)
        {
            //await _syncRequestSender.SendSyncRequest(new SyncRequest()
            //    .AddChange()); //todo send one 'change' for each entity type to force refresh of all entities / or add another ForceRefresh change request type since we don't have an id of the changed record to send

            //this will only send a push message to PWA apps for now
            await _backgroundJobManager.EnqueueAsync<DriverApplicationPushSenderBackgroundJob, DriverApplicationPushSenderBackgroundJobArgs>(new DriverApplicationPushSenderBackgroundJobArgs
            {
                RequestorUser = await Session.ToUserIdentifierAsync(),
            }.SetSyncRequestString(new SyncRequest()
                .AddChanges(EntityEnum.Dispatch, input.DriverIds.Select(x => new ChangedDispatch { DriverId = x }))
                .AddLogMessage($"Manually sent push request by user {AbpSession.UserId}")
            ));
        }

        [AbpAuthorize(AppPermissions.Pages_Administration_Host_Dashboard)]
        public async Task CleanupPushSubscriptions()
        {
            await _pushSubscriptionManager.CleanupSubscriptions();
        }







    }
}
