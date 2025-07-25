using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Abp.Authorization;
using Abp.Configuration;
using Abp.Domain.Repositories;
using Abp.Domain.Uow;
using Abp.Linq.Extensions;
using Abp.Notifications;
using Abp.Timing;
using Abp.UI;
using DispatcherWeb.Authorization;
using DispatcherWeb.Authorization.Roles;
using DispatcherWeb.Authorization.Users;
using DispatcherWeb.Configuration;
using DispatcherWeb.Dispatching;
using DispatcherWeb.DriverAssignments;
using DispatcherWeb.DriverAssignments.Dto;
using DispatcherWeb.Dto;
using DispatcherWeb.Features;
using DispatcherWeb.Infrastructure;
using DispatcherWeb.Infrastructure.Extensions;
using DispatcherWeb.LeaseHaulerRequests.Dto;
using DispatcherWeb.Notifications;
using DispatcherWeb.Offices;
using DispatcherWeb.Orders;
using DispatcherWeb.Scheduling;
using DispatcherWeb.Scheduling.Dto;
using DispatcherWeb.Url;
using Microsoft.EntityFrameworkCore;

namespace DispatcherWeb.LeaseHaulerRequests
{
    [AbpAuthorize]
    public class LeaseHaulerRequestEditAppService : DispatcherWebAppServiceBase, ILeaseHaulerRequestEditAppService
    {
        public IAppUrlService AppUrlService { get; set; }

        private readonly IRepository<LeaseHaulerRequest> _leaseHaulerRequestRepository;
        private readonly IRepository<AvailableLeaseHaulerTruck> _availableLeaseHaulerTruckRepository;
        private readonly IRepository<RequestedLeaseHaulerTruck> _requestedLeaseHaulerTruckRepository;
        private readonly IRepository<OrderLineTruck> _orderLineTruckRepository;
        private readonly IRepository<Dispatch> _dispatchRepository;
        private readonly IRepository<Ticket> _ticketRepository;
        private readonly INotificationPublisher _notificationPublisher;
        private readonly RoleManager _roleManager;
        private readonly IDriverAssignmentAppService _driverAssignmentAppService;
        private readonly ISchedulingAppService _schedulingAppService;
        private readonly ISingleOfficeAppService _singleOfficeService;
        private readonly IUserEmailer _userEmailer;
        private readonly ILeaseHaulerNotifier _leaseHaulerNotifier;

        public LeaseHaulerRequestEditAppService(
            IRepository<LeaseHaulerRequest> leaseHaulerRequestRepository,
            IRepository<AvailableLeaseHaulerTruck> availableLeaseHaulerTruckRepository,
            IRepository<RequestedLeaseHaulerTruck> requestedLeaseHaulerTruckRepository,
            IRepository<OrderLineTruck> orderLineTruckRepository,
            IRepository<Dispatch> dispatchRepository,
            IRepository<Ticket> ticketRepository,
            INotificationPublisher notificationPublisher,
            RoleManager roleManager,
            IDriverAssignmentAppService driverAssignmentAppService,
            ISchedulingAppService schedulingAppService,
            ISingleOfficeAppService singleOfficeService,
            IUserEmailer userEmailer,
            ILeaseHaulerNotifier leaseHaulerNotifier
        )
        {
            _leaseHaulerRequestRepository = leaseHaulerRequestRepository;
            _availableLeaseHaulerTruckRepository = availableLeaseHaulerTruckRepository;
            _requestedLeaseHaulerTruckRepository = requestedLeaseHaulerTruckRepository;
            _orderLineTruckRepository = orderLineTruckRepository;
            _dispatchRepository = dispatchRepository;
            _ticketRepository = ticketRepository;
            _notificationPublisher = notificationPublisher;
            _roleManager = roleManager;
            _driverAssignmentAppService = driverAssignmentAppService;
            _schedulingAppService = schedulingAppService;
            _singleOfficeService = singleOfficeService;
            _userEmailer = userEmailer;
            _leaseHaulerNotifier = leaseHaulerNotifier;
            AppUrlService = NullAppUrlService.Instance;
        }

