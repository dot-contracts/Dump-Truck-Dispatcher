using System;
using System.ComponentModel.DataAnnotations;

namespace DispatcherWeb
{
    public enum ResourceType
    {
        [Display(Name = "Phone/Fax")]
        PhoneFax = 1,
        [Display(Name = "Email Address")]
        Email = 2,
        [Display(Name = "Address")]
        Address = 3,
        [Display(Name = "Website")]
        Website = 4,
    }

    public enum AssetType
    {
        DumpTruck = 1,
        Tractor = 2,
        Trailer = 3,
        Other = 4,
    }

    public enum BedConstructionEnum
    {
        Steel = 0,
        Aluminum = 1,
        [Display(Name = "Rock Box")]
        RockBox = 2,
    }

    public enum QuoteStatus
    {
        Pending = 0,
        Active = 1,
        Inactive = 2,
    }

    public enum FilterActiveStatus
    {
        All = 0,
        Active = 1,
        Inactive = 2,
    }

    public enum FilterServiceKind
    {
        All = 0,
        Service = 1,
        Product = 2,
    }

    public enum QuoteChangeType
    {
        [Display(Name = "Quote Body Edited")]
        QuoteBodyEdited = 1,
        [Display(Name = "Line Item Edited")]
        LineItemEdited = 2,
        [Display(Name = "Line Item Added")]
        LineItemAdded = 3,
        [Display(Name = "Line Item Deleted")]
        LineItemDeleted = 4,
    }

    public enum QuoteFieldEnum
    {
        Customer = 1,
        Contact = 2,
        //Supplier = 3,
        Name = 4,
        Description = 5,
        [Display(Name = "Proposal Date")]
        ProposalDate = 6,
        [Display(Name = "Proposal Expiry Date")]
        ProposalExpiryDate = 7,
        [Display(Name = "PO Number")]
        PoNumber = 8,
        //[Display(Name = "Load At")]
        //LoadAt = 9,
        [Display(Name = "Job Site")]
        JobSite = 10,
        [Display(Name = "Charge To")]
        ChargeTo = 11,
        Directions = 12,
        Notes = 13,
        Status = 14,

        [Display(Name = "Freight Item")]
        LineItemFreightItem = 15,
        [Display(Name = "Unit of Measure")]
        LineItemUnitOfMeasure = 16,
        [Display(Name = "Designation")]
        LineItemDesignation = 17,
        [Display(Name = "Load At")]
        LineItemLoadAt = 18,
        [Display(Name = "Price per Unit")]
        LineItemPricePerUnit = 19,
        [Display(Name = "Freight Rate")]
        LineItemFreightRate = 20,
        [Display(Name = "Quantity")]
        LineItemQuantity = 21,
        [Display(Name = "Note")]
        LineItemNote = 22,

        [Display(Name = "Spectrum #")]
        SpectrumNumber = 23,
        [Display(Name = "Inactivation Date")]
        InactivationDate = 24,
        [Display(Name = "Base Fuel Cost")]
        BaseFuelCost = 25,

        [Display(Name = "Material UOM")]
        LineItemMaterialUom = 26,
        [Display(Name = "Freight UOM")]
        LineItemFreightUom = 27,
        [Display(Name = "Material Quantity")]
        LineItemMaterialQuantity = 28,
        [Display(Name = "Freight Quantity")]
        LineItemFreightQuantity = 29,
        [Display(Name = "Deliver To")]
        LineItemDeliverTo = 30,

        [Display(Name = "Sales Person")]
        SalesPerson = 32,
        [Display(Name = "Job Number")]
        LineItemJobNumber = 33,

        [Display(Name = "Lease Hauler Rate")]
        LineItemLeaseHaulerRate = 34,

        [Display(Name = "Fuel Surcharge Calculation")]
        FuelSurchargeCalculation = 35,

        [Display(Name = "Freight Rate to Pay Drivers")]
        LineItemFreightRateToPayDrivers = 36,

        Office = 37,

        [Display(Name = "Production Pay")]
        LineItemProductionPay = 38,

        [Display(Name = "Load Based")]
        LineItemLoadBased = 39,

        [Display(Name = "Bed Construction")]
        LineItemBedConstruction = 40,

        [Display(Name = "Tax Exempt")]
        IsTaxExempt = 41,

        [Display(Name = "Material Item")]
        LineItemMaterialItem = 42,

