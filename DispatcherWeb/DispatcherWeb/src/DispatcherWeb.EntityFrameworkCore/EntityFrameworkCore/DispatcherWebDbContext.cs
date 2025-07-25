using Abp.IdentityServer4vNext;
using Abp.Zero.EntityFrameworkCore;
using DispatcherWeb.ActiveReports;
using DispatcherWeb.Authorization.Delegation;
using DispatcherWeb.Authorization.Roles;
using DispatcherWeb.Authorization.Users;
using DispatcherWeb.BackgroundJobs;
using DispatcherWeb.CannedTexts;
using DispatcherWeb.Charges;
using DispatcherWeb.Chat;
using DispatcherWeb.CustomerNotifications;
using DispatcherWeb.Customers;
using DispatcherWeb.DailyFuelCosts;
using DispatcherWeb.DailyHistory;
using DispatcherWeb.Dispatching;
using DispatcherWeb.Drivers;
using DispatcherWeb.Editions;
using DispatcherWeb.Emailing;
using DispatcherWeb.EntityFrameworkCore.Configurations;
using DispatcherWeb.Friendships;
using DispatcherWeb.FuelSurchargeCalculations;
using DispatcherWeb.HaulingCategories;
using DispatcherWeb.HaulZones;
using DispatcherWeb.HostEmails;
using DispatcherWeb.Insurances;
using DispatcherWeb.Invoices;
using DispatcherWeb.Items;
using DispatcherWeb.LeaseHaulerRequests;
using DispatcherWeb.LeaseHaulers;
using DispatcherWeb.LeaseHaulerStatements;
using DispatcherWeb.Locations;
using DispatcherWeb.LuckStone;
using DispatcherWeb.MultiTenancy;
using DispatcherWeb.MultiTenancy.Payments;
using DispatcherWeb.Offices;
using DispatcherWeb.Orders;
using DispatcherWeb.Payments;
using DispatcherWeb.PayStatements;
using DispatcherWeb.Quotes;
using DispatcherWeb.ScheduledReports;
using DispatcherWeb.SecureFiles;
using DispatcherWeb.Sms;
using DispatcherWeb.Storage;
using DispatcherWeb.TaxRates;
using DispatcherWeb.TimeClassifications;
using DispatcherWeb.TimeOffs;
using DispatcherWeb.Trucks;
using DispatcherWeb.Trux;
using DispatcherWeb.UnitsOfMeasure;
using DispatcherWeb.VehicleCategories;
using DispatcherWeb.VehicleMaintenance;
using DispatcherWeb.WebPush;
using Microsoft.EntityFrameworkCore;
using Accounting = DispatcherWeb.MultiTenancy.Accounting;

namespace DispatcherWeb.EntityFrameworkCore
{
    public class DispatcherWebDbContext : AbpZeroDbContext<Tenant, Role, User, DispatcherWebDbContext>, IAbpPersistedGrantDbContext
    {
        /* Define an DbSet for each entity of the application */

        public virtual DbSet<BinaryObject> BinaryObjects { get; set; }

        public virtual DbSet<DeferredBinaryObject> DeferredBinaryObjects { get; set; }

        public virtual DbSet<Friendship> Friendships { get; set; }

        public virtual DbSet<ChatMessage> ChatMessages { get; set; }

        public virtual DbSet<SubscribableEdition> SubscribableEditions { get; set; }

        public virtual DbSet<SubscriptionPayment> SubscriptionPayments { get; set; }

        public virtual DbSet<Accounting.Invoice> AccountingInvoices { get; set; }

        public virtual DbSet<PersistedGrantEntity> PersistedGrants { get; set; }

        public virtual DbSet<AvailableLeaseHaulerTruck> AvailableLeaseHaulerTruck { get; set; }

        public virtual DbSet<SubscriptionPaymentExtensionData> SubscriptionPaymentExtensionDatas { get; set; }

        public virtual DbSet<BackgroundJobHistory> BackgroundJobHistory { get; set; }

        public virtual DbSet<BilledOrder> BilledOrders { get; set; }

        public virtual DbSet<CannedText> CannedTexts { get; set; }