        [AbpAuthorize(AppPermissions.Pages_LeaseHaulerRequests_Edit, AppPermissions.LeaseHaulerPortal_Truck_Request)]
        public async Task<LeaseHaulerRequestEditDto> GetLeaseHaulerRequestForEdit(GetLeaseHaulerRequestForEditInput input)
        {
            var hasEnabledJobBasedLeaseHaulerRequest =
                await FeatureChecker.IsEnabledAsync(AppFeatures.LeaseHaulerPortalJobBasedLeaseHaulerRequest)
                && await SettingManager.GetSettingValueAsync<bool>(AppSettings.LeaseHaulers.AllowLeaseHaulerRequestProcess);

            var model = input.LeaseHaulerRequestId != null
                ? await (await _leaseHaulerRequestRepository.GetQueryAsync())
                    .Where(lhr => lhr.Id == input.LeaseHaulerRequestId)
                    .WhereIf(hasEnabledJobBasedLeaseHaulerRequest && input.OrderLineId.HasValue, lhr => lhr.OrderLineId == input.OrderLineId)
                    .Select(lhr => new LeaseHaulerRequestEditDto
                    {
                        Id = lhr.Id,
                        OrderLineId = lhr.OrderLineId,
                        Date = lhr.Date,
                        Shift = lhr.Shift,
                        OfficeId = lhr.OfficeId,
                        OfficeName = lhr.Office.Name,
                        LeaseHaulerId = lhr.LeaseHaulerId,
                        LeaseHaulerName = lhr.LeaseHauler.Name,
                        Available = lhr.Available,
                        Approved = lhr.Approved,
                        NumberTrucksRequested = lhr.NumberTrucksRequested,
                        Message = lhr.Message,
                        Comments = lhr.Comments,
                        Status = lhr.Status,
                        SuppressLeaseHaulerDispatcherNotification = lhr.SuppressLeaseHaulerDispatcherNotification,
                    })
                    .FirstAsync()
                : new LeaseHaulerRequestEditDto
                {
                    Date = input.Date,
                    OrderLineId = input.OrderLineId,
                    SuppressLeaseHaulerDispatcherNotification = input.SuppressLeaseHaulerDispatcherNotification,
                };

            if (model.Id != 0)
            {
                await CheckEntitySpecificPermissions(
                    AppPermissions.Pages_LeaseHaulerRequests_Edit,
                    AppPermissions.LeaseHaulerPortal_Truck_Request,
                    Session.LeaseHaulerId,
                    model.LeaseHaulerId);

                if (hasEnabledJobBasedLeaseHaulerRequest)
                {
                    model.RequestedTrucks = await (await _requestedLeaseHaulerTruckRepository.GetQueryAsync())
                        .Where(x => x.LeaseHaulerRequestId == model.Id)
                        .Select(x => new RequestedLeaseHaulerTruckEditDto
                        {
                            TruckId = x.TruckId,
                            TruckCode = x.Truck.TruckCode,
                            DriverId = x.DriverId,
                            DriverName = x.Driver.FirstName + " " + x.Driver.LastName,
                        }).ToListAsync();


                    var trucksInUse = await GetTrucksInUse(new GetTrucksInUseInput
                    {
                        TruckIds = model.RequestedTrucks.Where(x => x.TruckId.HasValue).Select(x => x.TruckId.Value).Distinct().ToList(),
                        DriverIds = model.RequestedTrucks.Where(x => x.DriverId.HasValue).Select(x => x.DriverId.Value).Distinct().ToList(),
                    }.FillFrom(model));

                    MergeTrucksInUseIntoRequestedLeaseHaulerTrucks(model.RequestedTrucks, trucksInUse);
                }
                else
                {
                    model.AvailableTrucks = await (await _availableLeaseHaulerTruckRepository.GetQueryAsync())
                        .Where(x => x.LeaseHaulerRequestId == model.Id
                            && x.Date == model.Date
                            && x.Shift == model.Shift
                            && x.LeaseHaulerId == model.LeaseHaulerId
                            && x.OfficeId == model.OfficeId)
                        .Select(x => new AvailableTrucksTruckEditDto
                        {
                            TruckId = x.TruckId,
                            TruckCode = x.Truck.TruckCode,
                            DriverId = x.DriverId,
                            DriverName = x.Driver.FirstName + " " + x.Driver.LastName,
                        }).ToListAsync();

                    var trucksInUse = await GetTrucksInUse(new GetTrucksInUseInput
                    {
                        TruckIds = model.AvailableTrucks.Where(x => x.TruckId.HasValue).Select(x => x.TruckId.Value).Distinct().ToList(),
                        DriverIds = model.AvailableTrucks.Where(x => x.DriverId.HasValue).Select(x => x.DriverId.Value).Distinct().ToList(),
                    }.FillFrom(model));

                    MergeTrucksInUseIntoAvailableTrucks(model.AvailableTrucks, trucksInUse);
                }
            }

            await _singleOfficeService.FillSingleOffice(model);

            return model;
        }

        private void MergeTrucksInUseIntoRequestedLeaseHaulerTrucks(List<RequestedLeaseHaulerTruckEditDto> truckModels, List<AvailableTruckUsageDto> trucksInUse)
        {
            foreach (var truckModel in truckModels)
            {
                foreach (var truckInUse in trucksInUse)
                {
                    truckModel.IsTruckInUse |= truckInUse.TruckId == truckModel.TruckId;
                    truckModel.IsDriverInUse |= truckInUse.TruckId == truckModel.TruckId && truckInUse.DriverId == truckModel.DriverId;
                    if (truckModel.IsDriverInUse)
                    {
                        break;
                    }
                }
            }
        }

        private void MergeTrucksInUseIntoAvailableTrucks(List<AvailableTrucksTruckEditDto> truckModels, List<AvailableTruckUsageDto> trucksInUse)
        {
            foreach (var truckModel in truckModels)
            {
                foreach (var truckInUse in trucksInUse)
                {
                    truckModel.IsTruckInUse |= truckInUse.TruckId == truckModel.TruckId;
                    truckModel.IsDriverInUse |= truckInUse.TruckId == truckModel.TruckId && truckInUse.DriverId == truckModel.DriverId;
                    if (truckModel.IsDriverInUse)
                    {
                        break;
                    }
                }
            }
        }

