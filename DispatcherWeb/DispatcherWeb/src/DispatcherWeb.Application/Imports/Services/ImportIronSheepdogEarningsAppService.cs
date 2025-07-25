using System.Threading.Tasks;
using Abp.Authorization;
using Abp.Configuration;
using Abp.Domain.Repositories;
using DispatcherWeb.Authorization;
using DispatcherWeb.Authorization.Users;
using DispatcherWeb.Configuration;
using DispatcherWeb.Customers;
using DispatcherWeb.Drivers;
using DispatcherWeb.Imports.RowReaders;
using DispatcherWeb.Infrastructure.AzureBlobs;
using DispatcherWeb.Items;
using DispatcherWeb.LeaseHaulerRequests;
using DispatcherWeb.Locations;
using DispatcherWeb.LuckStone;
using DispatcherWeb.Orders;
using DispatcherWeb.TimeClassifications;
using DispatcherWeb.Trucks;
using DispatcherWeb.UnitsOfMeasure;

namespace DispatcherWeb.Imports.Services
{
    [AbpAuthorize(AppPermissions.Pages_Imports_Tickets_IronSheepdogEarnings)]
    public class ImportIronSheepdogEarningsAppService : ImportTicketEarningsBaseAppService, IImportIronSheepdogEarningsAppService
    {
        public ImportIronSheepdogEarningsAppService(
            IRepository<ImportedEarnings> importedEarningsRepository,
            IRepository<ImportedEarningsBatch> importedEarningsBatchRepository,
            IRepository<Order> orderRepository,
            IRepository<OrderLine> orderLineRepository,
            IRepository<OrderLineTruck> orderLineTruckRepository,
            IRepository<DriverAssignment> driverAssignmentRepository,
            IRepository<AvailableLeaseHaulerTruck> availableLeaseHaulerTruckRepository,
            IRepository<Ticket> ticketRepository,
            IRepository<Drivers.EmployeeTime> employeeTimeRepository,
            IRepository<Customer> customerRepository,
            IRepository<UnitOfMeasure> uomRepository,
            IRepository<Item> itemRepository,
            IRepository<Location> locationRepository,
            IRepository<LocationCategory> locationCategoryRepository,
            IRepository<Truck> truckRepository,
            IRepository<Driver> driverRepository,
            IRepository<TimeClassification> timeClassificationRepository,
            ISecureFileBlobService secureFileBlobService,
            UserManager userManager
        ) : base(
            importedEarningsRepository,
            importedEarningsBatchRepository,
            orderRepository,
            orderLineRepository,
            orderLineTruckRepository,
            driverAssignmentRepository,
            availableLeaseHaulerTruckRepository,
            ticketRepository,
            employeeTimeRepository,
            customerRepository,
            uomRepository,
            itemRepository,
            locationRepository,
            locationCategoryRepository,
            truckRepository,
            driverRepository,
            timeClassificationRepository,
            secureFileBlobService,
            userManager
        )
        {
        }

        protected override string GetExpectedCsvHeader()
        {
            return "Haultickets_TicketDateTime,Haultickets_Licenseplate,Haultickets_Site,Haultickets_ProductDescription,Haultickets_CustomerName,Haultickets_TicketID,Haultickets_HaulPaymentRate,Haultickets_HaulPaymentRateUOM,Haultickets_NetTons,Haultickets_FSCAmount,Haultickets_HaulPayment";
        }

        protected override TicketImportType TicketImportType => TicketImportType.IronSheepdog;

        protected override async Task<bool> GetProductionPayValue()
        {
            return await SettingManager.GetSettingValueAsync<bool>(AppSettings.IronSheepdog.UseForProductionPay);
        }

        protected override async Task<int> GetCustomerId()
        {
            return await SettingManager.GetSettingValueAsync<int>(AppSettings.IronSheepdog.IronSheepdogCustomerId);
        }

        protected override Task<string> GetHaulerRef()
        {
            return Task.FromResult(string.Empty);
        }

        protected override TicketImportTruckMatching TruckMatching => TicketImportTruckMatching.ByTruckCode;

        protected override bool AreRequiredFieldsFilled(TicketEarningsImportRow row)
        {
            return !string.IsNullOrEmpty(row.TicketNumber)
                && row.TicketDateTime.HasValue
                && !string.IsNullOrEmpty(row.Site)
                && !string.IsNullOrEmpty(row.CustomerName)
                && !string.IsNullOrEmpty(row.LicensePlate)
                && row.HaulPaymentRate.HasValue
                && row.NetTons.HasValue
                && row.HaulPayment.HasValue
                //&& !string.IsNullOrEmpty(row.HaulerRef)
                //&& row.FscAmount.HasValue
                && !string.IsNullOrEmpty(row.HaulPaymentRateUom)
                && !string.IsNullOrEmpty(row.ProductDescription);
        }
    }
}