        public virtual DbSet<Customer> Customers { get; set; }

        public virtual DbSet<CustomerContact> CustomerContacts { get; set; }

        public virtual DbSet<TrackableEmail> TrackableEmails { get; set; }

        public virtual DbSet<TrackableEmailEvent> TrackableEmailEvents { get; set; }

        public virtual DbSet<TrackableEmailReceiver> TrackableEmailReceivers { get; set; }

        public virtual DbSet<LeaseHauler> LeaseHaulers { get; set; }

        public virtual DbSet<LeaseHaulerContact> LeaseHaulerContacts { get; set; }

        public virtual DbSet<LeaseHaulerDriver> LeaseHaulerDrivers { get; set; }

        public virtual DbSet<LeaseHaulerUser> LeaseHaulerUsers { get; set; }

        public virtual DbSet<LeaseHaulerTruck> LeaseHaulerTrucks { get; set; }

        public virtual DbSet<LeaseHaulerRequest> LeaseHaulerRequests { get; set; }

        public virtual DbSet<LeaseHaulerStatement> LeaseHaulerStatements { get; set; }

        public virtual DbSet<LeaseHaulerStatementTicket> LeaseHaulerStatementTickets { get; set; }

        public virtual DbSet<MobileDevice> MobileDevices { get; set; }

        public virtual DbSet<Office> Offices { get; set; }

        public virtual DbSet<OrderLine> OrderLines { get; set; }

        public virtual DbSet<OrderPayment> OrderPayments { get; set; }

        public virtual DbSet<OrderLineTruck> OrderLineTrucks { get; set; }

        public virtual DbSet<Payment> Payments { get; set; }

        public virtual DbSet<RequestedLeaseHaulerTruck> RequestedLeaseHaulerTrucks { get; set; }

        public virtual DbSet<SentSms> SentSmses { get; set; }

        public virtual DbSet<Item> Items { get; set; }

        public virtual DbSet<ProductLocation> ProductLocations { get; set; }

        public virtual DbSet<ProductLocationPrice> ProductLocationPrices { get; set; }

        public virtual DbSet<PricingTier> PricingTiers { get; set; }

        public virtual DbSet<HaulZone> HaulZones { get; set; }

        public virtual DbSet<HaulingCategory> HaulingCategories { get; set; }

        public virtual DbSet<HaulingCategoryPrice> HaulingCategoryPrices { get; set; }

        public virtual DbSet<Location> Locations { get; set; }

        public virtual DbSet<LocationCategory> LocationCategories { get; set; }

        public virtual DbSet<LocationContact> LocationContacts { get; set; }

        public virtual DbSet<DailyFuelCost> DailyFuelCosts { get; set; }

        public virtual DbSet<Driver> Drivers { get; set; }

        public virtual DbSet<DriverAssignment> DriverAssignments { get; set; }

        public virtual DbSet<DriverMessage> DriverMessages { get; set; }

        public virtual DbSet<DriverPushSubscription> DriverPushSubscriptions { get; set; }

        public virtual DbSet<DriverApplicationLog> DriverApplicationLogs { get; set; }
        public virtual DbSet<DriverApplicationDevice> DriverApplicationDevices { get; set; }

        public virtual DbSet<Invoice> Invoices { get; set; }

        public virtual DbSet<InvoiceBatch> InvoiceBatches { get; set; }

        public virtual DbSet<InvoiceUploadBatch> InvoiceUploadBatches { get; set; }

        public virtual DbSet<InvoiceLine> InvoiceLines { get; set; }

        public virtual DbSet<PushSubscription> PushSubscriptions { get; set; }

        public virtual DbSet<FcmRegistrationToken> FcmRegistrationTokens { get; set; }

        public virtual DbSet<FcmPushMessage> FcmPushMessages { get; set; }

        public virtual DbSet<OfficeItemPrice> OfficeItemPrices { get; set; }

        public virtual DbSet<Order> Orders { get; set; }