        [AbpAuthorize(AppPermissions.Pages_LeaseHaulerRequests_Edit)]
        public async Task<List<AvailableTruckUsageDto>> GetTrucksInUse(GetTrucksInUseInput input)
        {
            input.DriverIds ??= new List<int>();
            input.TruckIds ??= new List<int>();

            var orderLineTrucks = await (await _orderLineTruckRepository.GetQueryAsync())
                .Where(x => x.OrderLine.Order.DeliveryDate == input.Date
                    && x.OrderLine.Order.Shift == input.Shift
                    && x.OrderLine.Order.OfficeId == input.OfficeId)
                .WhereIf(input.TruckIds.Any(),
                    x => input.TruckIds.Contains(x.TruckId))
                .Select(x => new AvailableTruckUsageDto
                {
                    TruckId = x.TruckId,
                    DriverId = x.DriverId,
                })
                .ToListAsync();

            var dispatches = await (await _dispatchRepository.GetQueryAsync())
                .Where(x => x.OrderLine.Order.DeliveryDate == input.Date
                    && x.OrderLine.Order.Shift == input.Shift
                    && x.OrderLine.Order.OfficeId == input.OfficeId
                    && !Dispatch.ClosedDispatchStatuses.Contains(x.Status))
                .WhereIf(input.DriverIds.Any() || input.TruckIds.Any(),
                    x => input.TruckIds.Contains(x.TruckId) || input.DriverIds.Contains(x.DriverId))
                .Select(x => new AvailableTruckUsageDto
                {
                    TruckId = x.TruckId,
                    DriverId = x.DriverId,
                })
                .ToListAsync();

            var tickets = await (await _ticketRepository.GetQueryAsync())
                .Where(x => x.OrderLine.Order.DeliveryDate == input.Date
                        && x.OrderLine.Order.Shift == input.Shift
                        && x.OrderLine.Order.OfficeId == input.OfficeId
                        && x.TruckId.HasValue)
                .WhereIf(input.DriverIds.Any() || input.TruckIds.Any(),
                    x => input.TruckIds.Contains(x.TruckId.Value)
                            || x.DriverId.HasValue && input.DriverIds.Contains(x.DriverId.Value))
                .Select(x => new AvailableTruckUsageDto
                {
                    TruckId = x.TruckId.Value,
                    DriverId = x.DriverId,
                })
                .ToListAsync();

            return orderLineTrucks
                .Union(dispatches)
                .Union(tickets)
                .GroupBy(x => new { x.TruckId, x.DriverId })
                .Select(x => new AvailableTruckUsageDto
                {
                    TruckId = x.Key.TruckId,
                    DriverId = x.Key.DriverId,
                }).ToList();
        }