        [Display(Name = "Hourly Driver Pay Rate")]
        LineItemHourlyDriverPayRate = 43,

        [Display(Name = "Driver Pay Time Code")]
        LineItemDriverPayTimeClassification = 44,

        //[Display(Name = "Use Driver Specific Hourly Rate")]
        //LineItemUseDriverSpecificHourlyRate = 45,

        [Display(Name = "Require Ticket")]
        LineItemRequireTicket = 46,

        [Display(Name = "Allowed Travel Time (minutes)")]
        LineItemTravelTime = 47,
    }

    public enum DesignationEnum
    {
        [Display(Name = "Freight Only")]
        FreightOnly = 1,
        [Display(Name = "Material Only")]
        MaterialOnly = 2,
        [Display(Name = "Freight and Material")]
        FreightAndMaterial = 3,
        //[Display(Name = "Rental")]
        //Rental = 4,
        [Display(Name = "Backhaul Freight Only")]
        BackhaulFreightOnly = 5,
        [Display(Name = "Backhaul Freight and Material")]
        BackhaulFreightAndMaterial = 9,
        [Display(Name = "Disposal")]
        Disposal = 6,
        [Display(Name = "Back haul freight & disposal")]
        BackHaulFreightAndDisposal = 7,
        [Display(Name = "Straight haul freight & disposal")]
        StraightHaulFreightAndDisposal = 8,
    }

    public enum PredefinedLocationKind
    {
        //JobSite = 1,
        InitialLoadAt = 2,
    }

    public enum PredefinedLocationCategoryKind
    {
        AsphaltPlant = 1,
        ConcretePlant = 2,
        LandfillOrRecycling = 3,
        Miscellaneous = 4,
        Yard = 5,
        Quarry = 6,
        SandPit = 7,
        Temporary = 8,
        //JobSite = 9,
        ProjectSite = 10,
        UnknownLoadSite = 11,
        UnknownDeliverySite = 12,
    }

    public enum PredefinedServiceKind
    {
        TemporarySite = 1,
        TemporaryLocation = 2,
    }

    public enum EmailDeliveryStatus
    {
        [Display(Name = "Not Processed")]
        NotProcessed = 0,
        Processed = 1,
        Dropped = 2,
        Deferred = 3,
        Bounced = 4,
        Delivered = 5,
        Opened = 6,
    }

    public static class EmailDeliveryStatuses
    {
        public static readonly EmailDeliveryStatus[] Failed = new[] { EmailDeliveryStatus.Dropped, EmailDeliveryStatus.Bounced };
        public static readonly EmailDeliveryStatus[] Sent = new[] { EmailDeliveryStatus.NotProcessed, EmailDeliveryStatus.Processed, EmailDeliveryStatus.Deferred };
    }

    public enum EmailReceiverKind
    {
        To = 1,
        [Display(Name = "CC")]
        Cc = 2,
        [Display(Name = "BCC")]
        Bcc = 3,
    }

    public enum SmsStatus
    {
        Unknown = 0,
        Accepted = 1,
        Delivered = 2,
        Failed = 3,
        Queued = 4,
        Received = 5,
        Receiving = 6,
        Sending = 7,
        Sent = 8,
        Undelivered = 9,
    }

    public enum OrderPriority
    {
        High = 1,
        Medium = 2,
        Low = 3,
    }

    public enum PaymentProcessor
    {
        [Display(Name = "")]
        None = 0,
        [Display(Name = "Heartland Connect")]
        HeartlandConnect = 1,
    }

    public enum OrderNotifyPreferredFormat
    {
        [Display(Name = "Neither")]
        Neither = 0x0,
        [Display(Name = "Email")]
        Email = 0x1,
        [Display(Name = "SMS")]
        Sms = 0x2,

        [Display(Name = "Both")]
        Both = Email | Sms,
    }

    public enum DriverMessageType
    {
        [Display(Name = "Email")]
        Email = 0x1,
        [Display(Name = "SMS")]
        Sms = 0x2,
    }

    public enum WorkOrderStatus
    {
        [Display(Name = "Pending")]
        Pending = 0,
        [Display(Name = "In Progress")]
        InProgress = 1,
        [Display(Name = "Complete")]
        Complete = 2,
    }

    public enum GpsPlatform
    {
        [Display(Name = "DTD Tracker")]
        DtdTracker = 1,
        [Display(Name = "Geotab")]
        Geotab = 2,
        [Display(Name = "Samsara")]
        Samsara = 3,
        [Display(Name = "IntelliShift")]
        IntelliShift = 4,
    }

