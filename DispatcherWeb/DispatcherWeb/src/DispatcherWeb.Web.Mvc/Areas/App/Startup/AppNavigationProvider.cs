using Abp.Application.Features;
using Abp.Application.Navigation;
using Abp.Authorization;
using Abp.Localization;
using DispatcherWeb.Authorization;
using DispatcherWeb.Configuration;
using DispatcherWeb.Features;
using DispatcherWeb.Web.Models.Layout;

namespace DispatcherWeb.Web.Areas.App.Startup
{
    public class AppNavigationProvider : NavigationProvider
    {
        public const string MenuName = "App";

        public override void SetNavigation(INavigationProviderContext context)
        {
            var menu = context.Manager.Menus[MenuName] =
                new MenuDefinition(MenuName, new FixedLocalizableString("Main Menu"));

            menu
                .AddItem(
                    new MenuItemDefinition(
                        AppPageNames.Host.Dashboard,
                        L("Dashboard"),
                        customData: new MenuItemCustomData { Id = "HostDashboardNavbarItem" },
                        url: "App/HostDashboard",
                        icon: "fa-regular fa-display-chart-up",
                        permissionDependency: new SimplePermissionDependency(AppPermissions.Pages_Administration_Host_Dashboard)
                    )
                ).AddItem(
                    new MenuItemDefinition(
                        AppPageNames.Host.Tenants,
                        L("Tenants"),
                        customData: new MenuItemCustomData { Id = "TenantsNavbarItem" },
                        url: "App/Tenants",
                        icon: "fa-regular fa-list",
                        permissionDependency: new SimplePermissionDependency(AppPermissions.Pages_Tenants)
                    )
                ).AddItem(
                    new MenuItemDefinition(
                        AppPageNames.Host.Editions,
                        L("Editions"),
                        customData: new MenuItemCustomData { Id = "EditionsNavbarItem" },
                        url: "App/Editions",
                        icon: "fa-regular fa-grid-2-plus",
                        permissionDependency: new SimplePermissionDependency(AppPermissions.Pages_Editions)
                    )
                ).AddItem(
                    new MenuItemDefinition(
                        AppPageNames.Host.VehicleCategories,
                        L("VehicleCategories"),
                        customData: new MenuItemCustomData { Id = "VehicleCategoriesNavbarItem" },
                        url: "App/VehicleCategories",
                        icon: "fa-regular fa-truck-moving",
                        permissionDependency: new SimplePermissionDependency(AppPermissions.Pages_VehicleCategories)
                    )
                ).AddItem(
                    new MenuItemDefinition(
                        AppPageNames.Host.HostEmails,
                        L("HostEmails"),
                        customData: new MenuItemCustomData { Id = "HostEmailsNavbarItem" },
                        url: "App/HostEmails",
                        icon: "fa-regular fa-envelope",
                        permissionDependency: new SimplePermissionDependency(AppPermissions.Pages_HostEmails)
                    )
                ).AddItem(
                    new MenuItemDefinition(
                        AppPageNames.Host.CustomerNotifications,
                        L("CustomerNotifications"),
                        customData: new MenuItemCustomData { Id = "CustomerNotificationsNavbarItem" },
                        url: "App/CustomerNotifications",
                        icon: "fa-regular fa-bell",
                        permissionDependency: new SimplePermissionDependency(AppPermissions.Pages_CustomerNotifications)
                    )
                ).AddItem(
                    new MenuItemDefinition(
                        AppPageNames.Tenant.Dashboard,
                        L("Dashboard"),
                        customData: new MenuItemCustomData { Id = "TenantDashboardNavbarItem" },
                        url: "App/Dashboard",
                        icon: "fa-regular fa-display-chart-up",
                        permissionDependency: new SimplePermissionDependency(AppPermissions.Pages_Dashboard),
                        featureDependency: new SimpleFeatureDependency(AppFeatures.FreeFunctionality)
                    )
                ).AddItem(
                    new MenuItemDefinition(
                        AppPageNames.Tenant.LeaseHaulerTruckRequest,
                        L("TruckRequestsList"),
                        customData: new MenuItemCustomData { Id = "LeaseHaulerTruckRequestsNavbarItem" },
                        url: "app/TruckRequests",
                        icon: "fa-regular fa-truck-moving",
                        permissionDependency: new SimplePermissionDependency(AppPermissions.LeaseHaulerPortal_Truck_Request),
                        featureDependency: new SimpleFeatureDependency(AppFeatures.LeaseHaulerPortal)
                    )
                ).AddItem(
                    new MenuItemDefinition(
                        AppPageNames.Tenant.Customers,
                        L("Customers"),
                        customData: new MenuItemCustomData { Id = "CustomersNavbarItem" },
                        url: "app/customers",
                        icon: "fa-regular fa-user-tie",
                        permissionDependency: new SimplePermissionDependency(AppPermissions.Pages_Customers),
                        featureDependency: new SimpleFeatureDependency(AppFeatures.PaidFunctionality)
                    )
                ).AddItem(
                    new MenuItemDefinition(
                        AppPageNames.Tenant.ProductsOrServices,
                        L("ProductsOrServices"),
                        customData: new MenuItemCustomData { Id = "ProductsOrServicesNavbarItem" },
                        icon: "fa-regular fa-boxes-stacked",
                        permissionDependency: new SimplePermissionDependency(AppPermissions.Pages_Items)
                    ).AddItem(new MenuItemDefinition(
                            AppPageNames.Tenant.Items,
                            L("Products"),
                            customData: new MenuItemCustomData { Id = "ProductsNavbarItem" },
                            url: "app/items",
                            icon: "fa-regular fa-envelope-open-dollar",
                            permissionDependency: new SimplePermissionDependency(AppPermissions.Pages_Items)
                        )
                    ).AddItem(new MenuItemDefinition(
                            AppPageNames.Tenant.PricingTiers,
                            L("PricingTiers"),
                            customData: new MenuItemCustomData { Id = "PricingTiers" },
                            url: "app/pricingtiers",
                            icon: "fa-regular fa-envelope-open-dollar",
                            permissionDependency: new SimplePermissionDependency(AppPermissions.Pages_Items_PricingTiers),
                            featureDependency: new SimpleFeatureDependency(AppFeatures.PricingTiers)
                        )
                    ).AddItem(new MenuItemDefinition(
                            AppPageNames.Tenant.TaxRates,
                            L("TaxRates"),
                            customData: new MenuItemCustomData { Id = "TaxRatesNavbarItem" },
                            url: "app/taxrates",
                            icon: "fa-solid fa-hand-holding-dollar",
                            permissionDependency: new SimplePermissionDependency(AppPermissions.Pages_Items_TaxRates_Edit)
                        )
                    ).AddItem(new MenuItemDefinition(
                            AppPageNames.Tenant.HaulZones,
                            L("HaulZones"),
                            customData: new MenuItemCustomData { Id = "HaulZonesNavbarItem" },
                            url: "app/haulzones",
                            icon: "fa-regular fa-map",
                            permissionDependency: new SimplePermissionDependency(AppPermissions.Pages_Items_HaulZones)
                        )
                    )
                ).AddItem(
                    new MenuItemDefinition(
                        AppPageNames.Tenant.Drivers,
                        L("Drivers"),
                        customData: new MenuItemCustomData { Id = "DriversNavbarItem" },
                        url: "app/drivers",
                        icon: "fa-regular fa-steering-wheel",
                        permissionDependency: new SimplePermissionDependency(AppPermissions.Pages_Drivers),
                        featureDependency: new SimpleFeatureDependency(AppFeatures.FreeFunctionality)
                    )
                ).AddItem(
                    new MenuItemDefinition(
                        AppPageNames.Tenant.Locations,
                        L("Locations"),
                        customData: new MenuItemCustomData { Id = "LocationsNavbarItem" },
                        url: "app/locations",
                        icon: "fa-regular fa-location-dot",
                        permissionDependency: new SimplePermissionDependency(AppPermissions.Pages_Locations),
                        featureDependency: new SimpleFeatureDependency(AppFeatures.PaidFunctionality)
                    )
                ).AddItem(
                    new MenuItemDefinition(
                        AppPageNames.Tenant.Quotes,
                        L("Quotes"),
                        customData: new MenuItemCustomData { Id = "QuotesNavbarItem" },
                        url: "app/quotes",
                        icon: "fa-regular fa-file-contract",
                        permissionDependency: new SimplePermissionDependency(AppPermissions.Pages_Quotes_View)
                    )
                ).AddItem(
                    new MenuItemDefinition(
                        AppPageNames.Tenant.ViewOrders,
                        L("Orders"),
                        customData: new MenuItemCustomData { Id = "ViewOrdersNavbarItem" },
                        url: "app/orders",
                        icon: "fa-regular fa-clipboard-list-check",
                        permissionDependency: new SimplePermissionDependency(AppPermissions.Pages_Orders_View)
                    )
                ).AddItem(
                    new MenuItemDefinition(
                        AppPageNames.Tenant.Orders,
                        L("Dispatching"),
                        customData: new MenuItemCustomData { Id = "DispatchingNavbarItem" },
                        icon: "fa-regular fa-truck-ramp-box",
                        featureDependency: new SimpleFeatureDependency(AppFeatures.PaidFunctionality)
                    ).AddItem(
                        new MenuItemDefinition(
                            AppPageNames.Tenant.Schedule,
                            L("Schedule"),
                            customData: new MenuItemCustomData { Id = "ScheduleNavbarItem" },
                            url: "app/scheduling",
                            icon: "fa-regular fa-calendar-range",
                            permissionDependency: new SimplePermissionDependency(AppPermissions.Pages_Schedule)
                        )
                    ).AddItem(
                        new MenuItemDefinition(
                            AppPageNames.Tenant.DriverMessages,
                            L("DriverMessagesMenuItem"),
                            customData: new MenuItemCustomData { Id = "DriverMessagesNavbarItem" },
                            url: "app/DriverMessages",
                            icon: "fa-regular fa-message",
                            permissionDependency: new SimplePermissionDependency(AppPermissions.Pages_DriverMessages)
                        )
                    ).AddItem(
                        new MenuItemDefinition(
                            AppPageNames.Tenant.TruckDispatchList,
                            L("TruckDispatchList"),
                            customData: new MenuItemCustomData { Id = "TruckDispatchListNavbarItem" },
                            url: "app/Dispatches/TruckDispatchList",
                            icon: "fa-regular fa-calendar-lines",
                            permissionDependency: new SimplePermissionDependency(AppPermissions.Pages_Dispatches),
                            featureDependency: new DispatchSettingFeatureDependency()
                        )
                    ).AddItem(
                        new MenuItemDefinition(
                            AppPageNames.Tenant.DriverAssignment,
                            L("DriverAssignment"),
                            customData: new MenuItemCustomData { Id = "DriverAssignmentNavbarItem" },
                            url: "app/driverassignments",
                            icon: "fa-regular fa-steering-wheel",
                            permissionDependency: new SimplePermissionDependency(AppPermissions.Pages_DriverAssignment)
                        )
                    )
                ).AddItem(
                    new MenuItemDefinition(
                        AppPageNames.Tenant.TicketsAndLoads,
                        L("TicketsAndLoads"),
                        customData: new MenuItemCustomData { Id = "TicketsAndLoadsNavbarItem" },
                        icon: "fa-regular fa-ticket",
                        featureDependency: new SimpleFeatureDependency(AppFeatures.PaidFunctionality)
                    ).AddItem(
                        new MenuItemDefinition(
                            AppPageNames.Tenant.Dispatches,
                            L("LoadHistory"),
                            customData: new MenuItemCustomData { Id = "LoadHistoryNavbarItem" },
                            url: "app/Dispatches",
                            icon: "fa-regular fa-trailer",
                            permissionDependency: new SimplePermissionDependency(AppPermissions.Pages_Dispatches),
                            featureDependency: new DispatchSettingFeatureDependency()
                        )
                    ).AddItem(
                        new MenuItemDefinition(
                            AppPageNames.Tenant.Tickets,
                            L("Tickets"),
                            customData: new MenuItemCustomData { Id = "TicketsNavbarItem" },
                            url: "app/tickets",
                            icon: "fa-regular fa-ticket-simple",
                            permissionDependency: new SimplePermissionDependency(AppPermissions.Pages_Tickets_View,
                                AppPermissions.CustomerPortal_TicketList)
                        )
                    ).AddItem(
                        new MenuItemDefinition(
                            AppPageNames.Tenant.TicketsByDriver,
                            L("TicketsByDriver"),
                            customData: new MenuItemCustomData { Id = "TicketsByDriverNavbarItem" },
                            url: "app/tickets/TicketsByDriver",
                            icon: "fa-regular fa-ticket-simple",
                            permissionDependency: new SimplePermissionDependency(AppPermissions.Pages_TicketsByDriver)
                        )
                    )
                ).AddItem(
                    new MenuItemDefinition(
                        AppPageNames.Tenant.Invoices,
                        L("Invoices"),
                        customData: new MenuItemCustomData { Id = "InvoicesNavbarItem" },
                        url: "app/invoices",
                        icon: "fa-regular fa-file-invoice",
                        permissionDependency: new SimplePermissionDependency(AppPermissions.Pages_Invoices, AppPermissions.CustomerPortal_Invoices),
                        featureDependency: new SimpleFeatureDependency(AppFeatures.AllowInvoicingFeature)
                    )
                ).AddItem(
                    new MenuItemDefinition(
                        AppPageNames.Tenant.LeaseHauler,
                        L("LeaseHauler"),
                        customData: new MenuItemCustomData { Id = "LeaseHaulerNavbarItem" },
                        icon: "fa-regular fa-truck-moving",
                        featureDependency: new SimpleFeatureDependency(true, AppFeatures.AllowLeaseHaulersFeature)
                    ).AddItem(
                        new MenuItemDefinition(
                            AppPageNames.Tenant.LeaseHaulers,
                            L("LeaseHaulers"),
                            customData: new MenuItemCustomData { Id = "LeaseHaulersNavbarItem" },
                            url: "app/leasehaulers",
                            icon: "fa-regular fa-truck-moving",
                            permissionDependency: new SimplePermissionDependency(AppPermissions.Pages_LeaseHaulers_Edit)
                        )
                    ).AddItem(
                        new MenuItemDefinition(
                            AppPageNames.Tenant.LeaseHaulerRequests,
                            L("LeaseHaulerRequests"),
                            customData: new MenuItemCustomData { Id = "LeaseHaulerRequestsNavbarItem" },
                            url: "app/leasehaulerrequests",
                            icon: "fa-regular fa-truck-moving",
                            permissionDependency: new SimplePermissionDependency(AppPermissions.Pages_LeaseHaulerRequests)
                        )
                    ).AddItem(
                        new MenuItemDefinition(
                            AppPageNames.Tenant.LeaseHaulerStatements,
                            L("LeaseHaulerStatements"),
                            customData: new MenuItemCustomData { Id = "LeaseHaulerStatementsNavbarItem" },
                            url: "app/leasehaulerstatements",
                            icon: "fa-regular fa-truck-moving",
                            permissionDependency: new SimplePermissionDependency(AppPermissions.Pages_LeaseHaulerStatements),
                            featureDependency: new SimpleFeatureDependency(AppFeatures.AllowLeaseHaulersFeature)
                        )
                    ).AddItem(
                        new MenuItemDefinition(
                            AppPageNames.Tenant.LeaseHaulerPerformance,
                            L("LeaseHaulerPerformance"),
                            customData: new MenuItemCustomData { Id = "LeaseHaulerPerformanceNavbarItem" },
                            url: "app/leasehaulerperformance",
                            icon: "fa-regular fa-chart-line",
                            permissionDependency: new SimplePermissionDependency(AppPermissions.Pages_LeaseHaulerPerformance)
                        )
                    )
                ).AddItem(
                    new MenuItemDefinition(
                        AppPageNames.Tenant.Time,
                        L("Time"),
                        customData: new MenuItemCustomData { Id = "TimeNavbarItem" },
                        icon: "fa-regular fa-clock",
                        featureDependency: new SimpleFeatureDependency(AppFeatures.FreeFunctionality)
                    ).AddItem(new MenuItemDefinition(
                            AppPageNames.Tenant.TimeEntry,
                            L("TimeEntry"),
                            customData: new MenuItemCustomData { Id = "TimeEntryNavbarItem" },
                            url: "app/employeetime",
                            icon: "fa-regular fa-stopwatch",
                            permissionDependency: new SimplePermissionDependency(AppPermissions.Pages_TimeEntry),
                            featureDependency: new SimpleFeatureDependency(AppFeatures.PaidFunctionality)
                        )
                    ).AddItem(new MenuItemDefinition(
                            AppPageNames.Tenant.TimeOff,
                            L("TimeOff"),
                            customData: new MenuItemCustomData { Id = "TimeOffNavbarItem" },
                            url: "app/timeoff",
                            icon: "fa-regular fa-user-clock",
                            permissionDependency: new SimplePermissionDependency(AppPermissions.Pages_TimeOff),
                            featureDependency: new SimpleFeatureDependency(AppFeatures.PaidFunctionality)
                        )
                    ).AddItem(new MenuItemDefinition(
                            AppPageNames.Tenant.TimeClassifications,
                            L("TimeClassifications"),
                            customData: new MenuItemCustomData { Id = "TimeClassificationsNavbarItem" },
                            url: "app/TimeClassifications",
                            icon: "fa-regular fa-list-timeline",
                            permissionDependency: new SimplePermissionDependency(AppPermissions.Pages_TimeEntry_EditTimeClassifications),
                            featureDependency: new SimpleFeatureDependency(AppFeatures.PaidFunctionality)
                        )
                    )
                ).AddItem(
                    new MenuItemDefinition(
                        AppPageNames.Tenant.DriverPayStatements,
                        L("DriverPay"),
                        customData: new MenuItemCustomData { Id = "DriverPayNavbarItem" },
                        url: "app/driverpaystatements",
                        icon: "fa-regular fa-envelope-open-dollar",
                        permissionDependency: new SimplePermissionDependency(AppPermissions.Pages_Backoffice_DriverPay),
                        featureDependency: new SimpleFeatureDependency(AppFeatures.PaidFunctionality)
                    )
                ).AddItem(
                    new MenuItemDefinition(
                        AppPageNames.Tenant.Trucks,
                        L("Trucks"),
                        customData: new MenuItemCustomData { Id = "TruckNavbarItem" },
                        icon: "fa-regular fa-truck-container",
                        permissionDependency: new SimplePermissionDependency(AppPermissions.Pages_Trucks),
                        featureDependency: new SimpleFeatureDependency(AppFeatures.FreeFunctionality)
                    ).AddItem(
                        new MenuItemDefinition(
                            AppPageNames.Tenant.Trucks,
                            L("Trucks"),
                            customData: new MenuItemCustomData { Id = "TrucksNavbarItem" },
                            url: "app/trucks",
                            icon: "fa-regular fa-truck-container",
                            permissionDependency: new SimplePermissionDependency(AppPermissions.Pages_Trucks),
                            featureDependency: new SimpleFeatureDependency(AppFeatures.FreeFunctionality)
                        )
                    ).AddItem(
                        new MenuItemDefinition(
                            AppPageNames.Host.ServiceType,
                            L("ServiceType"),
                            customData: new MenuItemCustomData { Id = "VehicleServiceTypeNavbarItem" },
                            url: "app/vehicleservicetypes",
                            icon: "fa-regular fa-toolbox",
                            permissionDependency: new SimplePermissionDependency(AppPermissions.Pages_VehicleServiceTypes_View),
                            featureDependency: new SimpleFeatureDependency(AppFeatures.PaidFunctionality)
                        )
                    ).AddItem(
                        new MenuItemDefinition(
                            AppPageNames.Tenant.VehicleServices,
                            L("VehicleMaintenanceServices"),
                            customData: new MenuItemCustomData { Id = "VehicleMaintenanceServicesNavbarItem" },
                            url: "app/vehicleservices",
                            icon: "fa-regular fa-toolbox",
                            permissionDependency: new SimplePermissionDependency(AppPermissions.Pages_VehicleService_View),
                            featureDependency: new SimpleFeatureDependency(AppFeatures.FreeFunctionality)
                        )
                    ).AddItem(
                        new MenuItemDefinition(
                            AppPageNames.Tenant.PreventiveMaintenanceSchedule,
                            L("PreventiveMaintenanceSchedule"),
                            customData: new MenuItemCustomData { Id = "PreventiveMaintenanceScheduleNavbarItem" },
                            url: "app/preventivemaintenanceschedule",
                            icon: "fa-regular fa-calendar-day",
                            permissionDependency: new SimplePermissionDependency(AppPermissions.Pages_PreventiveMaintenanceSchedule_View),
                            featureDependency: new SimpleFeatureDependency(AppFeatures.FreeFunctionality)
                        )
                    ).AddItem(
                        new MenuItemDefinition(
                            AppPageNames.Tenant.WorkOrders,
                            L("WorkOrders"),
                            customData: new MenuItemCustomData { Id = "WorkOrdersNavbarItem" },
                            url: "app/workorders",
                            icon: "fa-regular fa-car-wrench",
                            permissionDependency: new SimplePermissionDependency(AppPermissions.Pages_WorkOrders_View),
                            featureDependency: new SimpleFeatureDependency(AppFeatures.FreeFunctionality)
                        )
                    ).AddItem(
                        new MenuItemDefinition(
                            AppPageNames.Tenant.FuelPurchases,
                            L("FuelPurchases"),
                            customData: new MenuItemCustomData { Id = "FuelPurchasesNavbarItem" },
                            url: "app/fuel",
                            icon: "fa-regular fa-gas-pump",
                            permissionDependency: new SimplePermissionDependency(AppPermissions.Pages_FuelPurchases_View),
                            featureDependency: new SimpleFeatureDependency(AppFeatures.PaidFunctionality)
                        )
                    ).AddItem(
                        new MenuItemDefinition(
                            AppPageNames.Tenant.OutOfSeriveTrucks,
                            L("OutOfServiceTrucksReport"),
                            customData: new MenuItemCustomData { Id = "OutOfServiceTrucksReportNavbarItem" },
                            url: "app/OutOfServiceTrucksReport",
                            icon: "fa-regular fa-road-circle-xmark",
                            permissionDependency: new SimplePermissionDependency(AppPermissions.Pages_Reports_OutOfServiceTrucks),
                            featureDependency: new SimpleFeatureDependency(AppFeatures.FreeFunctionality)
                        )
                    ).AddItem(
                        new MenuItemDefinition(
                            AppPageNames.Tenant.VehicleUsages,
                            L("VehicleUsageMenuItem"),
                            customData: new MenuItemCustomData { Id = "VehicleUsageNavbarItem" },
                            url: "app/vehicleusages",
                            icon: "fa-regular fa-tire",
                            permissionDependency: new SimplePermissionDependency(AppPermissions.Pages_VehicleUsages_View)
                        )
                    )
                ).AddItem(new MenuItemDefinition(
                        AppPageNames.Tenant.Reports,
                        L("Reports"),
                        customData: new MenuItemCustomData { Id = "ReportsNavbarItem" },
                        icon: "fa-regular fa-print",
                        featureDependency: new SimpleFeatureDependency(AppFeatures.PaidFunctionality)
                    ).AddItem(new MenuItemDefinition(
                            AppPageNames.Tenant.PrintOrders,
                            L("DeliveryReport"),
                            customData: new MenuItemCustomData { Id = "PrintOrdersWithDeliveryInfoNavbarItem" },
                            url: "#",
                            icon: "fa-regular fa-file-chart-pie",
                            permissionDependency: new SimplePermissionDependency(AppPermissions.Pages_PrintOrders)
                        )
                    ).AddItem(new MenuItemDefinition(
                            AppPageNames.Tenant.RevenueBreakdown,
                            L("RevenueBreakdownReport"),
                            customData: new MenuItemCustomData { Id = "RevenueBreakdownReportNavbarItem" },
                            url: "app/RevenueBreakdownReport",
                            icon: "fa-regular fa-file-invoice-dollar",
                            permissionDependency: new SimplePermissionDependency(AppPermissions.Pages_Reports_RevenueBreakdown)
                        )
                    ).AddItem(new MenuItemDefinition(
                            AppPageNames.Tenant.RevenueBreakdownByTruck,
                            L("RevenueBreakdownByTruckReport"),
                            customData: new MenuItemCustomData { Id = "RevenueBreakdownByTruckReportNavbarItem" },
                            url: "app/RevenueBreakdownByTruckReport",
                            icon: "fa-regular fa-file-invoice-dollar",
                            permissionDependency: new SimplePermissionDependency(AppPermissions.Pages_Reports_RevenueBreakdownByTruck)
                        )
                    ).AddItem(new MenuItemDefinition(
                            AppPageNames.Tenant.RevenueAnalysis,
                            L("RevenueAnalysis"),
                            customData: new MenuItemCustomData { Id = "RevenueAnalysisReportNavbarItem" },
                            url: "app/RevenueAnalysisReport",
                            icon: "fa-regular fa-file-chart-column",
                            permissionDependency: new SimplePermissionDependency(AppPermissions.Pages_Reports_RevenueAnalysis)
                        )
                    ).AddItem(new MenuItemDefinition(
                            AppPageNames.Tenant.ScheduledReports,
                            L("ScheduledReports"),
                            customData: new MenuItemCustomData { Id = "ScheduledReportsNavbarItem" },
                            url: "app/ScheduledReports",
                            icon: "fa-regular fa-calendar-lines-pen",
                            permissionDependency: new SimplePermissionDependency(AppPermissions.Pages_Reports_ScheduledReports)
                        )
                    ).AddItem(new MenuItemDefinition(
                            AppPageNames.Tenant.Receipts,
                            L("Receipts"),
                            customData: new MenuItemCustomData { Id = "ReceiptsNavbarItem" },
                            url: "app/orders/receipts",
                            icon: "fa-regular fa-receipt",
                            permissionDependency: new SimplePermissionDependency(AppPermissions.Pages_Reports_Receipts)
                        )
                    ).AddItem(new MenuItemDefinition(
                            AppPageNames.Tenant.PaymentReconciliation,
                            L("PaymentReconciliation"),
                            customData: new MenuItemCustomData { Id = "PrintPaymentReconciliationReportNavbarItem" },
                            url: "#",
                            icon: "fa-regular fa-print", //?
                            featureDependency: new SimpleFeatureDependency(AppFeatures.AllowPaymentProcessingFeature),
                            permissionDependency: new SimplePermissionDependency(AppPermissions.Pages_Reports_PaymentReconciliation)
                        )
                    ).AddItem(new MenuItemDefinition(
                            AppPageNames.Tenant.DriverActivityDetailReport,
                            L("DriverActivityReport"),
                            customData: new MenuItemCustomData { Id = "DriverActivityReportNavbarItem" },
                            url: "app/driveractivitydetailreport",
                            icon: "fa-regular fa-timeline-arrow",
                            permissionDependency: new SimplePermissionDependency(AppPermissions.Pages_Reports_DriverActivityDetail)
                        )
                    ).AddItem(
                        new MenuItemDefinition(
                            AppPageNames.Tenant.VehicleWorkOrderCost,
                            L("VehicleWorkOrderCost"),
                            customData: new MenuItemCustomData { Id = "VehicleWorkOrderCostItem" },
                            url: "app/VehicleWorkOrderCostReport",
                            icon: "fa-regular fa-car-wrench",
                            permissionDependency: new SimplePermissionDependency(AppPermissions.Pages_Reports_VehicleWorkOrderCost)
                        )
                    ).AddItem(
                        new MenuItemDefinition(
                            AppPageNames.Tenant.JobsMissingTickets,
                            L("JobsMissingTickets"),
                            customData: new MenuItemCustomData { Id = "JobsMissingTickets" },
                            url: "app/JobsMissingTicketsReport",
                            icon: "fa-regular fa-ticket-simple",
                            permissionDependency: new SimplePermissionDependency(AppPermissions.Pages_Reports_JobsMissingTickets)
                        )
                    )
                ).AddItem(new MenuItemDefinition(
                        AppPageNames.Tenant.Imports,
                        L("Imports"),
                        customData: new MenuItemCustomData { Id = "ImportsNavbarItem" },
                        icon: "fa-regular fa-file-import",
                        featureDependency: new SimpleFeatureDependency(AppFeatures.PaidFunctionality)
                    )
                    .AddItem(new MenuItemDefinition(
                            AppPageNames.Tenant.ImportFuelUsage,
                            L("ImportFuelMenuItem"),
                            customData: new MenuItemCustomData { Id = "ImportFuelNavbarItem" },
                            url: "app/ImportFuel",
                            icon: "fa-regular fa-gas-pump",
                            permissionDependency: new SimplePermissionDependency(AppPermissions.Pages_Imports_FuelUsage)
                        )
                    )
                ).AddItem(new MenuItemDefinition(
                        AppPageNames.Common.Administration,
                        L("Administration"),
                        customData: new MenuItemCustomData { Id = "AdministrationNavbarItem" },
                        icon: "fa-regular fa-wrench",
                        featureDependency: new SimpleFeatureDependency(AppFeatures.FreeFunctionality)
                    ).AddItem(new MenuItemDefinition(
                            AppPageNames.Tenant.Offices,
                            L("Offices"),
                            customData: new MenuItemCustomData { Id = "OfficesNavbarItem" },
                            url: "app/offices",
                            icon: "fa-regular fa-building",
                            permissionDependency: new SimplePermissionDependency(AppPermissions.Pages_Offices),
                            featureDependency: new SimpleFeatureDependency(AppFeatures.PaidFunctionality)
                        )
                    ).AddItem(new MenuItemDefinition(
                            AppPageNames.Tenant.CannedTexts,
                            L("CannedTexts"),
                            customData: new MenuItemCustomData { Id = "CannedTextsNavbarItem" },
                            url: "app/cannedtexts",
                            icon: "fa-regular fa-list-tree",
                            permissionDependency: new SimplePermissionDependency(AppPermissions.Pages_CannedText),
                            featureDependency: new SimpleFeatureDependency(AppFeatures.PaidFunctionality)
                        )
                    ).AddItem(new MenuItemDefinition(
                            AppPageNames.Common.OrganizationUnits,
                            L("OrganizationUnits"),
                            customData: new MenuItemCustomData { Id = "OrganizationUnitsNavbarItem" },
                            url: "App/OrganizationUnits",
                            icon: "fa-regular fa-sitemap",
                            permissionDependency: new SimplePermissionDependency(AppPermissions.Pages_Administration_OrganizationUnits),
                            featureDependency: new SimpleFeatureDependency(AppFeatures.PaidFunctionality)
                        )
                    ).AddItem(new MenuItemDefinition(
                            AppPageNames.Common.Roles,
                            L("Roles"),
                            customData: new MenuItemCustomData { Id = "RolesNavbarItem" },
                            url: "App/Roles",
                            icon: "fa-regular fa-id-card-clip",
                            permissionDependency: new SimplePermissionDependency(AppPermissions.Pages_Administration_Roles),
                            featureDependency: new SimpleFeatureDependency(AppFeatures.FreeFunctionality)
                        )
                    ).AddItem(new MenuItemDefinition(
                            AppPageNames.Common.Users,
                            L("Users"),
                            customData: new MenuItemCustomData { Id = "UsersNavbarItem" },
                            url: "App/Users",
                            icon: "fa-regular fa-user",
                            permissionDependency: new SimplePermissionDependency(AppPermissions.Pages_Administration_Users),
                            featureDependency: new SimpleFeatureDependency(AppFeatures.FreeFunctionality)
                        )
                    ).AddItem(new MenuItemDefinition(
                            AppPageNames.Common.Languages,
                            L("Languages"),
                            customData: new MenuItemCustomData { Id = "LanguagesNavbarItem" },
                            url: "App/Languages",
                            icon: "fa-regular fa-language",
                            permissionDependency: new SimplePermissionDependency(AppPermissions.Pages_Administration_Languages),
                            featureDependency: new SimpleFeatureDependency(AppFeatures.PaidFunctionality)
                        )
                    ).AddItem(new MenuItemDefinition(
                            AppPageNames.Common.AuditLogs,
                            L("AuditLogs"),
                            customData: new MenuItemCustomData { Id = "AuditLogsNavbarItem" },
                            url: "App/AuditLogs",
                            icon: "fa-regular fa-table",
                            permissionDependency: new SimplePermissionDependency(AppPermissions.Pages_Administration_AuditLogs),
                            featureDependency: new SimpleFeatureDependency(AppFeatures.FreeFunctionality)
                        )
                    ).AddItem(new MenuItemDefinition(
                            AppPageNames.Host.Maintenance,
                            L("Maintenance"),
                            customData: new MenuItemCustomData { Id = "MaintenanceNavbarItem" },
                            url: "App/Maintenance",
                            icon: "fa-regular fa-lock",
                            permissionDependency: new SimplePermissionDependency(AppPermissions.Pages_Administration_Host_Maintenance)
                        )
                    ).AddItem(new MenuItemDefinition(
                            AppPageNames.Tenant.SubscriptionManagement,
                            L("Subscription"),
                            customData: new MenuItemCustomData { Id = "SubscriptionManagementNavbarItem" },
                            url: "App/SubscriptionManagement",
                            icon: "fa-regular fa-repeat",
                            permissionDependency: new SimplePermissionDependency(AppPermissions.Pages_Administration_Tenant_SubscriptionManagement),
                            featureDependency: new SimpleFeatureDependency(AppFeatures.PaidFunctionality)
                        )
                    ).AddItem(new MenuItemDefinition(
                            AppPageNames.Host.Settings,
                            L("Settings"),
                            customData: new MenuItemCustomData { Id = "HostSettingsNavbarItem" },
                            url: "App/HostSettings",
                            icon: "fa-regular fa-gear",
                            permissionDependency: new SimplePermissionDependency(AppPermissions.Pages_Administration_Host_Settings)
                        )
                    )
                    .AddItem(new MenuItemDefinition(
                            AppPageNames.Tenant.Settings,
                            L("Settings"),
                            customData: new MenuItemCustomData { Id = "TenantSettingsNavbarItem" },
                            url: "App/Settings",
                            icon: "fa-regular fa-gear",
                            permissionDependency: new SimplePermissionDependency(AppPermissions.Pages_Administration_Tenant_Settings),
                            featureDependency: new SimpleFeatureDependency(AppFeatures.FreeFunctionality)
                        )
                    )
                ).AddItem(
                    new MenuItemDefinition(
                        AppPageNames.Tenant.Schedule,
                        L("Schedule"),
                        customData: new MenuItemCustomData { Id = "ScheduleNavbarItem" },
                        url: "app/scheduling",
                        icon: "fa-regular fa-calendar-range",
                        permissionDependency: new SimplePermissionDependency(AppPermissions.LeaseHaulerPortal_Schedule),
                        featureDependency: new SimpleFeatureDependency(AppFeatures.LeaseHaulerPortal)
                    )
                ).AddItem(
                    new MenuItemDefinition(
                        AppPageNames.Tenant.Tickets,
                        L("TicketsList"),
                        customData: new MenuItemCustomData { Id = "TicketsNavbarItem" },
                        url: "app/tickets",
                        icon: "fa-regular fa-ticket-simple",
                        permissionDependency: new SimplePermissionDependency(AppPermissions.LeaseHaulerPortal_Tickets),
                        featureDependency: new SimpleFeatureDependency(AppFeatures.LeaseHaulerPortal)
                    )
                ).AddItem(new MenuItemDefinition(
                        AppPageNames.Tenant.LeaseHaulerMyCompany,
                        L("MyCompany"),
                        customData: new MenuItemCustomData { Id = "MyCompanyNavbarItem" },
                        url: "app/MyCompany",
                        icon: "fa-regular fa-building",
                        permissionDependency: new SimplePermissionDependency(AppPermissions.LeaseHaulerPortal_MyCompany),
                        featureDependency: new SimpleFeatureDependency(AppFeatures.LeaseHaulerPortal)
                    )
                );
        }

        private static ILocalizableString L(string name)
        {
            return new LocalizableString(name, DispatcherWebConsts.LocalizationSourceName);
        }
    }
}