        [AbpAuthorize(AppPermissions.Pages_LeaseHaulerRequests_Edit, AppPermissions.LeaseHaulerPortal_Truck_Request)]
        public async Task<LeaseHaulerRequestEditModel> EditLeaseHaulerRequest(LeaseHaulerRequestEditModel model)
        {
            var permissions = new
            {
                EditAnyLeaseHaulerRequest = await IsGrantedAsync(AppPermissions.Pages_LeaseHaulerRequests_Edit),
                EditLeaseHaulerSpecificTruckRequest = await IsGrantedAsync(AppPermissions.LeaseHaulerPortal_Truck_Request),
            };
            var hasEnabledJobBasedLeaseHaulerRequest =
                await FeatureChecker.IsEnabledAsync(AppFeatures.LeaseHaulerPortalJobBasedLeaseHaulerRequest)
                && await SettingManager.GetSettingValueAsync<bool>(AppSettings.LeaseHaulers.AllowLeaseHaulerRequestProcess);

            var leaseHaulerRequest = model.Id != 0
                ? await (await _leaseHaulerRequestRepository.GetQueryAsync()).Where(lhr => lhr.Id == model.Id).FirstAsync()
                : new LeaseHaulerRequest
                {
                    Guid = Guid.NewGuid(),
                    Status = hasEnabledJobBasedLeaseHaulerRequest ? LeaseHaulerRequestStatus.Requested : null,
                    LeaseHaulerId = model.LeaseHaulerId,
                };

            if (hasEnabledJobBasedLeaseHaulerRequest)
            {
                if (model.Id == 0 && model.OrderLineId.HasValue)
                {
                    // check if there is an existing request for this job and lease hauler
                    var exists = await (await _leaseHaulerRequestRepository.GetQueryAsync())
                        .AnyAsync(q => q.OrderLineId == model.OrderLineId && q.LeaseHaulerId == model.LeaseHaulerId);
                    if (exists)
                    {
                        throw new UserFriendlyException("Truck(s) request to this Lease Hauler for this job has already been made.");
                    }
                }

                if (model.Id == 0 && model.Date < await GetToday())
                {
                    throw new UserFriendlyException("You can't create a request for a job in the past.");
                }

                leaseHaulerRequest.OrderLineId = model.OrderLineId;
                leaseHaulerRequest.NumberTrucksRequested = model.NumberTrucksRequested;
            }
            else
            {
                EnsureApprovedIsNotGreaterThanAvailable(model.Approved, model.Available);

                if (permissions.EditLeaseHaulerSpecificTruckRequest)
                {
                    if (leaseHaulerRequest.Available != model.Available)
                    {
                        var message = "{LeaseHaulerName} has made trucks available.";
                        if (leaseHaulerRequest.Available.HasValue)
                        {
                            message = "Number of trucks available has been changed from {OldValue} to {NewValue} by {LeaseHaulerName} for {LeaseHaulerRequestDate}{LeaseHaulerRequestShift}.";
                        }
                        await NotifyDispatchersAboutChangedAvailableTrucksNumber(leaseHaulerRequest, model.Available ?? 0, message);
                    }
                }
                leaseHaulerRequest.Available = model.Available;

                if (permissions.EditAnyLeaseHaulerRequest)
                {
                    if (!model.SuppressLeaseHaulerDispatcherNotification && (!leaseHaulerRequest.Approved.HasValue || model.Approved > leaseHaulerRequest.Approved))
                    {
                        var notificationData = new NotifyLeaseHaulerInput
                        {
                            LeaseHaulerId = leaseHaulerRequest.LeaseHaulerId,
                            LeaseHaulerRequestGuid = leaseHaulerRequest.Guid,
                            Message = "{CompanyName} has accepted some of your trucks. Please visit the {LinkToRequest} to see the specifics.",
                        };
                        await _leaseHaulerNotifier.NotifyLeaseHaulerDispatchers(notificationData);
                    }
                }
                leaseHaulerRequest.Approved = model.Approved;
                leaseHaulerRequest.SuppressLeaseHaulerDispatcherNotification = model.SuppressLeaseHaulerDispatcherNotification;

                leaseHaulerRequest.NumberTrucksRequested = model.NumberTrucksRequested;
                leaseHaulerRequest.Comments = model.Comments;
            }

            var oldLeaseHaulerId = leaseHaulerRequest.LeaseHaulerId;

            leaseHaulerRequest.Date = model.Date.Date;
            leaseHaulerRequest.Shift = model.Shift;
            leaseHaulerRequest.OfficeId = model.OfficeId;

            if (permissions.EditAnyLeaseHaulerRequest
                || permissions.EditLeaseHaulerSpecificTruckRequest && hasEnabledJobBasedLeaseHaulerRequest)
            {
                leaseHaulerRequest.Status = model.Id != 0 ? model.Status : leaseHaulerRequest.Status;
                leaseHaulerRequest.LeaseHaulerId = model.LeaseHaulerId;
            }

            await CheckEntitySpecificPermissions(
                AppPermissions.Pages_LeaseHaulerRequests_Edit,
                AppPermissions.LeaseHaulerPortal_Truck_Request,
                Session.LeaseHaulerId,
                leaseHaulerRequest.LeaseHaulerId,
                oldLeaseHaulerId);

            model.Id = await _leaseHaulerRequestRepository.InsertOrUpdateAndGetIdAsync(leaseHaulerRequest);
            model.Trucks ??= new List<int?>();
            model.Drivers ??= new List<int?>();

            if (hasEnabledJobBasedLeaseHaulerRequest)
            {
                if (await IsGrantedAsync(AppPermissions.Pages_LeaseHaulerRequests_Edit)
                    && leaseHaulerRequest.Status == LeaseHaulerRequestStatus.Requested && leaseHaulerRequest.NumberTrucksRequested.HasValue)
                {
                    var leaseHaulerDispatchers = await (await UserManager.GetQueryAsync())
                        .Where(q => q.IsActive && q.LeaseHaulerUser.LeaseHaulerId == leaseHaulerRequest.LeaseHaulerId)
                        .AsNoTracking()
                        .ToListAsync();
                    foreach (var leaseHaulerDispatcher in leaseHaulerDispatchers)
                    {
                        await _userEmailer.SendLeaseHaulerJobRequestEmail(
                            leaseHaulerDispatcher,
                            leaseHaulerRequest.NumberTrucksRequested.Value,
                            leaseHaulerRequest.Date,
                            AppUrlService.CreateLinkToSchedule(leaseHaulerDispatcher.TenantId),
                            await SettingManager.GetSettingValueAsync(AppSettings.EmailTemplate.LeaseHaulerJobRequestEmailSubjectTemplate),
                            await SettingManager.GetSettingValueAsync(AppSettings.EmailTemplate.LeaseHaulerJobRequestEmailBodyTemplate)
                        );
                    }
                }

                if (leaseHaulerRequest.Status == LeaseHaulerRequestStatus.Approved
                    && model.Trucks != null
                    && model.Drivers != null)
                {
                    // lease hauler dispatcher has assigned trucks/drivers after the request has been approved
                    await SetLeaseHaulerTrucksToOrderLine(model.OrderLineId, model.Trucks, model.Drivers);
                }
                else
                {
                    // assigning trucks/drivers prior to approval/rejection
                    await UpdateRequestedLeaseHaulerTrucksAsync(leaseHaulerRequest, model.Trucks, model.Drivers);
                }
            }
            else
            {
                await UpdateAvailableTrucksAsync(leaseHaulerRequest, model.Trucks, model.Drivers, required: false);
            }

            return model;
        }

        private void EnsureApprovedIsNotGreaterThanAvailable(int? approved, int? available)
        {
            if (approved > available || approved != null && available == null)
            {
                throw new ArgumentException("Approved must be less than or equal to Available!");
            }
        }

        [AbpAuthorize(AppPermissions.Pages_LeaseHaulerRequests_Edit, AppPermissions.LeaseHaulerPortal_Truck_Request)]
        public async Task UpdateAvailable(IdValueInput<int?> input)
        {
            var permissions = new
            {
                EditAnyLeaseHaulerRequest = await IsGrantedAsync(AppPermissions.Pages_LeaseHaulerRequests_Edit),
                EditLeaseHaulerSpecificTruckRequest = await IsGrantedAsync(AppPermissions.LeaseHaulerPortal_Truck_Request),
            };

            var leaseHaulerRequest = await _leaseHaulerRequestRepository.GetAsync(input.Id);

            await CheckEntitySpecificPermissions(
                    AppPermissions.Pages_LeaseHaulerRequests_Edit,
                    AppPermissions.LeaseHaulerPortal_Truck_Request,
                    Session.LeaseHaulerId,
                    leaseHaulerRequest.LeaseHaulerId);

            var canEditRequest = permissions.EditAnyLeaseHaulerRequest
                || permissions.EditLeaseHaulerSpecificTruckRequest && leaseHaulerRequest.Approved is null or 0;

            if (canEditRequest)
            {
                EnsureApprovedIsNotGreaterThanAvailable(leaseHaulerRequest.Approved, input.Value);
                var message = "{LeaseHaulerName} has made trucks available.";
                if (leaseHaulerRequest.Available.HasValue)
                {
                    message = "Number of trucks available has been changed from {OldValue} to {NewValue} by {LeaseHaulerName} for {LeaseHaulerRequestDate}{LeaseHaulerRequestShift}.";
                }
                await NotifyDispatchersAboutChangedAvailableTrucksNumber(leaseHaulerRequest, input.Value ?? 0, message);
                leaseHaulerRequest.Available = input.Value;
            }
        }