    public enum FuelType
    {
        Diesel,
        Gas,
    }

    public enum FileType
    {
        Unknown = 0,

        Bmp = 1,
        Jpg = 2,
        Gif = 3,
        Png = 4,

        Doc = 11,
        Pdf = 12,
    }

    public enum ReportFormat
    {
        Pdf = 1,
        Csv = 2,
    }

    public enum ReportType
    {
        [Display(Name = "Out of Service Trucks")]
        OutOfServiceTrucks = 1,
    }

    [Flags]
    public enum DayOfWeekBitFlag
    {
        Sunday = 0x1,
        Monday = 0x2,
        Tuesday = 0x4,
        Wednesday = 0x8,
        Thursday = 0x10,
        Friday = 0x20,
        Saturday = 0x40,
    }

    public enum TaxCalculationType
    {
        [Display(Name = "Based on all of the freight and material")]
        FreightAndMaterialTotal = 1,
        [Display(Name = "Based on the line totals for any line with materials on the line")]
        MaterialLineItemsTotal = 2,
        [Display(Name = "Based on the total of all material charges")]
        MaterialTotal = 3,
        [Display(Name = "No tax calculation")]
        NoCalculation = 4,
    }

    public enum DispatchStatus
    {
        Created = 0,
        Sent = 1,
        //Received,
        Acknowledged = 3,
        Loaded = 4,
        Completed = 5,
        Error = 6,
        Canceled = 7,
    }

    public enum InvoiceStatus
    {
        Draft = 0,
        Sent = 1,
        Viewed = 2,
        [Display(Name = "Ready for export")]
        ReadyForExport = 3,
        Printed = 4,
        Approved = 5,
    }

    public enum BillingTermsEnum
    {
        [Display(Name = "Due on receipt")]
        DueOnReceipt = 0,
        [Display(Name = "Due by the first of the month")]
        DueByTheFirstOfTheMonth = 1,
        [Display(Name = "Net 10")]
        Net10 = 2,
        [Display(Name = "Net 15")]
        Net15 = 3,
        [Display(Name = "Net 30")]
        Net30 = 4,
        [Display(Name = "Net 60")]
        Net60 = 5,
        [Display(Name = "Net 5")]
        Net5 = 6,
        [Display(Name = "Net 14")]
        Net14 = 7,
    }

    public enum PreferredBillingDeliveryMethodEnum
    {
        Print = 0,
        Email = 1,
        None = 2,
    }

    public enum BillingStatus
    {
        Billed = 1,
        Unbilled = 2,
        Nonbillable = 3,
    }

    public enum DriverApplicationAction
    {
        ClockIn = 1,
        ClockOut = 2,
        AcknowledgeDispatch = 3,
        LoadDispatch = 4,
        CompleteDispatch = 5,
        AddSignature = 6,
        SaveDriverPushSubscription = 7,
        RemoveDriverPushSubscription = 8,
        ModifyDispatchTicket = 9,
        UploadDeferredBinaryObject = 10,
        UploadLogs = 11,
        ModifyEmployeeTime = 12,
        RemoveEmployeeTime = 13,
        AddDriverNote = 14,
        CancelDispatch = 15,
        MarkDispatchComplete = 16,
    }

    public enum DriverApplicationPushAction
    {
        None = 0,
        Sync = 1,
        SilentSync = 2,
    }

    public enum DeferredBinaryObjectDestination
    {
        TicketPhoto = 1,
        LoadSignature = 2,
    }

    public enum PayMethod
    {
        [Display(Name = "Hourly")]
        Hourly = 1,
        [Display(Name = "Salary")]
        Salary = 2,
        [Display(Name = "Percent of freight")]
        PercentOfFreight = 3,
    }

    public enum Shift : byte
    {
        Shift1,
        Shift2,
        Shift3,
        NoShift = byte.MaxValue,
    }

    public enum ImportType
    {
        FuelUsage = 1,
        VehicleUsage = 2,
        Customers = 3,
        Vendors = 4,
        Items = 5,
        Employees = 6,
        Trucks = 7,
        Trux = 8,
        LuckStone = 9,
        IronSheepdog = 10,
    }

    public enum TicketImportType
    {
        [Display(Name = "Luck Stone")]
        LuckStone = 9,

