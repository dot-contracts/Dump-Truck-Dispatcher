using System.Collections.Generic;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Threading.Tasks;
using Abp;
using Abp.Application.Features;
using Abp.Application.Services.Dto;
using Abp.Authorization;
using Abp.Domain.Repositories;
using Abp.Domain.Uow;
using Abp.Encryption;
using Abp.Events.Bus;
using Abp.Extensions;
using Abp.Linq.Extensions;
using DispatcherWeb.Authorization;
using DispatcherWeb.Dispatching;
using DispatcherWeb.Drivers;
using DispatcherWeb.Dto;
using DispatcherWeb.Editions.Dto;
using DispatcherWeb.Emailing;
using DispatcherWeb.Features;
using DispatcherWeb.Infrastructure.Extensions;
using DispatcherWeb.Invoices;
using DispatcherWeb.Items;
using DispatcherWeb.LeaseHaulerStatements;
using DispatcherWeb.MultiTenancy.Dto;
using DispatcherWeb.Offices;
using DispatcherWeb.Orders;
using DispatcherWeb.Payments;
using DispatcherWeb.PayStatements;
using DispatcherWeb.Sessions;
using DispatcherWeb.Sms;
using DispatcherWeb.SyncRequests;
using DispatcherWeb.SyncRequests.Entities;
using DispatcherWeb.TimeOffs;
using DispatcherWeb.Url;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DispatcherWeb.MultiTenancy
{
    [AbpAuthorize(AppPermissions.Pages_Tenants)]
    public class TenantAppService : DispatcherWebAppServiceBase, ITenantAppService
    {
        public IAppUrlService AppUrlService { get; set; }
        public IEventBus EventBus { get; set; }
        private readonly IRepository<Driver> _driverRepository;
        private readonly IRepository<EmployeeTimePayStatementTime> _employeeTimePayStatementTimeRepository;
        private readonly IRepository<Drivers.EmployeeTime> _employeeTimeRepository;
        private readonly IRepository<PayStatement> _payStatementRepository;
        private readonly IRepository<PayStatementDetail> _payStatementDetailRepository;
        private readonly IRepository<PayStatementDriverDateConflict> _payStatementDriverDateConflictRepository;
        private readonly IRepository<PayStatementTime> _payStatementTimeRepository;
        private readonly IRepository<PayStatementTicket> _payStatementTicketRepository;
        private readonly IRepository<LeaseHaulerStatementTicket> _leaseHaulerStatementTicketRepository;
        private readonly IRepository<LeaseHaulerStatement> _leaseHaulerStatementRepository;
        private readonly IRepository<Ticket> _ticketRepository;
        private readonly IRepository<Load> _loadRepository;
        private readonly IRepository<InvoiceUploadBatch> _invoiceUploadBatchRepository;
        private readonly IRepository<InvoiceEmail> _invoiceEmailRepository;
        private readonly IRepository<InvoiceLine> _invoiceLineRepository;
        private readonly IRepository<Invoice> _invoiceRepository;
        private readonly IRepository<InvoiceBatch> _invoiceBatchRepository;
        private readonly IRepository<Dispatch> _dispatchRepository;
        private readonly IRepository<OrderLineTruck> _orderLineTruckRepository;
        private readonly IRepository<Payment> _paymentsRepository;
        private readonly IRepository<OrderLine> _orderLineRepository;
        private readonly IRepository<OrderEmail> _orderEmailRepository;
        private readonly IRepository<OrderPayment> _orderPaymentRepository;
        private readonly IRepository<ReceiptLine> _receiptLineRepository;
        private readonly IRepository<Receipt> _receiptRepository;
        private readonly IRepository<Order> _orderRepository;
        private readonly IRepository<DriverMessage> _driverMessageRepository;
        private readonly IRepository<SentSms> _sentSmsRepository;
        private readonly IRepository<TimeOff> _timeOffRepository;
        private readonly IRepository<DriverAssignment> _driverAssignmentRepository;
        private readonly IRepository<BilledOrder> _billedOrderRepository;
        private readonly IEncryptionService _encryptionService;
        private readonly IRepository<PricingTier> _pricingTierRepository;
        private readonly ITenantSessionInfoCache _tenantSessionInfoCache;
        private readonly IOfficeOrganizationUnitSynchronizer _officeOrganizationUnitSynchronizer;
        private readonly ISyncRequestSender _syncRequestSender;
        private readonly IFeatureMigrator _featureMigrator;

        public TenantAppService(
            IRepository<Driver> driverRepository,
            IRepository<EmployeeTimePayStatementTime> employeeTimePayStatementTimeRepository,
            IRepository<Drivers.EmployeeTime> employeeTimeRepository,
            IRepository<PayStatement> payStatementRepository,
            IRepository<PayStatementDetail> payStatementDetailRepository,
            IRepository<PayStatementDriverDateConflict> payStatementDriverDateConflictRepository,
            IRepository<PayStatementTime> payStatementTimeRepository,
            IRepository<PayStatementTicket> payStatementTicketRepository,
            IRepository<LeaseHaulerStatementTicket> leaseHaulerStatementTicketRepository,
            IRepository<LeaseHaulerStatement> leaseHaulerStatementRepository,
            IRepository<Ticket> ticketRepository,
            IRepository<Load> loadRepository,
            IRepository<InvoiceUploadBatch> invoiceUploadBatchRepository,
            IRepository<InvoiceEmail> invoiceEmailRepository,
            IRepository<InvoiceLine> invoiceLineRepository,
            IRepository<Invoice> invoiceRepository,
            IRepository<InvoiceBatch> invoiceBatchRepository,
            IRepository<Dispatch> dispatchRepository,
            IRepository<OrderLineTruck> orderLineTruckRepository,
            IRepository<Payment> paymentsRepository,
            IRepository<OrderLine> orderLineRepository,
            IRepository<OrderEmail> orderEmailRepository,
            IRepository<OrderPayment> orderPaymentRepository,
            IRepository<ReceiptLine> receiptLineRepository,
            IRepository<Receipt> receiptRepository,
            IRepository<Order> orderRepository,
            IRepository<DriverMessage> driverMessageRepository,
            IRepository<SentSms> sentSmsRepository,
            IRepository<TimeOff> timeOffRepository,
            IRepository<DriverAssignment> driverAssignmentRepository,
            IRepository<BilledOrder> billedOrderRepository,
            IEncryptionService encryptionService,
            ITenantSessionInfoCache tenantSessionInfoCache,
            IOfficeOrganizationUnitSynchronizer officeOrganizationUnitSynchronizer,
            ISyncRequestSender syncRequestSender,
            IFeatureMigrator featureMigrator,
            IRepository<PricingTier> pricingTierRepository)
        {
            _driverRepository = driverRepository;
            _employeeTimePayStatementTimeRepository = employeeTimePayStatementTimeRepository;
            _employeeTimeRepository = employeeTimeRepository;
            _payStatementRepository = payStatementRepository;
            _payStatementDetailRepository = payStatementDetailRepository;
            _payStatementDriverDateConflictRepository = payStatementDriverDateConflictRepository;
            _payStatementTimeRepository = payStatementTimeRepository;
            _payStatementTicketRepository = payStatementTicketRepository;
            _leaseHaulerStatementTicketRepository = leaseHaulerStatementTicketRepository;
            _leaseHaulerStatementRepository = leaseHaulerStatementRepository;
            _ticketRepository = ticketRepository;
            _loadRepository = loadRepository;
            _invoiceUploadBatchRepository = invoiceUploadBatchRepository;
            _invoiceEmailRepository = invoiceEmailRepository;
            _invoiceLineRepository = invoiceLineRepository;
            _invoiceRepository = invoiceRepository;
            _invoiceBatchRepository = invoiceBatchRepository;
            _dispatchRepository = dispatchRepository;
            _orderLineTruckRepository = orderLineTruckRepository;
            _paymentsRepository = paymentsRepository;
            _orderLineRepository = orderLineRepository;
            _orderEmailRepository = orderEmailRepository;
            _orderPaymentRepository = orderPaymentRepository;
            _receiptLineRepository = receiptLineRepository;
            _receiptRepository = receiptRepository;
            _orderRepository = orderRepository;
            _driverMessageRepository = driverMessageRepository;
            _sentSmsRepository = sentSmsRepository;
            _timeOffRepository = timeOffRepository;
            _driverAssignmentRepository = driverAssignmentRepository;
            _billedOrderRepository = billedOrderRepository;
            _encryptionService = encryptionService;
            _tenantSessionInfoCache = tenantSessionInfoCache;
            _officeOrganizationUnitSynchronizer = officeOrganizationUnitSynchronizer;
            _syncRequestSender = syncRequestSender;
            _featureMigrator = featureMigrator;
            _pricingTierRepository = pricingTierRepository;
            AppUrlService = NullAppUrlService.Instance;
            EventBus = NullEventBus.Instance;
        }

        public async Task<PagedResultDto<TenantListDto>> GetTenants(GetTenantsInput input)
        {
            var query = (await TenantManager.GetQueryAsync())
                .Include(t => t.Edition)
                .WhereIf(!input.Filter.IsNullOrWhiteSpace(), t => t.Name.Contains(input.Filter) || t.TenancyName.Contains(input.Filter))
                .WhereIf(input.CreationDateStart.HasValue, t => t.CreationTime >= input.CreationDateStart.Value)
                .WhereIf(input.CreationDateEnd.HasValue, t => t.CreationTime <= input.CreationDateEnd.Value)
                .WhereIf(input.SubscriptionEndDateStart.HasValue, t => t.SubscriptionEndDateUtc >= input.SubscriptionEndDateStart.Value.ToUniversalTime())
                .WhereIf(input.SubscriptionEndDateEnd.HasValue, t => t.SubscriptionEndDateUtc <= input.SubscriptionEndDateEnd.Value.ToUniversalTime())
                .WhereIf(input.EditionIdSpecified, t => t.EditionId == input.EditionId)
                .WhereIf(input.Status == FilterActiveStatus.Active, x => x.IsActive)
                .WhereIf(input.Status == FilterActiveStatus.Inactive, x => !x.IsActive);

            var tenantCount = await query.CountAsync();

            var tenants = await query
                .Select(x => new TenantListDto
                {
                    Id = x.Id,
                    TenancyName = x.TenancyName,
                    Name = x.Name,
                    EditionId = x.EditionId,
                    EditionDisplayName = x.Edition.DisplayName,
                    IsActive = x.IsActive,
                    SubscriptionEndDateUtc = x.SubscriptionEndDateUtc,
                    ConnectionString = x.ConnectionString,
                    CreationTime = x.CreationTime,
                    IsInTrialPeriod = x.IsInTrialPeriod,
                })
                .OrderBy(input.Sorting)
                .PageBy(input)
                .ToListAsync();


            return new PagedResultDto<TenantListDto>(tenantCount, tenants);
        }

        [HttpPost]
        public async Task<PagedResultDto<SelectListDto>> GetTenantsSelectList(GetTenantsSelectListInput input)
        {
            return await (await TenantManager.GetQueryAsync())
                .WhereIf(input.EditionIds?.Any() == true, x => x.EditionId.HasValue && input.EditionIds.Contains(x.EditionId.Value))
                .WhereIf(input.ActiveFilter.HasValue, x => x.IsActive == input.ActiveFilter)
                .Select(x => new SelectListDto
                {
                    Id = x.Id.ToString(),
                    Name = x.Name,
                })
                .GetSelectListResult(input);
        }

        [AbpAuthorize(AppPermissions.Pages_Tenants_Create)]
        public async Task CreateTenant(CreateTenantInput input)
        {
            var tenantId = await TenantManager.CreateWithAdminUserAsync(
                input.CompanyName,
                input.AdminFirstName,
                input.AdminLastName,
                input.AdminPassword,
                input.AdminEmailAddress,
                input.ConnectionString,
                input.IsActive,
                input.EditionId,
                input.ShouldChangePasswordOnNextLogin,
                input.SendActivationEmail,
                input.SubscriptionEndDateUtc?.ToUniversalTime(),
                input.IsInTrialPeriod,
                AppUrlService.CreateEmailActivationUrlFormat
            );
            await _officeOrganizationUnitSynchronizer.MigrateOfficesForTenant(tenantId);
            await _pricingTierRepository.InsertAsync(new PricingTier
            {
                Name = "Retail",
                IsDefault = true,
                TenantId = tenantId,
            });
        }

        [AbpAuthorize(AppPermissions.Pages_Tenants_Edit)]
        public async Task<TenantEditDto> GetTenantForEdit(EntityDto input)
        {
            var tenant = await (await TenantManager.GetQueryAsync()).Where(t => t.Id == input.Id)
                .Select(x => new TenantEditDto
                {
                    Id = x.Id,
                    TenancyName = x.TenancyName,
                    Name = x.Name,
                    EditionId = x.EditionId,
                    IsActive = x.IsActive,
                    SubscriptionEndDateUtc = x.SubscriptionEndDateUtc,
                }).FirstOrDefaultAsync();

            tenant.ConnectionString = _encryptionService.DecryptIfNotEmpty(tenant.ConnectionString);
            return tenant;
        }

        [AbpAuthorize(AppPermissions.Pages_Tenants_Edit)]
        public async Task UpdateTenant(TenantEditDto input)
        {
            await TenantManager.CheckEditionAsync(input.EditionId, input.IsInTrialPeriod);

            input.ConnectionString = _encryptionService.EncryptIfNotEmpty(input.ConnectionString);
            var tenant = await TenantManager.GetByIdAsync(input.Id);
            if (tenant.EditionId != input.EditionId)
            {
                await EventBus.TriggerAsync(new TenantEditionChangedEventData
                {
                    TenantId = input.Id,
                    OldEditionId = tenant.EditionId,
                    NewEditionId = input.EditionId,
                });
            }

            tenant.TenancyName = input.TenancyName;
            tenant.Name = input.Name;
            tenant.IsActive = input.IsActive;
            tenant.ConnectionString = input.ConnectionString;
            tenant.EditionId = input.EditionId;
            tenant.IsInTrialPeriod = input.IsInTrialPeriod;
            tenant.SubscriptionEndDateUtc = tenant.SubscriptionEndDateUtc?.ToUniversalTime();

            await TenantManager.UpdateAsync(tenant);
        }

        [AbpAuthorize(AppPermissions.Pages_Tenants_Delete)]
        public async Task DeleteTenant(EntityDto input)
        {
            var tenant = await TenantManager.GetByIdAsync(input.Id);
            await TenantManager.DeleteAsync(tenant);
        }

        [AbpAuthorize(AppPermissions.Pages_Tenants_DeleteDispatchData)]
        public async Task DeleteDispatchDataForTenant(DeleteDispatchDataForTenantInput input)
        {
            var batchSize = input.BatchSize ?? 100;
            using (CurrentUnitOfWork.SetTenantId(input.Id))
            {
                using (CurrentUnitOfWork.DisableFilter(AbpDataFilters.SoftDelete))
                {
                    await _employeeTimePayStatementTimeRepository.DeleteInBatchesAsync(x => true, CurrentUnitOfWork, batchSize);
                    await _employeeTimeRepository.HardDeleteInBatchesAsync(x => true, CurrentUnitOfWork, batchSize);
                    await _payStatementTimeRepository.DeleteInBatchesAsync(x => true, CurrentUnitOfWork, batchSize);
                    await _payStatementTicketRepository.DeleteInBatchesAsync(x => true, CurrentUnitOfWork, batchSize);
                    await _payStatementDetailRepository.DeleteInBatchesAsync(x => true, CurrentUnitOfWork, batchSize);
                    await _payStatementDriverDateConflictRepository.DeleteInBatchesAsync(x => true, CurrentUnitOfWork, batchSize);
                    await _payStatementRepository.HardDeleteInBatchesAsync(x => true, CurrentUnitOfWork, batchSize);
                    await _leaseHaulerStatementTicketRepository.DeleteInBatchesAsync(x => true, CurrentUnitOfWork, batchSize);
                    await _leaseHaulerStatementRepository.HardDeleteInBatchesAsync(x => true, CurrentUnitOfWork, batchSize);
                    await _invoiceEmailRepository.DeleteInBatchesAsync(x => x.Invoice.TenantId == input.Id, CurrentUnitOfWork, batchSize);
                    await _invoiceLineRepository.HardDeleteInBatchesAsync(x => x.ParentInvoiceLineId.HasValue, CurrentUnitOfWork, batchSize);
                    await _invoiceLineRepository.HardDeleteInBatchesAsync(x => true, CurrentUnitOfWork, batchSize);
                    await _invoiceRepository.HardDeleteInBatchesAsync(x => true, CurrentUnitOfWork, batchSize);
                    await _invoiceUploadBatchRepository.HardDeleteInBatchesAsync(x => true, CurrentUnitOfWork, batchSize);
                    await _invoiceBatchRepository.HardDeleteInBatchesAsync(x => true, CurrentUnitOfWork, batchSize);

                    var ticketsWithLoad = await (await _ticketRepository.GetQueryAsync())
                        .Where(t => t.LoadId.HasValue)
                        .ToListAsync();
                    foreach (var ticket in ticketsWithLoad)
                    {
                        ticket.LoadId = null;
                    }
                    await CurrentUnitOfWork.SaveChangesAsync();

                    await _ticketRepository.HardDeleteInBatchesAsync(x => true, CurrentUnitOfWork, batchSize);
                    await _loadRepository.HardDeleteInBatchesAsync(x => true, CurrentUnitOfWork, batchSize);
                    await _dispatchRepository.HardDeleteInBatchesAsync(x => true, CurrentUnitOfWork, batchSize);
                    await _orderLineTruckRepository.HardDeleteInBatchesAsync(x => x.ParentOrderLineTruckId.HasValue, CurrentUnitOfWork, batchSize);
                    await _orderLineTruckRepository.HardDeleteInBatchesAsync(x => true, CurrentUnitOfWork, batchSize);
                    await _orderPaymentRepository.HardDeleteInBatchesAsync(x => true, CurrentUnitOfWork, batchSize);
                    await _paymentsRepository.HardDeleteInBatchesAsync(x => true, CurrentUnitOfWork, batchSize);
                    await _receiptLineRepository.HardDeleteInBatchesAsync(x => true, CurrentUnitOfWork, batchSize);
                    await _receiptRepository.HardDeleteInBatchesAsync(x => true, CurrentUnitOfWork, batchSize);
                    await _billedOrderRepository.HardDeleteInBatchesAsync(x => true, CurrentUnitOfWork, batchSize);
                    await _orderLineRepository.HardDeleteInBatchesAsync(x => true, CurrentUnitOfWork, batchSize);
                    await _orderEmailRepository.DeleteInBatchesAsync(x => x.Order.TenantId == input.Id, CurrentUnitOfWork, batchSize);
                    await _orderRepository.HardDeleteInBatchesAsync(x => true, CurrentUnitOfWork, batchSize);
                    await _driverMessageRepository.HardDeleteInBatchesAsync(x => true, CurrentUnitOfWork, batchSize);
                    await _sentSmsRepository.DeleteInBatchesAsync(x => true, CurrentUnitOfWork, batchSize);
                    await _timeOffRepository.HardDeleteInBatchesAsync(x => true, CurrentUnitOfWork, batchSize);
                    await _driverAssignmentRepository.HardDeleteInBatchesAsync(x => true, CurrentUnitOfWork, batchSize);
                }
            }
        }

        [AbpAuthorize(AppPermissions.Pages_Tenants_ChangeFeatures)]
        public async Task<GetTenantFeaturesEditOutput> GetTenantFeaturesForEdit(EntityDto input)
        {
            var features = FeatureManager.GetAll()
                .Where(f => f.Scope.HasFlag(FeatureScopes.Tenant))
                .Select(x => new FlatFeatureDto
                {
                    ParentName = x.Parent?.Name,
                    Name = x.Name,
                    DisplayName = L(x.DisplayName),
                    Description = L(x.Description),
                    DefaultValue = x.DefaultValue,
                    InputType = new FeatureInputTypeDto
                    {
                        Name = x.InputType.Name,
                        Attributes = x.InputType.Attributes,
                        Validator = x.InputType.Validator,
                    },
                })
                .OrderBy(f => f.DisplayName)
                .ToList();

            var featureValues = await TenantManager.GetFeatureValuesAsync(input.Id);

            return new GetTenantFeaturesEditOutput
            {
                Features = features,
                FeatureValues = featureValues.Select(fv => new NameValueDto(fv)).ToList(),
            };
        }

        [AbpAuthorize(AppPermissions.Pages_Tenants_ChangeFeatures)]
        public async Task UpdateTenantFeatures(UpdateTenantFeaturesInput input)
        {
            var oldValues = await TenantManager.GetFeatureValuesAsync(input.Id);
            var newValues = input.FeatureValues.Select(fv => new NameValue(fv.Name, fv.Value)).ToArray();

            await TenantManager.SetFeatureValuesAsync(input.Id, newValues);
            await ClearTenantCache(input.Id);
            await SendSyncRequestsIfNeeded(input.Id, oldValues);
            await _featureMigrator.MigrateTenantIfNeeded(input.Id, oldValues);
        }

        [AbpAuthorize(AppPermissions.Pages_Tenants_ChangeFeatures)]
        public async Task ResetTenantSpecificFeatures(EntityDto input)
        {
            var oldValues = await TenantManager.GetFeatureValuesAsync(input.Id);

            await TenantManager.ResetAllFeaturesAsync(input.Id);
            await ClearTenantCache(input.Id);
            await SendSyncRequestsIfNeeded(input.Id, oldValues);
            await _featureMigrator.MigrateTenantIfNeeded(input.Id, oldValues);
        }

        private async Task ClearTenantCache(int tenantId)
        {
            await _tenantSessionInfoCache.InvalidateCache(tenantId);
        }

        private async Task SendSyncRequestsIfNeeded(int tenantId, IReadOnlyList<NameValue> oldValues)
        {
            await CurrentUnitOfWork.SaveChangesAsync();
            var newValues = await TenantManager.GetFeatureValuesAsync(tenantId);
            if (oldValues.FirstOrDefault(x => x.Name == AppFeatures.ReactNativeDriverApp)?.Value
                != newValues.FirstOrDefault(x => x.Name == AppFeatures.ReactNativeDriverApp)?.Value
                || oldValues.FirstOrDefault(x => x.Name == AppFeatures.ChatFeature)?.Value
                != newValues.FirstOrDefault(x => x.Name == AppFeatures.ChatFeature)?.Value)
            {
                using (Session.Use(tenantId, Session.UserId))
                {
                    await _syncRequestSender.SendSyncRequest(new SyncRequest()
                        .AddChange(EntityEnum.Settings, new ChangedSettings()));
                }
            }
        }

        public async Task UnlockTenantAdmin(EntityDto input)
        {
            using (CurrentUnitOfWork.SetTenantId(input.Id))
            {
                var tenantAdmin = await UserManager.GetAdminAsync();
                if (tenantAdmin != null)
                {
                    tenantAdmin.Unlock();
                    await UserManager.UpdateAsync(tenantAdmin);
                }
            }
        }

        public async Task AddMonthToDriverDOTRequirements(EntityDto input)
        {
            var drivers = await (await _driverRepository.GetQueryAsync())
                .Where(x => !x.IsExternal && x.TenantId == input.Id)
                .ToListAsync();

            foreach (var item in drivers)
            {
                item.LicenseExpirationDate = item.LicenseExpirationDate?.AddMonths(1);
                item.LastPhysicalDate = item.LastPhysicalDate?.AddMonths(1);
                item.NextPhysicalDueDate = item.NextPhysicalDueDate?.AddMonths(1);
                item.LastMvrDate = item.LastMvrDate?.AddMonths(1);
                item.NextMvrDueDate = item.NextMvrDueDate?.AddMonths(1);

            }
        }
    }
}