        [AbpAuthorize(AppPermissions.Pages_LeaseHaulerRequests_Edit)]
        public async Task UpdateApproved(IdValueInput<int?> input)
        {
            var leaseHaulerRequest = await _leaseHaulerRequestRepository.GetAsync(input.Id);
            EnsureApprovedIsNotGreaterThanAvailable(input.Value, leaseHaulerRequest.Available);
            if (!leaseHaulerRequest.Approved.HasValue || input.Value >= leaseHaulerRequest.Approved)
            {
                leaseHaulerRequest.Approved = input.Value;
                var notificationData = new NotifyLeaseHaulerInput
                {
                    LeaseHaulerId = leaseHaulerRequest.LeaseHaulerId,
                    LeaseHaulerRequestGuid = leaseHaulerRequest.Guid,
                    Message = "{CompanyName} has accepted some of your trucks. Please visit the {LinkToRequest} to see the specifics.",
                };
                await _leaseHaulerNotifier.NotifyLeaseHaulerDispatchers(notificationData);
                return;
            }
            var availableTruckCount = await (await _availableLeaseHaulerTruckRepository.GetQueryAsync())
                .CountAsync(x => x.Date == leaseHaulerRequest.Date
                            && x.LeaseHaulerId == leaseHaulerRequest.LeaseHaulerId
                            && x.Shift == leaseHaulerRequest.Shift);
            if (availableTruckCount == 0 || availableTruckCount <= input.Value)
            {
                leaseHaulerRequest.Approved = input.Value;
                return;
            }
            throw new UserFriendlyException("Approved must be greater than or equal to the number of already added trucks");
        }

        [AbpAllowAnonymous]
        public async Task<AvailableTrucksEditDto> GetAvailableTrucksEditDto(Guid leaseHaulerRequestGuid)
        {
            var oltQuery = await _orderLineTruckRepository.GetQueryAsync();
            var model = await (await _leaseHaulerRequestRepository.GetQueryAsync())
                .Where(lhr => lhr.Guid == leaseHaulerRequestGuid)
                .Select(lhr => new AvailableTrucksEditDto
                {
                    Id = lhr.Id,
                    TenantId = lhr.TenantId,
                    LeaseHaulerId = lhr.LeaseHaulerId,
                    OfficeId = lhr.OfficeId,
                    Date = lhr.Date,
                    Shift = lhr.Shift,
                    Available = lhr.Available,
                    Approved = lhr.Approved,
                    Scheduled = oltQuery
                        .Count(olt => olt.OrderLine.Order.OfficeId == lhr.OfficeId
                            && olt.OrderLine.Order.Shift == lhr.Shift
                            && olt.OrderLine.Order.DeliveryDate == lhr.Date
                            && olt.Truck.LeaseHaulerTruck.LeaseHaulerId == lhr.LeaseHaulerId),
                    Comments = lhr.Comments,
                    Message = lhr.Message,
                })
                .FirstOrDefaultAsync();

            if (model == null)
            {
                return null;
            }

            model.CustomerName = await SettingManager.GetSettingValueForTenantAsync(AppSettings.General.CompanyName, model.TenantId);
            model.ShiftName = await SettingManager.GetShiftName(model.Shift, model.TenantId);

            var timeZone = await SettingManager.GetSettingValueForTenantAsync(TimingSettingNames.TimeZone, model.TenantId);
            var expirationDateTime = model.Date.Date.AddDays(2).ConvertTimeZoneFrom(timeZone);
            model.IsExpired = Clock.Now >= expirationDateTime;

            if (model.ShowTruckControls)
            {
                model.Trucks = await (await _availableLeaseHaulerTruckRepository.GetQueryAsync())
                    .Where(x => x.Date == model.Date
                        && x.Shift == model.Shift
                        && x.LeaseHaulerId == model.LeaseHaulerId
                        && x.OfficeId == model.OfficeId)
                    .Select(x => new AvailableTrucksTruckEditDto
                    {
                        TruckId = x.TruckId,
                        TruckCode = x.Truck.TruckCode,
                        DriverId = x.DriverId,
                        DriverName = x.Driver.FirstName + " " + x.Driver.LastName,
                    }).ToListAsync();

                var trucksInUse = await GetTrucksInUse(new GetTrucksInUseInput
                {
                    TruckIds = model.Trucks.Where(x => x.TruckId.HasValue).Select(x => x.TruckId.Value).Distinct().ToList(),
                    DriverIds = model.Trucks.Where(x => x.DriverId.HasValue).Select(x => x.DriverId.Value).Distinct().ToList(),
                }.FillFrom(model));

                MergeTrucksInUseIntoAvailableTrucks(model.Trucks, trucksInUse);
            }

            return model;
        }