        [Display(Name = "Iron Sheepdog")]
        IronSheepdog = 10,
    }

    public enum TicketImportTruckMatching
    {
        [Display(Name = "License Plate")]
        ByLicensePlate = 1,

        [Display(Name = "Truck Code")]
        ByTruckCode = 2,
    }

    public enum ReadingType : byte
    {
        [Display(Name = "Mileage")]
        Miles = 0,
        [Display(Name = "Engine Hours")]
        Hours = 1,
    }

    public enum DispatchVia
    {
        None = 0,
        //Sms = 1,
        SimplifiedSms = 2,
        DriverApplication = 3,
    }

    public enum LeaseHaulerMessageType
    {
        Sms,
        Email,
    }

    public enum DispatchListViewEnum
    {
        [Display(Name = "Open dispatches")]
        OpenDispatches = 0,
        [Display(Name = "Drivers not clocked in")]
        DriversNotClockedIn = 1,
        [Display(Name = "Unacknowledged dispatches")]
        UnacknowledgedDispatches = 2,
        [Display(Name = "Trucks with drivers and no dispatches")]
        TrucksWithDriversAndNoDispatches = 3,
        [Display(Name = "All trucks")]
        AllTrucks = 4,
    }

    public enum QuickbooksIntegrationKind
    {
        None = 0,
        [Display(Name = "QuickBooks Desktop")]
        Desktop = 1,
        [Display(Name = "QuickBooks Online Export")]
        QboExport = 3,
        [Display(Name = "Transaction Pro Export for QuickBooks Online")]
        TransactionProExport = 4,
        [Display(Name = "SBT csv Export")]
        SbtCsvExport = 5,
        [Display(Name = "Hollis Export")]
        HollisExport = 6,
        [Display(Name = "Sage Export")]
        SageExport = 7,
        [Display(Name = "J&J Export")]
        JAndJExport = 8,
    }

    public enum QuickbooksDeprecatedIntegrationKind
    {
        /*
        [Display(Name = "QuickBooks Online (Deprecated)")]
        Online = 2,
        */
    }

    public enum TicketListStatusFilterEnum
    {
        [Display(Name = "Missing Tickets Only")]
        MissingTicketsOnly = 1,
        [Display(Name = "Entered Tickets Only")]
        EnteredTicketsOnly = 2,
        [Display(Name = "Potential Duplicate Tickets")]
        PotentialDuplicateTickets = 3,
    }

    public enum SendSmsOnDispatchingEnum
    {
        [Display(Name = "Don't send")]
        DontSend = 1,
        [Display(Name = "Send when user not clocked in")]
        SendWhenUserNotClockedIn = 2,
        [Display(Name = "Send for all dispatches")]
        SendForAllDispatches = 3,
    }

    public enum ShowFuelSurchargeOnInvoiceEnum
    {
        None = 0, //default for historical invoices
        //[Display(Name = "Combined with freight")]
        //CombinedWithFreight = 1,
        [Display(Name = "Line item per ticket")]
        LineItemPerTicket = 2,
        [Display(Name = "Single line item at the bottom")]
        SingleLineItemAtTheBottom = 3,
    }

    public enum StaggeredTimeKind
    {
        None = 0,
        SetInterval = 1,
        //SpecificStartTimes = 2
    }

    public enum InvoicingMethodEnum
    {
        [Display(Name = "Aggregate all tickets on one invoice")]
        AggregateAllTickets = 0,
        [Display(Name = "Separate the tickets by job number")]
        SeparateTicketsByJobNumber = 1,
        [Display(Name = "Separate invoice per ticket")]
        SeparateInvoicePerTicket = 2,
        [Display(Name = "Separate the tickets by job")]
        SeparateTicketsByJob = 3,
    }

    public enum DriverDateConflictKind
    {
        BothProductionAndHourlyPay = 1,
        ProductionPayTimeButNoTickets = 2,
    }

    public enum ItemType
    {
        [Display(Name = "System")]
        System = 0,
        [Display(Name = "Service")]
        Service = 1,
        [Display(Name = "Inventory Part")]
        InventoryPart = 2,
        //[Display(Name = "Inventory Assembly")]
        //InventoryAssembly = 3,
        [Display(Name = "Non-inventory Part")]
        NonInventoryPart = 4,
        [Display(Name = "Other Charge")]
        OtherCharge = 5,
        [Display(Name = "Discount")]
        Discount = 6,
        [Display(Name = "Payment")]
        Payment = 7,
        [Display(Name = "Sales Tax Item")]
        SalesTaxItem = 8,
    }