        public virtual DbSet<PayStatement> PayStatements { get; set; }
        public virtual DbSet<PayStatementDetail> PayStatementDetails { get; set; }
        public virtual DbSet<PayStatementTicket> PayStatementTickets { get; set; }
        public virtual DbSet<PayStatementTime> PayStatementTimeRecords { get; set; }
        public virtual DbSet<PayStatementDriverDateConflict> PayStatementDriverDateConflicts { get; set; }

        public virtual DbSet<Receipt> Receipts { get; set; }

        public virtual DbSet<ReceiptLine> ReceiptLines { get; set; }

        public virtual DbSet<Truck> Trucks { get; set; }

        public virtual DbSet<TruckFile> TruckFiles { get; set; }

        public virtual DbSet<TruxEarnings> TruxEarnings { get; set; }

        public virtual DbSet<TruxEarningsBatch> TruxEarningsBatches { get; set; }

        public virtual DbSet<ImportedEarnings> ImportedEarnings { get; set; }

        public virtual DbSet<ImportedEarningsBatch> ImportedEarningsBatches { get; set; }

        public virtual DbSet<LuckStoneLocation> LuckStoneLocations { get; set; }

        public virtual DbSet<OneTimeLogin> OneTimeLogins { get; set; }

        public virtual DbSet<Quote> Quotes { get; set; }

        public virtual DbSet<QuoteLine> QuoteLines { get; set; }

        public virtual DbSet<QuoteHistoryRecord> QuoteHistory { get; set; }

        public virtual DbSet<QuoteFieldDiff> QuoteFieldDiffs { get; set; }

        public virtual DbSet<QuoteEmail> QuoteEmails { get; set; }

        public virtual DbSet<OrderEmail> OrderEmails { get; set; }

        public virtual DbSet<InvoiceEmail> InvoiceEmails { get; set; }

        public virtual DbSet<UnitOfMeasure> UnitsOfMeasure { get; set; }

        public virtual DbSet<UnitOfMeasureBase> UnitOfMeasureBases { get; set; }

        public virtual DbSet<Ticket> Tickets { get; set; }

        public virtual DbSet<Charge> Charges { get; set; }

        public virtual DbSet<TimeOff> TimeOffs { get; set; }

        public virtual DbSet<VehicleCategory> VehicleCategories { get; set; }
        public virtual DbSet<VehicleServiceType> VehicleServiceTypes { get; set; }
        public virtual DbSet<VehicleService> VehicleServices { get; set; }
        public virtual DbSet<VehicleServiceDocument> VehicleServiceDocuments { get; set; }
        public virtual DbSet<WialonDeviceType> WialonDeviceTypes { get; set; }
        public virtual DbSet<WorkOrder> WorkOrders { get; set; }
        public virtual DbSet<WorkOrderLine> WorkOrderLines { get; set; }
        public virtual DbSet<WorkOrderPicture> WorkOrderPictures { get; set; }
        public virtual DbSet<PreventiveMaintenance> PreventiveMaintenance { get; set; }
        public virtual DbSet<OutOfServiceHistory> OutOfServiceHistory { get; set; }
        public virtual DbSet<ScheduledReport> ScheduledReports { get; set; }
        //public virtual DbSet<VehicleMileageHistory> VehicleMileageHistory { get; set; }

        public virtual DbSet<Dispatch> Dispatches { get; set; }
        public virtual DbSet<Load> Loads { get; set; }
        public virtual DbSet<SecureFileDefinition> SecureFileDefinitions { get; set; }
        public virtual DbSet<FuelPurchase> FuelPurchases { get; set; }
        public virtual DbSet<VehicleUsage> VehicleUsages { get; set; }

        public virtual DbSet<TenantDailyHistory> TenantDailyHistory { get; set; }
        public virtual DbSet<UserDailyHistory> UserDailyHistory { get; set; }
        public virtual DbSet<TransactionDailyHistory> TransactionDailyHistory { get; set; }

        public virtual DbSet<PaymentHeartlandKey> PaymentHeartlandKeys { get; set; }

        public virtual DbSet<EmployeeTime> EmployeeTime { get; set; }

        public virtual DbSet<EmployeeTimePayStatementTime> EmployeeTimePayStatementTimeRecords { get; set; }