        [AbpAllowAnonymous]
        public async Task EditAvailableTrucks(AvailableTrucksEditModel model)
        {
            ShortGuid.TryParse(model.Id, out var guid);
            var leaseHaulerRequest = await _leaseHaulerRequestRepository.FirstOrDefaultAsync(x => x.Guid == guid.Guid);

            if (leaseHaulerRequest == null)
            {
                throw new ArgumentException($"There is no LeaseHaulerRequest with Guid={guid.Guid}");
            }

            if (leaseHaulerRequest.Approved.HasValue && model.Available < leaseHaulerRequest.Approved)
            {
                throw new UserFriendlyException("You can't reduce the number of trucks available below the already approved value");
            }

            if (leaseHaulerRequest.Available != model.Available)
            {
                var message = "{LeaseHaulerName} has made trucks available.";
                if (leaseHaulerRequest.Available.HasValue)
                {
                    message = "Number of trucks available has been changed from {OldValue} to {NewValue} by {LeaseHaulerName} for {LeaseHaulerRequestDate}{LeaseHaulerRequestShift}.";
                }
                await NotifyDispatchersAboutChangedAvailableTrucksNumber(leaseHaulerRequest, model.Available, message);
            }

            leaseHaulerRequest.Available = model.Available;

            if (leaseHaulerRequest.Approved > 0)
            {
                //the field is only shown when 'approved' was set
                leaseHaulerRequest.Comments = model.Comments;

                await UpdateAvailableTrucksAsync(leaseHaulerRequest, model.Trucks, model.Drivers, required: true);
            }

            // Local functions
            // #8604 Commented until further notice
            //void ThrowUserFriendlyExceptionToRefreshPageIfAvailableHasChanged()
            //{
            //    (
            //        leaseHaulerRequest.Available != model.Available
            //        || leaseHaulerRequest.Approved != model.Approved
            //    ).ThrowUserFriendlyExceptionIfTrue("The Available or Approved values are changed. Please refresh the page.");
            //}
        }

        private async Task UpdateRequestedLeaseHaulerTrucksAsync(LeaseHaulerRequest leaseHaulerRequest, List<int?> trucks, List<int?> drivers)
        {
            if (trucks == null || drivers == null || trucks.Any(x => !x.HasValue) || drivers.Any(x => !x.HasValue))
            {
                throw new UserFriendlyException("Trucks and Drivers are required fields");
            }

            var requestedLeaseHaulerTrucks = await (await _requestedLeaseHaulerTruckRepository.GetQueryAsync())
                .Where(q => q.LeaseHaulerRequestId == leaseHaulerRequest.Id)
                .ToListAsync();
            var leaseHaulerTrucksInUse = await GetTrucksInUse(new GetTrucksInUseInput
            {
                TruckIds = requestedLeaseHaulerTrucks.Select(s => s.TruckId).Distinct().ToList(),
                DriverIds = requestedLeaseHaulerTrucks.Select(s => s.DriverId).Distinct().ToList(),
                Date = leaseHaulerRequest.Date,
                LeaseHaulerId = leaseHaulerRequest.LeaseHaulerId,
                OfficeId = leaseHaulerRequest.OfficeId,
                Shift = leaseHaulerRequest.Shift,
            });

            if (leaseHaulerTrucksInUse.Any(q => trucks.Contains(q.TruckId)))
            {
                throw new UserFriendlyException("One of the trucks is associated with dispatches, or tickets.");
            }

            if (leaseHaulerTrucksInUse.Any(q => drivers.Contains(q.DriverId)))
            {
                throw new UserFriendlyException("One of the drivers is associated with dispatches, or tickets.");
            }

            var trucksWithValues = trucks.Where(x => x.HasValue).ToList();
            if (trucksWithValues.Distinct().Count() < trucksWithValues.Count)
            {
                throw new UserFriendlyException("You specified the same truck more than once");
            }

            for (var i = 0; i < Math.Min(trucks.Count, drivers.Count); i++)
            {
                if (!trucks[i].HasValue)
                {
                    if (drivers[i].HasValue)
                    {
                        throw new UserFriendlyException("Truck is required for selected Drivers");
                    }
                    continue;
                }

                if (!drivers[i].HasValue)
                {
                    throw new UserFriendlyException("Driver is required for selected Trucks");
                }

                var existing = requestedLeaseHaulerTrucks.FirstOrDefault(x => x.TruckId == trucks[i]);
                if (existing != null)
                {
                    if (existing.DriverId == drivers[i].Value)
                    {
                        continue;
                    }

                    if (leaseHaulerTrucksInUse.Any(x => x.TruckId == existing.TruckId && x.DriverId == existing.DriverId))
                    {
                        throw new UserFriendlyException("One of the drivers is associated with dispatches, or tickets.", "If you want to remove or change the driver, you need to remove any associated dispatches, and tickets for this date.");
                    }
                    existing.DriverId = drivers[i].Value;
                }
                else
                {
                    await _requestedLeaseHaulerTruckRepository.InsertAsync(new RequestedLeaseHaulerTruck
                    {
                        TenantId = leaseHaulerRequest.TenantId,
                        LeaseHaulerRequestId = leaseHaulerRequest.Id,
                        TruckId = trucks[i].Value,
                        DriverId = drivers[i].Value,
                    });
                }
            }

            var trucksToRemoveFromTheRequest = requestedLeaseHaulerTrucks.Where(x => !trucks.Contains(x.TruckId)).ToList();
            // if we reach this far, it means that some of the trucks were changed/removed
            foreach (var truckToRemove in trucksToRemoveFromTheRequest)
            {
                if (requestedLeaseHaulerTrucks.Any(x => x.TruckId == truckToRemove.TruckId))
                {
                    continue;
                }

                await _requestedLeaseHaulerTruckRepository.DeleteAsync(truckToRemove);
            }
        }