    public static class ItemTypes
    {
        public static readonly ItemType[] Material = new[]
        {
            ItemType.InventoryPart,
            ItemType.NonInventoryPart,
        };

        public static readonly ItemType[] Freight = new[]
        {
            ItemType.Service,
        };

        public static readonly ItemType[] Other = new[]
        {
            ItemType.System,
            ItemType.OtherCharge,
            ItemType.Discount,
            ItemType.Payment,
            ItemType.SalesTaxItem,
        };
    }

    public enum EntityEnum
    {
        Dispatch = 1,
        EmployeeTime = 2,
        DriverAssignment = 3,
        EmployeeTimeClassification = 4,
        TimeClassification = 5,
        ChatMessage = 6,
        Settings = 7,
        Order = 8,
        OrderLine = 9,
        OrderLineTruck = 10,
        Driver = 11,
        LeaseHaulerDriver = 12,
        Truck = 13,
        LeaseHaulerTruck = 14,
        LeaseHauler = 15,
        User = 16,
        Customer = 17,
        Location = 18,
        Item = 19,
        VehicleCategory = 20,
        UnitOfMeasure = 21,
        AvailableLeaseHaulerTruck = 22,
        Insurance = 23,
        CustomerContact = 24,
        LeaseHaulerUser = 25,
    }

    public enum ChangeType
    {
        Removed = 0, //deleted, canceled or completed
        Modified = 1, //created or updated
    }

    public enum MobilePlatform
    {
        PWA = 2,
        Android = 3,
        IOS = 4,
    }

    public enum FcmPushMessageType
    {
        ReloadSpecificEntities = 1,
        ReloadAllEntities = 2,
    }

    public enum PayStatementItemKind
    {
        Time = 1,
        Ticket = 2,
    }

    public enum WialonMeasureUnits
    {
        SI = 0,
        US = 1,
        Imperial = 2,
        MetricWithGallons = 3,
    }

    public enum InvoiceTemplateEnum
    {
        [Display(Name = "Invoice 1")]
        Invoice1 = 1,
        [Display(Name = "Invoice 2")]
        Invoice2 = 2,
        [Display(Name = "Invoice 3")]
        Invoice3 = 3,
        [Display(Name = "Invoice 4")]
        Invoice4 = 4,
        [Display(Name = "Invoice 5")]
        Invoice5 = 5,
        [Display(Name = "Minimal Description")]
        MinimalDescription = 6,
        [Display(Name = "Invoice 6")]
        Invoice6 = 7,
        [Display(Name = "Invoice 7")]
        Invoice7 = 8,
        [Display(Name = "Grouped by Customer, Job, and Product")]
        Invoice8 = 9,
        [Display(Name = "Grouped by Customer, Job, Product, and Location")]
        Invoice9 = 10,
        [Display(Name = "Invoice 8")]
        Invoice10 = 11,
    }

    public enum ChildInvoiceLineKind
    {
        None = 0,
        FuelSurchargeLinePerTicket = 1,
        BottomFuelSurchargeLine = 2,
    }

    public enum AnalyzeRevenueBy
    {
        Driver = 0,
        Truck = 1,
        Customer = 2,
        Date = 3,
    }

    public enum TruckPositionActivityType
    {
        Still = 1,
        OnFoot = 2,
        Walking = 3,
        Running = 4,
        InVehicle = 5,
        OnBicycle = 6,
        Unknown = 7,
    }

    public enum TruckPositionGeofenceAction
    {
        Enter = 1,
        Exit = 2,
    }

    public enum TruckPositionEvent
    {
        MotionChange = 1,
        Geofence = 2,
        Heartbeat = 3,
        ProviderChange = 4,
    }

    public enum HostEmailType
    {
        [Display(Name = "Release Notes")]
        ReleaseNotes = 0x1,
        [Display(Name = "Transactional")]
        Transactional = 0x2,
        [Display(Name = "Service Status")]
        ServiceStatus = 0x4,
        [Display(Name = "Marketing")]
        Marketing = 0x8,
    }