        public virtual DbSet<TaxRate> TaxRates { get; set; }

        public virtual DbSet<TimeClassification> TimeClassifications { get; set; }

        public virtual DbSet<EmployeeTimeClassification> EmployeeTimeClassifications { get; set; }
        public virtual DbSet<UserDelegation> UserDelegations { get; set; }

        public virtual DbSet<FuelSurchargeCalculation> FuelSurchargeCalculations { get; set; }

        public virtual DbSet<HostEmail> HostEmails { get; set; }

        public virtual DbSet<HostEmailEdition> HostEmailEditions { get; set; }

        public virtual DbSet<HostEmailTenant> HostEmailTenants { get; set; }

        public virtual DbSet<HostEmailRole> HostEmailRoles { get; set; }

        public virtual DbSet<HostEmailReceiver> HostEmailReceivers { get; set; }

        public virtual DbSet<CustomerNotification> CustomerNotifications { get; set; }

        public virtual DbSet<CustomerNotificationEdition> CustomerNotificationEditions { get; set; }

        public virtual DbSet<CustomerNotificationTenant> CustomerNotificationTenants { get; set; }

        public virtual DbSet<CustomerNotificationRole> CustomerNotificationRoles { get; set; }

        public virtual DbSet<DismissedCustomerNotification> DismissedCustomerNotifications { get; set; }

        public virtual DbSet<OrderLineVehicleCategory> OrderLineVehicleCategories { get; set; }

        public virtual DbSet<QuoteLineVehicleCategory> QuoteLineVehicleCategories { get; set; }

        public virtual DbSet<ActiveReport> ActiveReports { get; set; }

        public virtual DbSet<ActiveReportCategory> ActiveReportCategories { get; set; }

        public virtual DbSet<Insurance> Insurances { get; set; }

        public virtual DbSet<InsuranceType> InsuranceTypes { get; set; }

        public virtual DbSet<TempFile> TempFiles { get; set; }