        private async Task UpdateAvailableTrucksAsync(LeaseHaulerRequest leaseHaulerRequest, List<int?> trucks, List<int?> drivers, bool required)
        {
            var existingTrucks = await (await _availableLeaseHaulerTruckRepository.GetQueryAsync())
                    .Where(x => x.LeaseHaulerRequestId == leaseHaulerRequest.Id
                        && x.Date == leaseHaulerRequest.Date
                        && x.Shift == leaseHaulerRequest.Shift
                        && x.LeaseHaulerId == leaseHaulerRequest.LeaseHaulerId
                        && x.OfficeId == leaseHaulerRequest.OfficeId)
                    .ToListAsync();

            var trucksInUse = await GetTrucksInUse(new GetTrucksInUseInput
            {
                TruckIds = existingTrucks.Select(x => x.TruckId).Distinct().ToList(),
                DriverIds = existingTrucks.Select(x => x.DriverId).Distinct().ToList(),
                Date = leaseHaulerRequest.Date,
                LeaseHaulerId = leaseHaulerRequest.LeaseHaulerId,
                OfficeId = leaseHaulerRequest.OfficeId,
                Shift = leaseHaulerRequest.Shift,
            });

            if (required)
            {
                if (trucks == null
                    || drivers == null
                    || trucks.Any(x => !x.HasValue)
                    || drivers.Any(x => !x.HasValue))
                {
                    throw new UserFriendlyException("Trucks and Drivers are required fields");
                }
            }

            var trucksWithValues = trucks?.Where(x => x.HasValue).ToList();

            if (trucksWithValues?.Distinct().Count() < trucksWithValues?.Count)
            {
                throw new UserFriendlyException("You specified the same truck more than once");
            }

            if (trucks != null && drivers != null)
            {
                for (int i = 0; i < Math.Min(trucks.Count, drivers.Count); i++)
                {
                    if (!trucks[i].HasValue)
                    {
                        if (drivers[i].HasValue)
                        {
                            throw new UserFriendlyException("Truck is required for selected Drivers");
                        }
                        continue;
                    }
                    if (!drivers[i].HasValue)
                    {
                        throw new UserFriendlyException("Driver is required for selected Trucks");
                    }
                    var existing = existingTrucks.FirstOrDefault(x => x.TruckId == trucks[i]);
                    if (existing != null)
                    {
                        if (existing.DriverId != drivers[i].Value)
                        {
                            if (trucksInUse.Any(x => x.TruckId == existing.TruckId && x.DriverId == existing.DriverId))
                            {
                                throw new UserFriendlyException("One of the drivers is associated with dispatches, or tickets.", "If you want to remove or change the driver, you need to remove any associated dispatches, and tickets for this date.");
                            }
                            existing.DriverId = drivers[i].Value;
                        }
                    }
                    else
                    {
                        await _availableLeaseHaulerTruckRepository.InsertAsync(new AvailableLeaseHaulerTruck
                        {
                            TenantId = leaseHaulerRequest.TenantId,
                            LeaseHaulerRequestId = leaseHaulerRequest.Id,
                            LeaseHaulerId = leaseHaulerRequest.LeaseHaulerId,
                            OfficeId = leaseHaulerRequest.OfficeId,
                            Date = leaseHaulerRequest.Date,
                            Shift = leaseHaulerRequest.Shift,
                            TruckId = trucks[i].Value,
                            DriverId = drivers[i].Value,
                        });

                        var message = "{LeaseHaulerName} has committed trucks.";
                        await NotifyDispatchersAboutChangedAvailableTrucksNumber(leaseHaulerRequest, 0, message);
                    }
                }
            }

            var trucksToDelete = existingTrucks.Where(x => trucks == null || !trucks.Contains(x.TruckId)).ToList();
            foreach (var truckToDelete in trucksToDelete)
            {
                if (trucksInUse.Any(x => x.TruckId == truckToDelete.TruckId))
                {
                    continue;
                }

                await _availableLeaseHaulerTruckRepository.DeleteAsync(truckToDelete);
            }
        }

        private async Task SetLeaseHaulerTrucksToOrderLine(int? orderLineId, List<int?> trucks, List<int?> drivers)
        {
            if (orderLineId.HasValue && trucks != null && drivers != null)
            {
                for (int i = 0; i < Math.Min(trucks.Count, drivers.Count); i++)
                {
                    await _schedulingAppService.AddOrderLineTruck(new AddOrderLineTruckInput
                    {
                        OrderLineId = orderLineId.Value,
                        TruckId = trucks[i].Value,
                        DriverId = drivers[i].Value,
                    });
                }
            }
        }

        private async Task NotifyDispatchersAboutChangedAvailableTrucksNumber(LeaseHaulerRequest leaseHaulerRequest, int newValue, string message)
        {
            var userIds = new List<Abp.UserIdentifier>();

            using (CurrentUnitOfWork.DisableFilter(AbpDataFilters.MustHaveTenant))
            using (CurrentUnitOfWork.DisableFilter(AbpDataFilters.MayHaveTenant))
            {
                var dispatchingRoleIds = await (await _roleManager.GetAvailableRolesAsync())
                    .Where(x => x.TenantId == leaseHaulerRequest.TenantId
                        && x.Name == StaticRoleNames.Tenants.Dispatching)
                    .Select(x => x.Id)
                    .ToListAsync();

                var users = await (await UserManager.GetQueryAsync())
                    .Include(x => x.Roles)
                    .Where(x => x.OfficeId == leaseHaulerRequest.OfficeId
                        && x.TenantId == leaseHaulerRequest.TenantId
                        && x.Roles.Any(r => dispatchingRoleIds.Contains(r.RoleId)))
                    .ToListAsync();

                foreach (var user in users)
                {
                    userIds.Add(user.ToUserIdentifier());
                }
            }

            if (userIds.Any())
            {
                var additionalData = await (await _leaseHaulerRequestRepository.GetQueryAsync())
                    .Where(x => x.Id == leaseHaulerRequest.Id)
                    .Select(x => new
                    {
                        LeaseHaulerName = x.LeaseHauler.Name,
                    }).FirstAsync();

                message = message
                    .Replace("{OldValue}", leaseHaulerRequest.Available.ToString())
                    .Replace("{NewValue}", newValue.ToString())
                    .Replace("{LeaseHaulerName}", additionalData.LeaseHaulerName)
                    .Replace("{LeaseHaulerRequestDate}", leaseHaulerRequest.Date.ToShortDateString())
                    .Replace("{LeaseHaulerRequestShift}", leaseHaulerRequest.Shift.HasValue ? " " + await SettingManager.GetShiftName(leaseHaulerRequest.Shift) : "");

                var notificationData = new MessageNotificationData(message)
                {
                    ["leaseHaulerRequestId"] = leaseHaulerRequest.Id,
                };

                await _notificationPublisher.PublishAsync(
                    AppNotificationNames.AvailableTruckNumberChanged,
                    notificationData,
                    severity: NotificationSeverity.Info,
                    userIds: userIds.ToArray()
                );
            }
        }

