using System;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Threading.Tasks;
using Abp.Application.Services.Dto;
using Abp.Authorization;
using Abp.Domain.Repositories;
using Abp.Domain.Uow;
using Abp.Extensions;
using Abp.Linq.Extensions;
using Abp.Runtime.Session;
using Abp.Timing;
using Abp.UI;
using DispatcherWeb.Authorization;
using DispatcherWeb.Authorization.Roles;
using DispatcherWeb.Authorization.Users;
using DispatcherWeb.Authorization.Users.Dto;
using DispatcherWeb.Caching;
using DispatcherWeb.Configuration;
using DispatcherWeb.Dispatching;
using DispatcherWeb.Drivers;
using DispatcherWeb.Dto;
using DispatcherWeb.Features;
using DispatcherWeb.Infrastructure.Extensions;
using DispatcherWeb.LeaseHaulerRequests;
using DispatcherWeb.LeaseHaulers.Dto;
using DispatcherWeb.LeaseHaulers.Exporting;
using DispatcherWeb.LeaseHaulerUsers;
using DispatcherWeb.Offices;
using DispatcherWeb.Orders;
using DispatcherWeb.Sessions;
using DispatcherWeb.Trucks;
using DispatcherWeb.Trucks.Dto;
using DispatcherWeb.Url;
using DispatcherWeb.VehicleCategories;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace DispatcherWeb.LeaseHaulers
{
    [AbpAuthorize(AppPermissions.Pages_LeaseHauler, AppPermissions.LeaseHaulerPortal)]
    public class LeaseHaulerAppService : DispatcherWebAppServiceBase, ILeaseHaulerAppService
    {
        public IAppUrlService AppUrlService { get; set; }

        private readonly IRepository<LeaseHauler> _leaseHaulerRepository;
        private readonly IRepository<LeaseHaulerContact> _leaseHaulerContactRepository;
        private readonly IRepository<LeaseHaulerTruck> _leaseHaulerTruckRepository;
        private readonly IRepository<LeaseHaulerDriver> _leaseHaulerDriverRepository;
        private readonly IRepository<AvailableLeaseHaulerTruck> _availableLeaseHaulerTruckRepository;
        private readonly IRepository<Driver> _driverRepository;
        private readonly IRepository<Truck> _truckRepository;
        private readonly IRepository<Dispatch> _dispatchRepository;
        private readonly IRepository<Ticket> _ticketRepository;
        private readonly IRepository<OrderLineTruck> _orderLineTruckRepository;
        private readonly IRepository<VehicleCategory> _vehicleCategoryRepository;
        private readonly IRepository<OneTimeLogin, Guid> _oneTimeLoginRepository;
        private readonly ListCacheCollection _listCaches;
        private readonly ILeaseHaulerListCsvExporter _leaseHaulerListCsvExporter;
        private readonly IDriverAppService _driverAppService;
        private readonly ITruckAppService _truckAppService;
        private readonly IUserCreatorService _userCreatorService;
        private readonly IDriverUserLinkService _driverUserLinkService;
        private readonly IDriverInactivatorService _driverInactivatorService;
        private readonly ISingleOfficeAppService _singleOfficeService;
        private readonly IPasswordHasher<User> _passwordHasher;
        private readonly IUserEmailer _userEmailer;
        private readonly ICrossTenantOrderSender _crossTenantOrderSender;
        private readonly ILeaseHaulerUserAppService _leaseHaulerUserService;
        private readonly IConfigurationRoot _appConfiguration;

        public LeaseHaulerAppService(
            IRepository<LeaseHauler> leaseHaulerRepository,
            IRepository<LeaseHaulerContact> leaseHaulerContactRepository,
            IRepository<LeaseHaulerTruck> leaseHaulerTruckRepository,
            IRepository<LeaseHaulerDriver> leaseHaulerDriverRepository,
            IRepository<AvailableLeaseHaulerTruck> availableLeaseHaulerTruckRepository,
            IRepository<Driver> driverRepository,
            IRepository<Truck> truckRepository,
            IRepository<Dispatch> dispatchRepository,
            IRepository<Ticket> ticketRepository,
            IRepository<OrderLineTruck> orderLineTruckRepository,
            IRepository<VehicleCategory> vehicleCategoryRepository,
            IRepository<OneTimeLogin, Guid> oneTimeLoginRepository,
            ListCacheCollection listCaches,
            ILeaseHaulerListCsvExporter leaseHaulerListCsvExporter,
            IDriverAppService driverAppService,
            ITruckAppService truckAppService,
            IUserCreatorService userCreatorService,
            IDriverUserLinkService driverUserLinkService,
            IDriverInactivatorService driverInactivatorService,
            ISingleOfficeAppService singleOfficeService,
            IPasswordHasher<User> passwordHasher,
            IUserEmailer userEmailer,
            ICrossTenantOrderSender crossTenantOrderSender,
            ILeaseHaulerUserAppService leaseHaulerUserService,
            IAppConfigurationAccessor configurationAccessor
            )
        {
            _leaseHaulerRepository = leaseHaulerRepository;
            _leaseHaulerContactRepository = leaseHaulerContactRepository;
            _leaseHaulerTruckRepository = leaseHaulerTruckRepository;
            _leaseHaulerDriverRepository = leaseHaulerDriverRepository;
            _availableLeaseHaulerTruckRepository = availableLeaseHaulerTruckRepository;
            _driverRepository = driverRepository;
            _truckRepository = truckRepository;
            _dispatchRepository = dispatchRepository;
            _ticketRepository = ticketRepository;
            _orderLineTruckRepository = orderLineTruckRepository;
            _vehicleCategoryRepository = vehicleCategoryRepository;
            _oneTimeLoginRepository = oneTimeLoginRepository;
            _listCaches = listCaches;
            _leaseHaulerListCsvExporter = leaseHaulerListCsvExporter;
            _driverAppService = driverAppService;
            _truckAppService = truckAppService;
            _userCreatorService = userCreatorService;
            _driverUserLinkService = driverUserLinkService;
            _driverInactivatorService = driverInactivatorService;
            _singleOfficeService = singleOfficeService;
            _passwordHasher = passwordHasher;
            _userEmailer = userEmailer;
            _crossTenantOrderSender = crossTenantOrderSender;
            _leaseHaulerUserService = leaseHaulerUserService;
            _appConfiguration = configurationAccessor.Configuration;
            AppUrlService = NullAppUrlService.Instance;
        }

        [AbpAuthorize(AppPermissions.Pages_LeaseHaulers_Edit)]
        public async Task<PagedResultDto<LeaseHaulerDto>> GetLeaseHaulers(GetLeaseHaulersInput input)
        {
            var query = await GetFilteredLeaseHaulerQueryAsync(input);

            var totalCount = await query.CountAsync();

            var items = await query
                .Select(x => new LeaseHaulerDto
                {
                    Id = x.Id,
                    Name = x.Name,
                    City = x.City,
                    State = x.State,
                    ZipCode = x.ZipCode,
                    CountryCode = x.CountryCode,
                    AccountNumber = x.AccountNumber,
                    StreetAddress1 = x.StreetAddress1,
                    PhoneNumber = x.PhoneNumber,
                    IsActive = x.IsActive,
                    Insurances = x.LeaseHaulerInsurances
                        .Where(i => i.IsActive)
                        .Select(i => new LeaseHaulerInsuranceDto
                        {
                            Id = i.Id,
                            InsuranceTypeName = i.InsuranceType.Name,
                            ExpirationDate = i.ExpirationDate,
                        })
                        .ToList(),
                })
                .OrderBy(input.Sorting)
                .PageBy(input)
                .ToListAsync();

            return new PagedResultDto<LeaseHaulerDto>(
                totalCount,
                items);
        }

        private async Task<IQueryable<LeaseHauler>> GetFilteredLeaseHaulerQueryAsync(IGetLeaseHaulerListFilter input)
        {
            return (await _leaseHaulerRepository.GetQueryAsync())
                .WhereIf(!input.Name.IsNullOrEmpty(), x => x.Name.Contains(input.Name))
                .WhereIf(!input.City.IsNullOrEmpty(), x => x.City.StartsWith(input.City))
                .WhereIf(!input.State.IsNullOrEmpty(), x => x.State.StartsWith(input.State))
                .WhereIf(input.Status == FilterActiveStatus.Active, x => x.IsActive)
                .WhereIf(input.Status == FilterActiveStatus.Inactive, x => !x.IsActive);
        }

        [AbpAuthorize(AppPermissions.Pages_LeaseHaulers_Edit)]
        [HttpPost]
        public async Task<FileDto> GetLeaseHaulersToCsv(GetLeaseHaulersInput input)
        {
            var query = await GetFilteredLeaseHaulerQueryAsync(input);
            var items = await query
                .Select(x => new LeaseHaulerEditDto
                {
                    Id = x.Id,
                    Name = x.Name,
                    StreetAddress1 = x.StreetAddress1,
                    StreetAddress2 = x.StreetAddress2,
                    City = x.City,
                    State = x.State,
                    ZipCode = x.ZipCode,
                    CountryCode = x.CountryCode,
                    AccountNumber = x.AccountNumber,
                    PhoneNumber = x.PhoneNumber,
                    IsActive = x.IsActive,
                    HaulingCompanyTenantId = x.HaulingCompanyTenantId,
                })
                .OrderBy(input.Sorting)
                .ToListAsync();

            if (!items.Any())
            {
                throw new UserFriendlyException("There is no data to export!");
            }

            return await _leaseHaulerListCsvExporter.ExportToFileAsync(items);
        }

        [AbpAuthorize(AppPermissions.Pages_LeaseHauler)]
        public async Task<PagedResultDto<SelectListDto>> GetLeaseHaulersSelectList(GetLeaseHaulersSelectListInput input)
        {
            var query = (await _leaseHaulerRepository.GetQueryAsync())
                .WhereIf(!input.IncludeInactive, x => x.IsActive)
                .WhereIf(input.HasHaulingCompanyTenantId == true, x => x.HaulingCompanyTenantId.HasValue)
                .WhereIf(input.HasHaulingCompanyTenantId == false, x => x.HaulingCompanyTenantId == null)
                .Select(x => new SelectListDto
                {
                    Id = x.Id.ToString(),
                    Name = x.Name,
                });

            return await query.GetSelectListResult(input);
        }

        [AbpAuthorize(AppPermissions.Pages_LeaseHaulers_Edit)]
        public async Task<LeaseHaulerEditDto> GetLeaseHaulerForEdit(NullableIdDto input)
        {
            LeaseHaulerEditDto leaseHaulerEditDto;

            if (input.Id.HasValue)
            {
                var leaseHauler = await _leaseHaulerRepository.GetAsync(input.Id.Value);
                leaseHaulerEditDto = new LeaseHaulerEditDto
                {
                    Id = leaseHauler.Id,
                    Name = leaseHauler.Name,
                    StreetAddress1 = leaseHauler.StreetAddress1,
                    StreetAddress2 = leaseHauler.StreetAddress2,
                    City = leaseHauler.City,
                    State = leaseHauler.State,
                    ZipCode = leaseHauler.ZipCode,
                    CountryCode = leaseHauler.CountryCode,
                    AccountNumber = leaseHauler.AccountNumber,
                    PhoneNumber = leaseHauler.PhoneNumber,
                    MailingAddress1 = leaseHauler.MailingAddress1,
                    MailingAddress2 = leaseHauler.MailingAddress2,
                    MailingCity = leaseHauler.MailingCity,
                    MailingState = leaseHauler.MailingState,
                    MailingCountryCode = leaseHauler.MailingCountryCode,
                    MailingZipCode = leaseHauler.MailingZipCode,
                    MotorCarrierNumber = leaseHauler.MotorCarrierNumber,
                    DeptOfTransportationNumber = leaseHauler.DeptOfTransportationNumber,
                    EinOrTin = leaseHauler.EinOrTin,
                    HireDate = leaseHauler.HireDate,
                    TerminationDate = leaseHauler.TerminationDate,
                    IsActive = leaseHauler.IsActive,
                    HaulingCompanyTenantId = leaseHauler.HaulingCompanyTenantId,
                };
            }
            else
            {
                leaseHaulerEditDto = new LeaseHaulerEditDto
                {
                    IsActive = true,
                };
            }

            return leaseHaulerEditDto;
        }

        [AbpAuthorize(AppPermissions.Pages_LeaseHaulers_Edit, AppPermissions.LeaseHaulerPortal_MyCompany)]
        public async Task<int> EditLeaseHauler(LeaseHaulerEditDto model)
        {
            var permissions = new
            {
                EditLeaseHauler = await IsGrantedAsync(AppPermissions.Pages_LeaseHaulers_Edit),
                EditLeaseHaulerPortalMyCompany = await IsGrantedAsync(AppPermissions.LeaseHaulerPortal_MyCompany),
            };

            var leaseHauler = model.Id.HasValue ? await _leaseHaulerRepository.GetAsync(model.Id.Value) : new LeaseHauler();

            if (await (await _leaseHaulerRepository.GetQueryAsync()).AnyAsync(lh => lh.Id != leaseHauler.Id && lh.Name == model.Name))
            {
                throw new UserFriendlyException($"A Lease Hauler with name '{model.Name}' already exists!");
            }

            leaseHauler.Name = model.Name;
            leaseHauler.StreetAddress1 = model.StreetAddress1;
            leaseHauler.StreetAddress2 = model.StreetAddress2;
            leaseHauler.City = model.City;
            leaseHauler.State = model.State;
            leaseHauler.ZipCode = model.ZipCode;
            leaseHauler.CountryCode = model.CountryCode;
            leaseHauler.PhoneNumber = model.PhoneNumber;
            leaseHauler.MailingAddress1 = model.MailingAddress1;
            leaseHauler.MailingAddress2 = model.MailingAddress2;
            leaseHauler.MailingCity = model.MailingCity;
            leaseHauler.MailingState = model.MailingState;
            leaseHauler.MailingCountryCode = model.MailingCountryCode;
            leaseHauler.MailingZipCode = model.MailingZipCode;
            leaseHauler.MotorCarrierNumber = model.MotorCarrierNumber;
            leaseHauler.DeptOfTransportationNumber = model.DeptOfTransportationNumber;
            leaseHauler.EinOrTin = model.EinOrTin;
            leaseHauler.HireDate = model.HireDate;
            leaseHauler.TerminationDate = model.TerminationDate;

            if (permissions.EditLeaseHauler)
            {
                leaseHauler.AccountNumber = model.AccountNumber;
                leaseHauler.IsActive = model.IsActive;
            }
            else if (permissions.EditLeaseHaulerPortalMyCompany)
            {
                var leaseHaulerFilter = Session.GetLeaseHaulerIdOrThrow(this);
                if (leaseHaulerFilter != (leaseHauler.Id > 0 ? leaseHauler.Id : model.Id))
                {
                    throw new AbpAuthorizationException();
                }
            }
            else
            {
                throw new AbpAuthorizationException();
            }

            if (await FeatureChecker.IsEnabledAsync(AppFeatures.AllowSendingOrdersToDifferentTenant)
                && await PermissionChecker.IsGrantedAsync(AppPermissions.Pages_LeaseHaulers_SetHaulingCompanyTenantId))
            {
                if (model.HaulingCompanyTenantId.HasValue)
                {
                    if (model.HaulingCompanyTenantId == await AbpSession.GetTenantIdAsync())
                    {
                        throw new UserFriendlyException($"{model.HaulingCompanyTenantId} is your own tenant id");
                    }

                    if (!await (await TenantManager.GetQueryAsync()).AnyAsync(x => x.Id == model.HaulingCompanyTenantId))
                    {
                        throw new UserFriendlyException($"Tenant with id {model.HaulingCompanyTenantId} wasn't found");
                    }
                }
                leaseHauler.HaulingCompanyTenantId = model.HaulingCompanyTenantId;
            }

            if (model.Id.HasValue)
            {
                return model.Id.Value;
            }

            return await _leaseHaulerRepository.InsertAndGetIdAsync(leaseHauler);
        }

        //*************************************************//

        [AbpAuthorize(AppPermissions.Pages_LeaseHauler, AppPermissions.LeaseHaulerPortal_MyCompany_Contacts)]
        public async Task<PagedResultDto<LeaseHaulerContactDto>> GetLeaseHaulerContacts(GetLeaseHaulerContactsInput input)
        {
            await CheckEntitySpecificPermissions(
                AppPermissions.Pages_LeaseHauler,
                AppPermissions.LeaseHaulerPortal_MyCompany_Contacts,
                Session.LeaseHaulerId,
                input.LeaseHaulerId);

            var query = (await _leaseHaulerContactRepository.GetQueryAsync())
                .Where(x => x.LeaseHaulerId == input.LeaseHaulerId);

            var totalCount = await query.CountAsync();

            var items = await query
                .Select(x => new LeaseHaulerContactDto
                {
                    Id = x.Id,
                    LeaseHaulerId = x.LeaseHaulerId,
                    Name = x.Name,
                    Phone = x.Phone,
                    Email = x.Email,
                    CellPhoneNumber = x.CellPhoneNumber,
                    Title = x.Title,
                })
                .OrderBy(input.Sorting)
                .ToListAsync();

            return new PagedResultDto<LeaseHaulerContactDto>(
                totalCount,
                items);
        }

        [AbpAuthorize(AppPermissions.Pages_LeaseHauler, AppPermissions.LeaseHaulerPortal_MyCompany_Trucks)]
        public async Task<PagedResultDto<LeaseHaulerTruckDto>> GetLeaseHaulerTrucks(GetLeaseHaulerTrucksInput input)
        {
            await CheckEntitySpecificPermissions(
                AppPermissions.Pages_LeaseHauler,
                AppPermissions.LeaseHaulerPortal_MyCompany_Trucks,
                Session.LeaseHaulerId,
                input.LeaseHaulerId);

            var query = (await _leaseHaulerTruckRepository.GetQueryAsync())
                .Where(x => x.LeaseHaulerId == input.LeaseHaulerId);

            var totalCount = await query.CountAsync();

            var items = await query
                .Select(x => x.Truck)
                .Select(x => new LeaseHaulerTruckDto
                {
                    Id = x.Id,
                    TruckCode = x.TruckCode,
                    VehicleCategoryName = x.VehicleCategory.Name,
                    DefaultDriverName = x.DefaultDriver.FirstName + " " + x.DefaultDriver.LastName,
                    IsActive = x.IsActive,
                    AlwaysShowOnSchedule = x.LeaseHaulerTruck.AlwaysShowOnSchedule,
                })
                .OrderBy(input.Sorting)
                .ToListAsync();

            return new PagedResultDto<LeaseHaulerTruckDto>(
                totalCount,
                items);
        }

        [AbpAuthorize(AppPermissions.Pages_LeaseHauler, AppPermissions.LeaseHaulerPortal_MyCompany_Drivers)]
        public async Task<PagedResultDto<LeaseHaulerDriverDto>> GetLeaseHaulerDrivers(GetLeaseHaulerDriversInput input)
        {
            await CheckEntitySpecificPermissions(
                AppPermissions.Pages_LeaseHauler,
                AppPermissions.LeaseHaulerPortal_MyCompany_Drivers,
                Session.LeaseHaulerId,
                input.LeaseHaulerId);

            var query = (await _leaseHaulerDriverRepository.GetQueryAsync())
                .Where(x => x.LeaseHaulerId == input.LeaseHaulerId);

            var totalCount = await query.CountAsync();

            var items = await query
                .Select(x => x.Driver)
                .Select(x => new LeaseHaulerDriverDto
                {
                    Id = x.Id,
                    FirstName = x.FirstName,
                    LastName = x.LastName,
                    IsInactive = x.IsInactive,
                })
                .OrderBy(input.Sorting)
                .ToListAsync();

            return new PagedResultDto<LeaseHaulerDriverDto>(
                totalCount,
                items);
        }

        [AbpAllowAnonymous]
        public async Task<PagedResultDto<SelectListDto>> GetLeaseHaulerDriversSelectList(GetLeaseHaulerDriversSelectListInput input)
        {
            using (CurrentUnitOfWork.DisableFilter(AbpDataFilters.MustHaveTenant, AbpDataFilters.MayHaveTenant))
            {
                var tenantId = await Session.GetTenantIdAsync();
                return await (await _leaseHaulerDriverRepository.GetQueryAsync())
                    .WhereIf(input.LeaseHaulerId.HasValue, x => x.LeaseHaulerId == input.LeaseHaulerId)
                    .WhereIf(!input.LeaseHaulerId.HasValue, x => x.TenantId == tenantId)
                    .Select(x => x.Driver)
                    .SelectIdName()
                    .GetSelectListResult(input);
            }
        }

        [AbpAllowAnonymous]
        public async Task<PagedResultDto<SelectListDto>> GetLeaseHaulerTrucksSelectList(GetLeaseHaulerTrucksSelectListInput input)
        {
            using (CurrentUnitOfWork.DisableFilter(AbpDataFilters.MustHaveTenant, AbpDataFilters.MayHaveTenant))
            {
                return await (await _leaseHaulerTruckRepository.GetQueryAsync())
                    .Where(x => x.LeaseHaulerId == input.LeaseHaulerId)
                    .WhereIf(input.AssetType.HasValue, x => x.Truck.VehicleCategory.AssetType == input.AssetType.Value)
                    .Select(x => x.Truck)
                    .Where(x => x.IsActive)
                    .Select(x => new SelectListDto
                    {
                        Id = x.Id.ToString(),
                        Name = x.TruckCode,
                    })
                    .GetSelectListResult(input);
            }
        }

        [AbpAuthorize(AppPermissions.Pages_LeaseHaulers_Edit, AppPermissions.LeaseHaulerPortal_MyCompany_Contacts)]
        public async Task<ListResultDto<LeaseHaulerContactSelectListDto>> GetLeaseHaulerContactSelectList(int leaseHaulerId, int? leaseHaulerContactId, LeaseHaulerMessageType messageType)
        {
            await CheckEntitySpecificPermissions(
                AppPermissions.Pages_LeaseHaulers_Edit,
                AppPermissions.LeaseHaulerPortal_MyCompany_Contacts,
                Session.LeaseHaulerId,
                leaseHaulerId);

            var contacts = await (await _leaseHaulerContactRepository.GetQueryAsync())
                .Where(lhc => lhc.LeaseHaulerId == leaseHaulerId)
                .WhereIf(messageType == LeaseHaulerMessageType.Sms, lhc => !string.IsNullOrEmpty(lhc.CellPhoneNumber))
                .WhereIf(messageType == LeaseHaulerMessageType.Email, lhc => !string.IsNullOrEmpty(lhc.Email))
                .WhereIf(leaseHaulerContactId.HasValue, lhc => lhc.Id == leaseHaulerContactId)
                .OrderBy(lhc => lhc.Name)
                .Select(lhc => new LeaseHaulerContactSelectListDto
                {
                    Id = lhc.Id.ToString(),
                    Name = lhc.Name,
                    IsDefault = lhc.IsDispatcher || leaseHaulerContactId.HasValue,
                })
                .ToListAsync();
            return new ListResultDto<LeaseHaulerContactSelectListDto>(contacts);
        }

        [AbpAuthorize(AppPermissions.Pages_LeaseHaulers_Edit, AppPermissions.LeaseHaulerPortal_MyCompany_Contacts)]
        public async Task<LeaseHaulerContactEditDto> GetLeaseHaulerContactForEdit(NullableIdDto input)
        {
            LeaseHaulerContactEditDto leaseHaulerContactEditDto;

            if (input.Id.HasValue)
            {
                leaseHaulerContactEditDto = await (await _leaseHaulerContactRepository.GetQueryAsync())
                    .Where(x => x.Id == input.Id.Value)
                    .Select(x => new LeaseHaulerContactEditDto
                    {
                        Id = x.Id,
                        LeaseHaulerId = x.LeaseHaulerId,
                        Name = x.Name,
                        Phone = x.Phone,
                        Email = x.Email,
                        Title = x.Title,
                        CellPhoneNumber = x.CellPhoneNumber,
                        NotifyPreferredFormat = x.NotifyPreferredFormat,
                        IsDispatcher = x.IsDispatcher,
                        AllowPortalAccess = x.AllowPortalAccess,
                    })
                    .FirstAsync();

                await CheckEntitySpecificPermissions(
                    AppPermissions.Pages_LeaseHaulers_Edit,
                    AppPermissions.LeaseHaulerPortal_MyCompany_Contacts,
                    Session.LeaseHaulerId,
                    leaseHaulerContactEditDto.LeaseHaulerId);
            }
            else
            {
                leaseHaulerContactEditDto = new LeaseHaulerContactEditDto();
            }

            return leaseHaulerContactEditDto;
        }

        [AbpAuthorize(AppPermissions.Pages_LeaseHaulers_Edit, AppPermissions.LeaseHaulerPortal_MyCompany_Trucks)]
        public async Task<LeaseHaulerTruckEditDto> GetLeaseHaulerTruckForEdit(GetLeaseHaulerTruckForEditInput input)
        {
            LeaseHaulerTruckEditDto leaseHaulerTruckEditDto;

            if (input.Id.HasValue)
            {
                leaseHaulerTruckEditDto = await (await _truckRepository.GetQueryAsync())
                    .Select(t => new LeaseHaulerTruckEditDto
                    {
                        Id = t.Id,
                        TruckCode = t.TruckCode,
                        LeaseHaulerId = t.LeaseHaulerTruck.LeaseHaulerId,
                        LicensePlate = t.Plate,
                        VehicleCategoryId = t.VehicleCategoryId,
                        VehicleCategoryName = t.VehicleCategory.Name,
                        DefaultDriverId = t.DefaultDriverId,
                        DefaultDriverName = t.DefaultDriver != null ? t.DefaultDriver.FirstName + " " + t.DefaultDriver.LastName : "",
                        IsActive = t.IsActive,
                        InactivationDate = t.InactivationDate,
                        CanPullTrailer = t.CanPullTrailer,
                        AlwaysShowOnSchedule = t.LeaseHaulerTruck.AlwaysShowOnSchedule,
                        OfficeId = t.OfficeId,
                        OfficeName = t.Office.Name,
                        VehicleCategoryIsPowered = t.VehicleCategory.IsPowered,
                        VehicleCategoryAssetType = t.VehicleCategory.AssetType,
                        HaulingCompanyTruckId = t.HaulingCompanyTruckId,
                        CurrentTrailerId = t.CurrentTrailerId,
                        CurrentTrailerCode = t.CurrentTrailer.TruckCode,
                        BedConstruction = t.BedConstruction,
                        IsApportioned = t.IsApportioned,
                    })
                    .SingleAsync(t => t.Id == input.Id.Value);
            }
            else
            {
                leaseHaulerTruckEditDto = new LeaseHaulerTruckEditDto
                {
                    LeaseHaulerId = input.LeaseHaulerId ?? 0,
                    IsActive = true,
                };

                if (input.VehicleCategoryId.HasValue)
                {
                    var vehicleCategory = await (await _vehicleCategoryRepository.GetQueryAsync())
                        .Where(x => x.Id == input.VehicleCategoryId)
                        .Select(x => new
                        {
                            x.Id,
                            x.Name,
                            x.AssetType,
                            x.IsPowered,
                        }).FirstOrDefaultAsync();

                    if (vehicleCategory != null)
                    {
                        leaseHaulerTruckEditDto.VehicleCategoryId = vehicleCategory.Id;
                        leaseHaulerTruckEditDto.VehicleCategoryName = vehicleCategory.Name;
                        leaseHaulerTruckEditDto.VehicleCategoryAssetType = vehicleCategory.AssetType;
                        leaseHaulerTruckEditDto.VehicleCategoryIsPowered = vehicleCategory.IsPowered;
                    }
                }
            }

            await _singleOfficeService.FillSingleOffice(leaseHaulerTruckEditDto);

            await CheckEntitySpecificPermissions(
                AppPermissions.Pages_LeaseHaulers_Edit,
                AppPermissions.LeaseHaulerPortal_MyCompany_Trucks,
                Session.LeaseHaulerId,
                leaseHaulerTruckEditDto.LeaseHaulerId
            );

            return leaseHaulerTruckEditDto;
        }

        [AbpAuthorize(AppPermissions.Pages_LeaseHaulers_Edit, AppPermissions.LeaseHaulerPortal_MyCompany_Drivers)]
        public async Task<LeaseHaulerDriverEditDto> GetLeaseHaulerDriverForEdit(NullableIdDto input)
        {
            LeaseHaulerDriverEditDto leaseHaulerDriverEditDto;

            if (input.Id.HasValue)
            {
                leaseHaulerDriverEditDto = await (await _driverRepository.GetQueryAsync())
                    .Select(x => new LeaseHaulerDriverEditDto
                    {
                        Id = x.Id,
                        FirstName = x.FirstName,
                        LastName = x.LastName,
                        DriverIsActive = !x.IsInactive,
                        EmailAddress = x.EmailAddress,
                        CellPhoneNumber = x.CellPhoneNumber,
                        OrderNotifyPreferredFormat = x.OrderNotifyPreferredFormat,
                        UserId = x.UserId,
                        HaulingCompanyDriverId = x.HaulingCompanyDriverId,
                        LeaseHaulerId = x.LeaseHaulerDriver.LeaseHaulerId,
                    })
                    .SingleAsync(t => t.Id == input.Id.Value);

                var user = await GetUserForLhDriver(leaseHaulerDriverEditDto.EmailAddress, leaseHaulerDriverEditDto.UserId);
                leaseHaulerDriverEditDto.EnableForDriverApplication = user != null && await UserManager.IsInRoleAsync(user, StaticRoleNames.Tenants.LeaseHaulerDriver);

                leaseHaulerDriverEditDto.SetRandomPassword = user == null;
                leaseHaulerDriverEditDto.ShouldChangePasswordOnNextLogin = user == null;
                leaseHaulerDriverEditDto.SendActivationEmail = user == null;

                await CheckEntitySpecificPermissions(
                    AppPermissions.Pages_LeaseHaulers_Edit,
                    AppPermissions.LeaseHaulerPortal_MyCompany_Drivers,
                    Session.LeaseHaulerId,
                    leaseHaulerDriverEditDto.LeaseHaulerId);
            }
            else
            {
                leaseHaulerDriverEditDto = new LeaseHaulerDriverEditDto
                {
                    DriverIsActive = true,
                    SetRandomPassword = true,
                    ShouldChangePasswordOnNextLogin = true,
                    SendActivationEmail = true,
                };
            }

            return leaseHaulerDriverEditDto;
        }

        [AbpAuthorize(AppPermissions.LeaseHaulerPortal)]
        public async Task<string> GetLeaseHaulerCompanyName()
        {
            var leaseHaulerId = Session.GetLeaseHaulerIdOrThrow(this);
            var leaseHaulerName = await (await _leaseHaulerRepository.GetQueryAsync())
                .Where(q => q.Id == leaseHaulerId)
                .Select(s => s.Name)
                .FirstAsync();
            return leaseHaulerName;
        }

        private int GetOneTimeLoginLinkLifetimeInHours()
        {
            const int defaultLifetime = 720;
            var lifetimeString = _appConfiguration["App:OneTimeLoginLinkLifetimeInHours"];
            if (!string.IsNullOrEmpty(lifetimeString) && int.TryParse(lifetimeString, out var lifetime))
            {
                return lifetime;
            }
            return defaultLifetime;
        }

        [AbpAuthorize(AppPermissions.Pages_LeaseHaulers_Edit, AppPermissions.LeaseHaulerPortal_MyCompany_Contacts)]
        public async Task<int> EditLeaseHaulerContact(LeaseHaulerContactEditDto model)
        {
            var leaseHaulerContact = model.Id.HasValue ? await _leaseHaulerContactRepository.GetAsync(model.Id.Value) : new LeaseHaulerContact();

            await CheckEntitySpecificPermissions(
                AppPermissions.Pages_LeaseHaulers_Edit,
                AppPermissions.LeaseHaulerPortal_MyCompany_Contacts,
                Session.LeaseHaulerId,
                leaseHaulerContact.Id > 0 ? leaseHaulerContact.LeaseHaulerId : model.LeaseHaulerId);

            leaseHaulerContact.LeaseHaulerId = model.LeaseHaulerId;
            leaseHaulerContact.Name = model.Name;
            leaseHaulerContact.Phone = model.Phone;
            leaseHaulerContact.Email = model.Email;
            leaseHaulerContact.Title = model.Title;
            leaseHaulerContact.CellPhoneNumber = model.CellPhoneNumber;
            leaseHaulerContact.NotifyPreferredFormat = model.NotifyPreferredFormat;
            leaseHaulerContact.IsDispatcher = model.IsDispatcher;
            leaseHaulerContact.AllowPortalAccess = model.AllowPortalAccess;

            var result = await _leaseHaulerContactRepository.InsertOrUpdateAndGetIdAsync(leaseHaulerContact);

            if (model.AllowPortalAccess)
            {
                var user = await UserManager.FindByEmailAsync(leaseHaulerContact.Email);
                if (user == null)
                {
                    var nameParts = leaseHaulerContact.Name.Split(' ');
                    var newUser = new UserEditDto
                    {
                        EmailAddress = leaseHaulerContact.Email,
                        Name = nameParts.First(),
                        Surname = nameParts.Length > 1 ? nameParts[1] : nameParts.First(),
                        PhoneNumber = leaseHaulerContact.Phone,
                        UserName = leaseHaulerContact.Email.Split("@").First(),
                        IsActive = true,
                        IsLockoutEnabled = true,
                        ShouldChangePasswordOnNextLogin = true,
                    };

                    var createOrUpdateUserInput = new CreateOrUpdateUserInput
                    {
                        User = newUser,
                        AssignedRoleNames = new[] { StaticRoleNames.Tenants.LeaseHaulerDispatcher },
                        SendActivationEmail = false,
                        SetRandomPassword = true,
                    };

                    user = await _userCreatorService.CreateUser(createOrUpdateUserInput);
                    await _leaseHaulerUserService.UpdateLeaseHaulerUser(leaseHaulerContact.LeaseHaulerId, user.Id, user.TenantId);
                }

                if (model.SendInviteEmail)
                {
                    var oneTimeLogin = new OneTimeLogin
                    {
                        Id = Guid.NewGuid(),
                        UserId = user.Id,
                        ExpiryTime = Clock.Now.AddHours(GetOneTimeLoginLinkLifetimeInHours()),
                    };
                    await _oneTimeLoginRepository.InsertAndGetIdAsync(oneTimeLogin);

                    await _userEmailer.SendLeaseHaulerInviteEmail(
                        user,
                        AppUrlService.CreateLeaseHaulerInvitationUrlFormat(oneTimeLogin.Id),
                        await SettingManager.GetSettingValueAsync(AppSettings.EmailTemplate.LeaseHaulerInviteEmailSubjectTemplate),
                        await SettingManager.GetSettingValueAsync(AppSettings.EmailTemplate.LeaseHaulerInviteEmailBodyTemplate)
                    );
                }
            }

            return result;
        }

        [AbpAuthorize(AppPermissions.Pages_LeaseHaulers_Edit, AppPermissions.LeaseHaulerPortal_MyCompany_Trucks)]
        public async Task<EditLeaseHaulerTruckResult> EditLeaseHaulerTruck(LeaseHaulerTruckEditDto model)
        {
            var permissions = new
            {
                EditLeaseHaulerTrucks = await IsGrantedAsync(AppPermissions.Pages_LeaseHaulers_Edit),
                EditLeaseHaulerPortalTrucks = await IsGrantedAsync(AppPermissions.LeaseHaulerPortal_MyCompany_Trucks),
            };

            var truck = model.Id.HasValue
                ? await (await _truckRepository.GetQueryAsync())
                    .Include(t => t.LeaseHaulerTruck)
                    .FirstAsync(t => t.Id == model.Id.Value)
            : new Truck();

            if (permissions.EditLeaseHaulerTrucks)
            {
                // do nothing
            }
            else if (permissions.EditLeaseHaulerPortalTrucks)
            {
                var leaseHaulerFilter = Session.GetLeaseHaulerIdOrThrow(this);
                if (leaseHaulerFilter != (truck.Id > 0 ? truck.LeaseHaulerTruck?.LeaseHaulerId : model.LeaseHaulerId))
                {
                    throw new AbpAuthorizationException();
                }
            }
            else
            {
                throw new AbpAuthorizationException();
            }

            if (truck.HaulingCompanyTruckId != null)
            {
                throw new UserFriendlyException(L("CannotEditTruckLinkedToHaulingCompany"));
            }

            if (await (await _truckRepository.GetQueryAsync())
                .AnyAsync(t => t.LeaseHaulerTruck.LeaseHaulerId == model.LeaseHaulerId
                    && t.TruckCode == model.TruckCode
                    && t.Id != truck.Id))
            {
                throw new UserFriendlyException(L("TruckCode{0}AlreadyExistsForLeaseHauler", model.TruckCode));
            }

            if (model.AlwaysShowOnSchedule && !model.OfficeId.HasValue)
            {
                throw new UserFriendlyException("Office is required");
            }

            var vehicleCategories = await _listCaches.VehicleCategory.GetList(ListCacheEmptyKey.Instance);


            var newVehicleCategory = vehicleCategories.Find(model.VehicleCategoryId);
            if (newVehicleCategory == null)
            {
                throw new UserFriendlyException("Category is required");
            }
            var oldVehicleCategory = truck.VehicleCategoryId == model.VehicleCategoryId
                ? newVehicleCategory
                : truck.VehicleCategoryId != 0
                    ? vehicleCategories.Find(truck.VehicleCategoryId)
                    : null;

            if ((!model.Id.HasValue || newVehicleCategory != oldVehicleCategory || truck.LeaseHaulerTruck?.AlwaysShowOnSchedule != model.AlwaysShowOnSchedule) && newVehicleCategory.IsPowered && model.AlwaysShowOnSchedule)
            {
                var currentNumberOfTrucks = await _truckRepository.CountAsync(t => t.OfficeId != null && t.VehicleCategory.IsPowered && t.Id != model.Id) + 1;
                var maxNumberOfTrucks = (await FeatureChecker.GetValueAsync(AppFeatures.NumberOfTrucksFeature)).To<int>();
                if (currentNumberOfTrucks > maxNumberOfTrucks)
                {
                    return new EditLeaseHaulerTruckResult
                    {
                        NeededBiggerNumberOfTrucks = currentNumberOfTrucks,
                    };
                }
            }

            if (newVehicleCategory.AssetType == AssetType.Trailer && model.DefaultDriverId.HasValue)
            {
                throw new ArgumentException("A Trailer cannot have a default driver");
            }

            if (!newVehicleCategory.IsPowered && model.DefaultDriverId.HasValue)
            {
                throw new ArgumentException("An unpowered vehicle cannot have a default driver!");
            }

            if (model.Id.HasValue && newVehicleCategory.AssetType == AssetType.Trailer && model.IsActive != true)
            {
                var tractors = await (await _truckRepository.GetQueryAsync()).Where(q => q.CurrentTrailerId == model.Id).ToListAsync();
                tractors.ForEach(t => t.CurrentTrailerId = null);
            }
            await ThrowUserFriendlyExceptionIfTruckWasTrailerAndCategoryChanged();

            truck.TruckCode = model.TruckCode;
            truck.Plate = model.LicensePlate;
            truck.VehicleCategoryId = model.VehicleCategoryId;
            truck.CanPullTrailer = model.CanPullTrailer;
            truck.InactivationDate = model.IsActive ? null : model.InactivationDate;
            truck.DefaultDriverId = model.DefaultDriverId;
            truck.OfficeId = model.AlwaysShowOnSchedule ? model.OfficeId : null;
            truck.BedConstruction = model.BedConstruction;
            truck.IsApportioned = model.IsApportioned;

            if (truck.IsActive
                && !model.IsActive
                && model.Id != null)
            {
                if (await TruckHasUpcomingLeaseHaulerRequests(model.Id.Value))
                {
                    throw new UserFriendlyException(L("UnableToInactivateLhTruckWithRequests"));
                }
                if (await TruckHasOpenDispatchesOrOrderLines(model.Id.Value))
                {
                    throw new UserFriendlyException(L("UnableToInactivateLhTruckWithOrdersOrDispatches"));
                }
            }
            truck.IsActive = model.IsActive;
            await UpdateCurrentTrailer();

            await _truckRepository.InsertOrUpdateAndGetIdAsync(truck);

            if (model.Id == null)
            {
                await _leaseHaulerTruckRepository.InsertAsync(new LeaseHaulerTruck
                {
                    TruckId = truck.Id,
                    LeaseHaulerId = model.LeaseHaulerId,
                    AlwaysShowOnSchedule = model.AlwaysShowOnSchedule,
                });
            }
            else
            {
                if (permissions.EditLeaseHaulerTrucks)
                {
                    truck.LeaseHaulerTruck.AlwaysShowOnSchedule = model.AlwaysShowOnSchedule;
                }
            }

            await _crossTenantOrderSender.SyncMaterialCompanyTrucksIfNeeded(truck.Id);

            return new EditLeaseHaulerTruckResult();

            async Task UpdateCurrentTrailer()
            {
                if (!truck.CanPullTrailer && model.CurrentTrailerId != null)
                {
                    throw new ArgumentException("The truck must be able to pull a trailer to set a CurrentTrailerId");
                }
                if (truck.CanPullTrailer && model.CurrentTrailerId != null)
                {
                    var trailer = await (await _truckRepository.GetQueryAsync())
                        .Where(t => t.Id == model.CurrentTrailerId)
                        .Select(t => new
                        {
                            t.VehicleCategory.AssetType,
                            t.IsActive,
                            t.IsOutOfService,
                        })
                        .FirstOrDefaultAsync();
                    if (trailer.AssetType != AssetType.Trailer)
                    {
                        throw new UserFriendlyException("The current trailer must be a trailer!");
                    }
                    if (!trailer.IsActive || trailer.IsOutOfService)
                    {
                        throw new UserFriendlyException("The current trailer must be active!");
                    }
                    var tractorWithCurrentTrailer = await (await _truckRepository.GetQueryAsync())
                        .WhereIf(model.Id != null, t => t.Id != model.Id)
                        .Where(t => t.CurrentTrailerId == model.CurrentTrailerId)
                        .FirstOrDefaultAsync();
                    if (tractorWithCurrentTrailer != null)
                    {
                        tractorWithCurrentTrailer.CurrentTrailerId = null;
                    }
                }

                truck.CurrentTrailerId = model.CurrentTrailerId;
            }

            async Task ThrowUserFriendlyExceptionIfTruckWasTrailerAndCategoryChanged()
            {
                if (model.Id != null
                    && oldVehicleCategory.AssetType == AssetType.Trailer
                    && newVehicleCategory.AssetType != AssetType.Trailer)
                {
                    if (model.IsActive)
                    {
                        var tractorCode = await _truckAppService.GetTractorWithCurrentTrailer(new TrailerIsSetAsCurrentTrailerForAnotherTractorInput { TrailerId = model.Id.Value });
                        if (!string.IsNullOrEmpty(tractorCode))
                        {
                            throw new UserFriendlyException($"This trailer is the current trailer on {tractorCode} and its category can't be changed while it's assigned to a truck.");
                        }
                    }
                    if (await TruckHasHistory(model.Id.Value))
                    {
                        throw new UserFriendlyException($"This trailer already has history, so its category can't be changed.");
                    }
                }
                if (model.Id != null
                    && oldVehicleCategory.AssetType != AssetType.Trailer
                    && newVehicleCategory.AssetType == AssetType.Trailer)
                {
                    if (await TruckHasHistory(model.Id.Value))
                    {
                        throw new UserFriendlyException($"This truck already has history, so its category can't be changed.");
                    }
                }
            }
        }
        [AbpAuthorize(AppPermissions.Pages_LeaseHaulers_Edit, AppPermissions.LeaseHaulerPortal_MyCompany_Trucks)]
        public async Task EditLeaseHaulerTruckFromList(EditLeaseHaulerTruckFromListInput input)
        {
            var permissions = new
            {
                EditLeaseHaulerTrucks = await IsGrantedAsync(AppPermissions.Pages_LeaseHaulers_Edit),
                EditLeaseHaulerPortalTrucks = await IsGrantedAsync(AppPermissions.LeaseHaulerPortal_MyCompany_Trucks),
            };

            var truck = await (await _truckRepository.GetQueryAsync())
                    .Include(t => t.LeaseHaulerTruck)
                    .FirstAsync(t => t.Id == input.Id);
            if (permissions.EditLeaseHaulerTrucks)
            {
                // do nothing
            }
            else if (permissions.EditLeaseHaulerPortalTrucks)
            {
                var leaseHaulerFilter = Session.GetLeaseHaulerIdOrThrow(this);
                if (leaseHaulerFilter != truck.LeaseHaulerTruck?.LeaseHaulerId)
                {
                    throw new AbpAuthorizationException();
                }
            }
            else
            {
                throw new AbpAuthorizationException();
            }
            truck.IsActive = input.IsActive;
            truck.LeaseHaulerTruck.AlwaysShowOnSchedule = input.AlwaysShowOnSchedule;
        }

        [AbpAuthorize(AppPermissions.Pages_LeaseHaulers_Edit, AppPermissions.LeaseHaulerPortal_MyCompany_Drivers)]
        public async Task EditLeaseHaulerDriver(LeaseHaulerDriverEditDto model)
        {
            var driver = model.Id.HasValue
                ? await (await _driverRepository.GetQueryAsync()).Include(t => t.LeaseHaulerDriver).FirstAsync(t => t.Id == model.Id.Value)
                : new Driver();

            await CheckEntitySpecificPermissions(
                AppPermissions.Pages_LeaseHaulers_Edit,
                AppPermissions.LeaseHaulerPortal_MyCompany_Drivers,
                Session.LeaseHaulerId,
                driver.Id > 0 ? driver.LeaseHaulerDriver?.LeaseHaulerId : model.LeaseHaulerId);

            if (driver.HaulingCompanyDriverId != null)
            {
                throw new UserFriendlyException(L("CannotEditDriverLinkedToHaulingCompany"));
            }

            driver.FirstName = model.FirstName;
            driver.LastName = model.LastName;
            driver.EmailAddress = model.EmailAddress;
            driver.CellPhoneNumber = model.CellPhoneNumber;
            driver.OrderNotifyPreferredFormat = model.OrderNotifyPreferredFormat;
            driver.IsExternal = true;

            if (!driver.IsInactive
                && !model.DriverIsActive
                && model.Id != null)
            {
                if (await DriverHasUpcomingLeaseHaulerRequests(model.Id.Value))
                {
                    throw new UserFriendlyException(L("UnableToInactivateLhDriverWithRequests"));
                }
            }
            if (!driver.IsInactive && !model.DriverIsActive)
            {
                driver.TerminationDate = await GetToday();
            }
            else if ((driver.IsInactive || model.Id == null) && model.DriverIsActive)
            {
                driver.DateOfHire = await GetToday();
                driver.TerminationDate = null;
            }
            driver.IsInactive = !model.DriverIsActive;

            await _driverRepository.InsertOrUpdateAndGetIdAsync(driver);

            if (model.Id == null)
            {
                await _leaseHaulerDriverRepository.InsertAsync(new LeaseHaulerDriver
                {
                    DriverId = driver.Id,
                    LeaseHaulerId = model.LeaseHaulerId,
                });
            }

            if (model.EnableForDriverApplication)
            {
                await CreateOrUpdateUserForLHDriver(driver, model);
            }
            else
            {
                await InactivateUserForLHDriver(driver);
            }

            await _crossTenantOrderSender.SyncMaterialCompanyDriversIfNeeded(driver.Id);
        }

        private async Task<bool> DriverHasUpcomingLeaseHaulerRequests(int driverId)
        {
            var today = await GetToday();
            return await (await _availableLeaseHaulerTruckRepository.GetQueryAsync())
                .AnyAsync(x => x.DriverId == driverId && x.Date >= today);
        }

        private async Task<bool> TruckHasUpcomingLeaseHaulerRequests(int truckId)
        {
            var today = await GetToday();
            return await (await _availableLeaseHaulerTruckRepository.GetQueryAsync())
                    .AnyAsync(x => x.TruckId == truckId && x.Date >= today);
        }

        private async Task<bool> TruckHasOpenDispatchesOrOrderLines(int truckId)
        {
            var today = await GetToday();
            return await (await _dispatchRepository.GetQueryAsync())
                    .AnyAsync(x => x.TruckId == truckId && !Dispatch.ClosedDispatchStatuses.Contains(x.Status))
                || await (await _orderLineTruckRepository.GetQueryAsync())
                    .AnyAsync(x => x.TruckId == truckId && !x.IsDone && !x.OrderLine.IsComplete && x.OrderLine.Order.DeliveryDate >= today);
        }

        private async Task<bool> TruckHasHistory(int truckId)
        {
            return await (await _ticketRepository.GetQueryAsync()).AnyAsync(x => x.TruckId == truckId)
                   || await (await _orderLineTruckRepository.GetQueryAsync()).AnyAsync(x => x.TruckId == truckId);
        }

        [AbpAuthorize(AppPermissions.Pages_LeaseHaulers_Edit)]
        public async Task DeleteLeaseHauler(EntityDto input)
        {
            if (await (await _leaseHaulerRepository.GetQueryAsync())
                    .Where(x => x.Id == input.Id)
                    .AnyAsync(x => x.LeaseHaulerTrucks.Any() || x.LeaseHaulerDrivers.Any()))
            {
                throw new UserFriendlyException("Cannot delete the Lease Hauler because it has one or more trucks or drivers.");
            }

            var lhContacts = await (await _leaseHaulerContactRepository.GetQueryAsync())
                .Where(lhc => lhc.LeaseHaulerId == input.Id)
                .ToListAsync();

            foreach (var lhContact in lhContacts)
            {
                await _leaseHaulerContactRepository.DeleteAsync(lhContact);
            }

            await _leaseHaulerRepository.DeleteAsync(input.Id);
        }

        [AbpAuthorize(AppPermissions.Pages_LeaseHaulers_Edit, AppPermissions.LeaseHaulerPortal_MyCompany_Contacts)]
        public async Task DeleteLeaseHaulerContact(EntityDto input)
        {
            var leaseHaulerContact = await _leaseHaulerContactRepository.FirstOrDefaultAsync(input.Id);
            await CheckEntitySpecificPermissions(
                AppPermissions.Pages_LeaseHaulers_Edit,
                AppPermissions.LeaseHaulerPortal_MyCompany_Contacts,
                Session.LeaseHaulerId,
                leaseHaulerContact.LeaseHaulerId);
            await _leaseHaulerContactRepository.DeleteAsync(leaseHaulerContact);
        }

        [AbpAuthorize(AppPermissions.Pages_LeaseHaulers_Edit, AppPermissions.LeaseHaulerPortal_MyCompany_Trucks)]
        public async Task DeleteLeaseHaulerTruck(EntityDto input)
        {
            bool hasDependencies = await (await _availableLeaseHaulerTruckRepository.GetQueryAsync())
                    .AnyAsync(x => x.TruckId == input.Id)
                || await (await _dispatchRepository.GetQueryAsync())
                    .AnyAsync(x => x.TruckId == input.Id)
                || await (await _orderLineTruckRepository.GetQueryAsync())
                    .AnyAsync(x => x.TruckId == input.Id);

            if (hasDependencies)
            {
                throw new UserFriendlyException(L("UnableToDeleteTruckWithAssociatedData"));
            }

            var leaseHaulerTruck = await _leaseHaulerTruckRepository.FirstOrDefaultAsync(q => q.TruckId == input.Id);
            await CheckEntitySpecificPermissions(
                AppPermissions.Pages_LeaseHaulers_Edit,
                AppPermissions.LeaseHaulerPortal_MyCompany_Trucks,
                Session.LeaseHaulerId,
                leaseHaulerTruck.LeaseHaulerId);
            await _truckAppService.DeleteTruck(input);
            await _leaseHaulerTruckRepository.DeleteAsync(leaseHaulerTruck);
        }

        [AbpAuthorize(AppPermissions.Pages_LeaseHaulers_Edit, AppPermissions.LeaseHaulerPortal_MyCompany_Drivers)]
        public async Task DeleteLeaseHaulerDriver(EntityDto input)
        {
            if (await (await _availableLeaseHaulerTruckRepository.GetQueryAsync())
                .AnyAsync(x => x.DriverId == input.Id))
            {
                throw new UserFriendlyException(L("UnableToDeleteDriverWithAssociatedData"));
            }

            var leaseHaulerDriver = await _leaseHaulerDriverRepository.FirstOrDefaultAsync(q => q.DriverId == input.Id);
            await CheckEntitySpecificPermissions(
                AppPermissions.Pages_LeaseHaulers_Edit,
                AppPermissions.LeaseHaulerPortal_MyCompany_Drivers,
                Session.LeaseHaulerId,
                leaseHaulerDriver.LeaseHaulerId);
            await _driverAppService.DeleteDriver(input);
            await _leaseHaulerDriverRepository.DeleteAsync(leaseHaulerDriver);
        }

        private async Task<User> GetUserForLhDriver(Driver driver)
        {
            return await GetUserForLhDriver(driver.EmailAddress, driver.UserId);
        }

        private async Task<User> GetUserForLhDriver(string driverEmail, long? driverUserId)
        {
            User user;
            if (driverEmail.IsNullOrEmpty())
            {
                return null;
            }

            if (driverUserId != null)
            {
                user = await UserManager.GetUserByIdAsync(driverUserId.Value);
                if (user == null) //|| user.EmailAddress.IsNullOrEmpty() || user.EmailAddress?.ToUpper() != driverEmail?.ToUpper())
                {
                    return null;
                }
                return user;
            }

            user = await UserManager.FindByEmailAsync(driverEmail);
            return user;
        }

        private async Task CreateOrUpdateUserForLHDriver(Driver driver, LeaseHaulerDriverEditDto model)
        {
            if (driver.EmailAddress.IsNullOrEmpty())
            {
                throw new UserFriendlyException("Driver Email Address is required");
            }

            var newUser = false;
            var user = await GetUserForLhDriver(driver);

            if (user == null)
            {
                newUser = true;
                user = await _userCreatorService.CreateUser(new CreateOrUpdateUserInput
                {
                    User = new UserEditDto
                    {
                        PhoneNumber = driver.CellPhoneNumber,
                        EmailAddress = driver.EmailAddress,
                        Name = driver.FirstName,
                        Surname = driver.LastName,
                        OfficeId = driver.OfficeId,
                        IsActive = model.DriverIsActive,
                        IsLockoutEnabled = true,
                        UserName = driver.EmailAddress.Substring(0, driver.EmailAddress.IndexOf("@")),
                        ShouldChangePasswordOnNextLogin = model.ShouldChangePasswordOnNextLogin,
                        Password = model.Password,
                    },
                    AssignedRoleNames = new[] { StaticRoleNames.Tenants.LeaseHaulerDriver },
                    SendActivationEmail = model.SendActivationEmail,
                    SetRandomPassword = model.SetRandomPassword,
                });
            }
            else
            {
                user.Name = model.FirstName;
                user.Surname = model.LastName;
                user.EmailAddress = model.EmailAddress;
                user.PhoneNumber = model.CellPhoneNumber;
                //user.IsActive = model.DriverIsActive;
                user.ShouldChangePasswordOnNextLogin = model.ShouldChangePasswordOnNextLogin;

                CheckErrors(await UserManager.UpdateAsync(user));

                if (model.SetRandomPassword)
                {
                    var randomPassword = await UserManager.CreateRandomPassword();
                    user.Password = _passwordHasher.HashPassword(user, randomPassword);
                    model.Password = randomPassword;
                }
                else if (!model.Password.IsNullOrEmpty())
                {
                    await UserManager.InitializeOptionsAsync(await AbpSession.GetTenantIdOrNullAsync());
                    CheckErrors(await UserManager.ChangePasswordAsync(user, model.Password));
                }

                var otherDrivers = await (await _driverRepository.GetQueryAsync())
                    .Include(x => x.LeaseHaulerDriver)
                    .Where(x => x.Id != driver.Id && x.UserId == user.Id)
                    .ToListAsync();

                if (model.DriverIsActive)
                {
                    user.IsActive = true;
                    foreach (var otherDriver in otherDrivers)
                    {
                        if (!otherDriver.IsInactive)
                        {
                            otherDriver.IsInactive = true;
                            await _driverInactivatorService.InactivateDriverAsync(otherDriver, otherDriver.LeaseHaulerDriver?.LeaseHaulerId);
                        }
                    }
                }
                else
                {
                    if (!otherDrivers.Any(x => !x.IsInactive))
                    {
                        user.IsActive = false;
                    }
                }

                CheckErrors(await UserManager.UpdateAsync(user));
            }

            if (!await UserManager.IsInRoleAsync(user, StaticRoleNames.Tenants.LeaseHaulerDriver))
            {
                await UserManager.AddToRoleAsync(user, StaticRoleNames.Tenants.LeaseHaulerDriver);
            }

            if (model.SendActivationEmail && !newUser)
            {
                user.SetNewEmailConfirmationCode();
                await UserManager.UpdateAsync(user);

                await _userEmailer.SendEmailActivationLinkAsync(
                    user,
                    await AppUrlService.CreateEmailActivationUrlFormatAsync(await AbpSession.GetTenantIdOrNullAsync()),
                    model.Password
                );
            }

            driver.UserId = user.Id;
        }

        private async Task InactivateUserForLHDriver(Driver driver)
        {
            if (driver.UserId == null)
            {
                return;
            }

            var user = await GetUserForLhDriver(driver);
            if (user != null)
            {
                var otherDrivers = await (await _driverRepository.GetQueryAsync())
                    .Where(x => x.Id != driver.Id && x.UserId == driver.UserId)
                    .ToListAsync();

                if (!otherDrivers.Any(x => !x.IsInactive)) //if there are no other drivers, or all other drivers are inactive too
                {
                    user.IsActive = false;
                    CheckErrors(await UserManager.UpdateAsync(user));
                    await UserManager.RemoveFromRoleAsync(user, StaticRoleNames.Tenants.LeaseHaulerDriver);
                }
            }

            await _driverUserLinkService.EnsureCanUnlinkAsync(driver);
            driver.UserId = null;
        }
    }
}