    [Flags] //we can't reuse HostEmailType because [Flag] enums can't be used with GetEnumSelectList
    public enum HostEmailPreference
    {
        [Display(Name = "Release Notes")]
        ReleaseNotes = 0x1,
        [Display(Name = "Transactional")]
        Transactional = 0x2,
        [Display(Name = "Service Status")]
        ServiceStatus = 0x4,
        [Display(Name = "Marketing")]
        Marketing = 0x8,
    }

    public enum DriverIsPaidForLoadBasedOnEnum
    {
        [Display(Name = "Ticket Date")]
        TicketDate = 1,
        [Display(Name = "Order Date")]
        OrderDate = 2,
    }

    public enum DriverAppImageResolutionEnum
    {
        Small = 1,
        Medium = 2,
        Large = 3,
        Original = 4,
    }

    public enum LogoType
    {
        ApplicationLogo,
        ReportsLogo,
    }

    public enum PayStatementReportOrientation
    {
        [Display(Name = "Portrait")]
        Portrait = 1,
        [Display(Name = "Landscape")]
        Landscape = 2,
    }

    public enum TruckTripTypes
    {
        ToLoadSite = 0,
        ToDumpSite = 1,
    }

    public enum JobStatus
    {
        [Display(Name = "Scheduled")]
        Scheduled = 0,
        [Display(Name = "In Progress")]
        InProgress = 1,
        [Display(Name = "Completed")]
        Completed = 2,
    }

    public enum DeviceTypeEnum
    {
        [Display(Name = "Tablet")]
        Tablet = 0,
        [Display(Name = "GPS Device")]
        GpsDevice = 1,
        [Display(Name = "Camera")]
        Camera = 2,
    }

    public enum LeaseHaulerRequestStatus
    {
        Requested = 0,
        Approved = 1,
        Rejected = 2,
    }

    public enum AuditLogErrorState
    {
        [Display(Name = "Success")]
        Success = 0,
        [Display(Name = "Has error")]
        HasError = 1,
        [Display(Name = "Meaningful Errors")]
        MeaningfulErrors = 2,
    }

    public enum ScheduleTruckSortKind
    {
        ByTruckCode = 1,
        ByDriverSeniority = 2,
    }

    public enum UnitOfMeasureBaseEnum
    {
        [Display(Name = "Hours")]
        Hours = 1,
        [Display(Name = "Tons")]
        Tons = 2,
        [Display(Name = "Loads")]
        Loads = 3,
        [Display(Name = "Cubic Yards")]
        CubicYards = 4,
        [Display(Name = "Each")]
        Each = 5,
        [Display(Name = "Cubic Meters")]
        CubicMeters = 6,
        [Display(Name = "Miles")]
        Miles = 7,
        [Display(Name = "Drive Miles")]
        DriveMiles = 8,
        [Display(Name = "Air Miles")]
        AirMiles = 9,
        [Display(Name = "Drive KMs")]
        DriveKMs = 10,
        [Display(Name = "Air KMs")]
        AirKMs = 11,
    }

    public enum FulcrumEntity
    {
        [Display(Name = "Trucks")]
        Truck = 1,
        [Display(Name = "Drivers")]
        Driver = 2,
        [Display(Name = "Customers")]
        Customer = 3,
        [Display(Name = "Products")]
        Product = 4,
        [Display(Name = "Lease Haulers")]
        LeaseHauler = 5,
        [Display(Name = "Tax Rates")]
        TaxRate = 6,
    }

    public enum FulcrumDtdTicketAction
    {
        Create = 1,
        Update = 2,
        Delete = 3,
    }

    public enum DocumentType
    {
        Insurance = 1,
        Certification = 2,
    }

    public enum FuelSurchargeCalculationType
    {
        BasedOnActualFuelCost = 1,
        SimplePercentage = 2,
    }

    public enum RequiredTicketEntryEnum
    {
        None = 0,
        Always = 1,
        ByJobDefaultingToRequired = 2,
        ByJobDefaultingToNotRequired = 3,
    }

    public enum VehicleServiceTypeEnum
    {
        PreventiveMaintenance = 1,
        OutsideRepair = 2,
        OutsideWarrantyWork = 3,
        InsideRepair = 4,
    }

    public enum ListCacheSide
    {
        Backend,
        Frontend,
    }

    public static class ListCacheSides
    {
        public static readonly ListCacheSide[] All =
        [
            ListCacheSide.Backend,
            ListCacheSide.Frontend,
        ];
    }

    public enum SyncRequestDefaultFilter
    {
        None = 0,
        ByDriverIds = 1,
    }
}