        [AbpAuthorize(AppPermissions.Pages_LeaseHaulerRequests_Edit)]
        public async Task SetDriverForLeaseHaulerTruck(SetDriverForTruckInput input)
        {
            if (input.DriverId == null)
            {
                throw new UserFriendlyException("Driver is required");
            }

            if (input.LeaseHaulerId == null)
            {
                throw new ApplicationException("LeaseHaulerId is null in SetDriverForLeaseHaulerTruck input");
            }

            var availableLeaseHaulerTrucks = await (await _availableLeaseHaulerTruckRepository.GetQueryAsync())
                .WhereIf(input.OfficeId.HasValue, x => x.OfficeId == input.OfficeId)
                .Where(x => x.Date == input.Date
                    && x.Shift == input.Shift
                    && x.TruckId == input.TruckId)
                .ToListAsync();

            var oldDriverIds = availableLeaseHaulerTrucks.Select(x => x.DriverId).Distinct().ToList();
            foreach (var oldDriverId in oldDriverIds)
            {
                var validationResult = await _driverAssignmentAppService.HasOrderLineTrucks(new HasOrderLineTrucksInput
                {
                    Date = input.Date,
                    OfficeId = input.OfficeId,
                    Shift = input.Shift,
                    TruckId = input.TruckId,
                    DriverId = oldDriverId,
                });
                if (validationResult.HasOpenDispatches)
                {
                    throw new UserFriendlyException(L("CannotChangeDriverBecauseOfDispatchesError"));
                }
                if (validationResult.HasOrderLineTrucks)
                {
                    //uncomment if we start supporting null DriverIds in AvailableLeaseHaulerTrucks
                    //if (input.DriverId == null)
                    //{
                    //    throw new UserFriendlyException(L("CannotRemoveDriverBecauseOfOrderLineTrucksError"));
                    //}

                    var orderLineTrucks = await (await _orderLineTruckRepository.GetQueryAsync())
                        .WhereIf(input.OfficeId.HasValue, x => input.OfficeId == x.OrderLine.Order.OfficeId)
                        .Where(x => input.Date == x.OrderLine.Order.DeliveryDate && input.Shift == x.OrderLine.Order.Shift)
                        .Where(x => oldDriverId == x.DriverId)
                        .Where(x => input.TruckId == x.TruckId)
                        .ToListAsync();

                    foreach (var orderLineTruck in orderLineTrucks)
                    {
                        orderLineTruck.DriverId = input.DriverId.Value;
                    }
                }
            }

            foreach (var availableLeaseHaulerTruck in availableLeaseHaulerTrucks)
            {
                availableLeaseHaulerTruck.DriverId = input.DriverId.Value;
            }
        }

        [AbpAuthorize(AppPermissions.Pages_LeaseHaulerRequests_Edit)]
        public async Task RemoveAvailableLeaseHaulerTruckFromSchedule(RemoveAvailableLeaseHaulerTruckFromScheduleInput input)
        {
            var hasOrderLineTrucks = await (await _orderLineTruckRepository.GetQueryAsync())
                        .Where(x => input.Date == x.OrderLine.Order.DeliveryDate && input.Shift == x.OrderLine.Order.Shift)
                        .WhereIf(input.OfficeId.HasValue, x => input.OfficeId == x.OrderLine.Order.OfficeId)
                        .Where(x => input.TruckId == x.TruckId)
                        .AnyAsync();

            if (hasOrderLineTrucks)
            {
                throw new UserFriendlyException("You cannot remove this truck because it was already added to orders");
            }

            await _availableLeaseHaulerTruckRepository.DeleteAsync(x => x.Date == input.Date
                    && (!input.OfficeId.HasValue || x.OfficeId == input.OfficeId)
                    && x.Shift == input.Shift
                    && x.TruckId == input.TruckId);
        }

        [AbpAuthorize(AppPermissions.LeaseHaulerPortal_Jobs_Reject)]
        public async Task RejectJob(RejectLeaseHaulerRequestDto input)
        {
            if (input.Id <= 0)
            {
                throw new ArgumentException("Unable to reject lease hauler request");
            }

            var request = await _leaseHaulerRequestRepository.GetAsync(input.Id);
            await CheckEntitySpecificPermissions(
                anyEntityPermissionName: null,
                specificEntityPermissionName: AppPermissions.LeaseHaulerPortal_Jobs_Reject,
                Session.LeaseHaulerId,
                request.LeaseHaulerId);
            request.Comments = input.Comments;
            request.Status = LeaseHaulerRequestStatus.Rejected;
        }
    }
}