        public DispatcherWebDbContext(DbContextOptions<DispatcherWebDbContext> options)
            : base(options)
        {

        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<BinaryObject>(b =>
            {
                b.HasIndex(e => new { e.TenantId });
            });

            modelBuilder.Entity<ChatMessage>(b =>
            {
                b.HasIndex(e => new { e.TenantId, e.UserId, e.ReadState });
                b.HasIndex(e => new { e.TenantId, e.TargetUserId, e.ReadState });
                b.HasIndex(e => new { e.TargetTenantId, e.TargetUserId, e.ReadState });
                b.HasIndex(e => new { e.TargetTenantId, e.UserId, e.ReadState });
            });

            modelBuilder.Entity<Friendship>(b =>
            {
                b.HasIndex(e => new { e.TenantId, e.UserId });
                b.HasIndex(e => new { e.TenantId, e.FriendUserId });
                b.HasIndex(e => new { e.FriendTenantId, e.UserId });
                b.HasIndex(e => new { e.FriendTenantId, e.FriendUserId });
            });

            modelBuilder.Entity<Tenant>(b =>
            {
                b.HasIndex(e => new { e.SubscriptionEndDateUtc });
                b.HasIndex(e => new { e.CreationTime });
            });

            modelBuilder.Entity<SubscriptionPayment>(b =>
            {
                b.HasIndex(e => new { e.Status, e.CreationTime });
                b.HasIndex(e => new { PaymentId = e.ExternalPaymentId, e.Gateway });
            });

            modelBuilder.Entity<SubscriptionPaymentExtensionData>(b =>
            {
                b.HasQueryFilter(m => !m.IsDeleted)
                    .HasIndex(e => new { e.SubscriptionPaymentId, e.Key, e.IsDeleted })
                    .IsUnique();
            });

            modelBuilder.Entity<UserDelegation>(b =>
            {
                b.HasIndex(e => new { e.TenantId, e.SourceUserId });
                b.HasIndex(e => new { e.TenantId, e.TargetUserId });
            });

            modelBuilder.ApplyConfiguration(new ActiveReportConfiguration());
            modelBuilder.ApplyConfiguration(new ActiveReportCategoryConfiguration());
            modelBuilder.ApplyConfiguration(new AuditLogConfiguration());
            modelBuilder.ApplyConfiguration(new AvailableLeaseHaulerTruckConfiguration());
            modelBuilder.ApplyConfiguration(new BilledOrderConfiguration());
            modelBuilder.ApplyConfiguration(new CannedTextConfiguration());
            modelBuilder.ApplyConfiguration(new ChargeConfiguration());
            modelBuilder.ApplyConfiguration(new ChatMessageConfiguration());
            modelBuilder.ApplyConfiguration(new CustomerConfiguration());
            modelBuilder.ApplyConfiguration(new CustomerContactConfiguration());
            modelBuilder.ApplyConfiguration(new CustomerNotificationConfiguration());
            modelBuilder.ApplyConfiguration(new CustomerNotificationEditionConfiguration());
            modelBuilder.ApplyConfiguration(new CustomerNotificationTenantConfiguration());
            modelBuilder.ApplyConfiguration(new DailyFuelCostConfiguration());
            modelBuilder.ApplyConfiguration(new DismissedCustomerNotificationConfiguration());
            modelBuilder.ApplyConfiguration(new DispatchConfiguration());
            modelBuilder.ApplyConfiguration(new DriverAssignmentConfiguration());
            modelBuilder.ApplyConfiguration(new DriverConfiguration());
            modelBuilder.ApplyConfiguration(new DriverPushSubscriptionConfiguration());
            modelBuilder.ApplyConfiguration(new EmployeeTimeClassificationConfiguration());
            modelBuilder.ApplyConfiguration(new EmployeeTimeConfiguration());
            modelBuilder.ApplyConfiguration(new EmployeeTimePayStatementTimeConfiguration());
            modelBuilder.ApplyConfiguration(new FcmPushMessageConfiguration());
            modelBuilder.ApplyConfiguration(new FuelPurchaseConfiguration());
            modelBuilder.ApplyConfiguration(new FuelSurchargeCalculationConfiguration());
            modelBuilder.ApplyConfiguration(new HaulingCategoryConfiguration());
            modelBuilder.ApplyConfiguration(new HaulingCategoryPriceConfiguration());
            modelBuilder.ApplyConfiguration(new HaulZoneConfiguration());
            modelBuilder.ApplyConfiguration(new HostEmailConfiguration());
            modelBuilder.ApplyConfiguration(new HostEmailEditionConfiguration());
            modelBuilder.ApplyConfiguration(new HostEmailReceiverConfiguration());
            modelBuilder.ApplyConfiguration(new HostEmailTenantConfiguration());
            modelBuilder.ApplyConfiguration(new ImportedEarningsConfiguration());
            modelBuilder.ApplyConfiguration(new InsuranceConfiguration());
            modelBuilder.ApplyConfiguration(new InvoiceConfiguration());
            modelBuilder.ApplyConfiguration(new InvoiceBatchConfiguration());
            modelBuilder.ApplyConfiguration(new InvoiceEmailConfiguration());
            modelBuilder.ApplyConfiguration(new InvoiceLineConfiguration());
            modelBuilder.ApplyConfiguration(new InvoiceUploadBatchConfiguration());
            modelBuilder.ApplyConfiguration(new ItemConfiguration());
            modelBuilder.ApplyConfiguration(new LeaseHaulerConfiguration());
            modelBuilder.ApplyConfiguration(new LeaseHaulerDriverConfiguration());
            modelBuilder.ApplyConfiguration(new LeaseHaulerStatementConfiguration());
            modelBuilder.ApplyConfiguration(new LeaseHaulerStatementTicketConfiguration());
            modelBuilder.ApplyConfiguration(new LeaseHaulerTruckConfiguration());
            modelBuilder.ApplyConfiguration(new LeaseHaulerUserConfiguration());
            modelBuilder.ApplyConfiguration(new LoadConfiguration());
            modelBuilder.ApplyConfiguration(new LocationConfiguration());
            modelBuilder.ApplyConfiguration(new LuckStoneLocationConfiguration());
            modelBuilder.ApplyConfiguration(new MobileDeviceConfiguration());
            modelBuilder.ApplyConfiguration(new OfficeConfiguration());
            modelBuilder.ApplyConfiguration(new OfficeItemPriceConfiguration());
            modelBuilder.ApplyConfiguration(new OrderConfiguration());
            modelBuilder.ApplyConfiguration(new OrderEmailConfiguration());
            modelBuilder.ApplyConfiguration(new OrderLineConfiguration());
            modelBuilder.ApplyConfiguration(new OrderLineTruckConfiguration());
            modelBuilder.ApplyConfiguration(new OrderLineVehicleCategoryConfiguration());
            modelBuilder.ApplyConfiguration(new OrderPaymentConfiguration());
            modelBuilder.ApplyConfiguration(new OneTimeLoginConfiguration());
            modelBuilder.ApplyConfiguration(new PaymentConfiguration());
            modelBuilder.ApplyConfiguration(new PaymentHeartlandKeyConfiguration());
            modelBuilder.ApplyConfiguration(new PayStatementConfiguration());
            modelBuilder.ApplyConfiguration(new PayStatementDetailConfiguration());
            modelBuilder.ApplyConfiguration(new PayStatementDriverDateConflictConfiguration());
            modelBuilder.ApplyConfiguration(new PayStatementTicketConfiguration());
            modelBuilder.ApplyConfiguration(new PayStatementTimeConfiguration());
            modelBuilder.ApplyConfiguration(new PreventiveMaintenanceConfiguration());
            modelBuilder.ApplyConfiguration(new PricingTierConfiguration());
            modelBuilder.ApplyConfiguration(new ProductLocationConfiguration());
            modelBuilder.ApplyConfiguration(new ProductLocationPriceConfiguration());
            modelBuilder.ApplyConfiguration(new ReceiptConfiguration());
            modelBuilder.ApplyConfiguration(new ReceiptLineConfiguration());
            modelBuilder.ApplyConfiguration(new RequestedLeaseHaulerTruckConfiguration());
            modelBuilder.ApplyConfiguration(new QuoteConfiguration());
            modelBuilder.ApplyConfiguration(new QuoteEmailConfiguration());
            modelBuilder.ApplyConfiguration(new QuoteHistoryConfiguration());
            modelBuilder.ApplyConfiguration(new QuoteLineConfiguration());
            modelBuilder.ApplyConfiguration(new QuoteLineVehicleCategoryConfiguration());
            modelBuilder.ApplyConfiguration(new SubscribableEditionConfiguration());
            modelBuilder.ApplyConfiguration(new SubscriptionPaymentConfiguration());
            modelBuilder.ApplyConfiguration(new TaxRateConfiguration());
            modelBuilder.ApplyConfiguration(new TempFileConfiguration());
            modelBuilder.ApplyConfiguration(new TicketConfiguration());
            modelBuilder.ApplyConfiguration(new TimeClassificationConfiguration());
            modelBuilder.ApplyConfiguration(new TimeOffConfiguration());
            modelBuilder.ApplyConfiguration(new TrackableEmailConfiguration());
            modelBuilder.ApplyConfiguration(new TrackableEmailEventConfiguration());
            modelBuilder.ApplyConfiguration(new TrackableEmailReceiverConfiguration());
            modelBuilder.ApplyConfiguration(new TruckConfiguration());
            modelBuilder.ApplyConfiguration(new TruxEarningsConfiguration());
            modelBuilder.ApplyConfiguration(new UnitOfMeasureConfiguration());
            modelBuilder.ApplyConfiguration(new UserConfiguration());
            modelBuilder.ApplyConfiguration(new VehicleServiceConfiguration());
            modelBuilder.ApplyConfiguration(new VehicleServiceTypeConfiguration());
            modelBuilder.ApplyConfiguration(new VehicleUsageConfiguration());
            modelBuilder.ApplyConfiguration(new WialonDeviceTypeConfiguration());
            modelBuilder.ApplyConfiguration(new WorkOrderConfiguration());
            modelBuilder.ApplyConfiguration(new WorkOrderLineConfiguration());

            modelBuilder.ConfigurePersistedGrantEntity();
        }
    }
}
