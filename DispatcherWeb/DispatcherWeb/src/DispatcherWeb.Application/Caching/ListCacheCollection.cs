using Abp.Dependency;
using DispatcherWeb.Authorization.Users;
using DispatcherWeb.Customers;
using DispatcherWeb.Dispatching;
using DispatcherWeb.DriverAssignments;
using DispatcherWeb.Drivers;
using DispatcherWeb.FuelSurchargeCalculations;
using DispatcherWeb.Items;
using DispatcherWeb.LeaseHaulers;
using DispatcherWeb.Locations;
using DispatcherWeb.Offices;
using DispatcherWeb.Orders;
using DispatcherWeb.TaxRates;
using DispatcherWeb.Tickets;
using DispatcherWeb.Trucks;
using DispatcherWeb.UnitOfMeasures;
using DispatcherWeb.VehicleCategories;

namespace DispatcherWeb.Caching
{
    public class ListCacheCollection : ISingletonDependency
    {
        public ListCacheDateKeyLookupService DateKeyLookup { get; }
        public IOrderListCache Order { get; }
        public IOrderLineListCache OrderLine { get; }
        public IOrderLineTruckListCache OrderLineTruck { get; }
        public IDriverAssignmentListCache DriverAssignment { get; }
        public IDriverListCache Driver { get; }
        public ILeaseHaulerDriverListCache LeaseHaulerDriver { get; }
        public ITruckListCache Truck { get; }
        public ILeaseHaulerTruckListCache LeaseHaulerTruck { get; }
        public IAvailableLeaseHaulerTruckListCache AvailableLeaseHaulerTruck { get; }
        public ILeaseHaulerListCache LeaseHauler { get; }
        public IUserListCache User { get; }
        public ILeaseHaulerUserListCache LeaseHaulerUser { get; }
        public ICustomerListCache Customer { get; }
        public ICustomerContactListCache CustomerContact { get; }
        public ILocationListCache Location { get; }
        public IItemListCache Item { get; }
        public IVehicleCategoryListCache VehicleCategory { get; }
        public IUnitOfMeasureListCache UnitOfMeasure { get; }
        public IOrderLineVehicleCategoryListCache OrderLineVehicleCategory { get; }
        public ILeaseHaulerRequestListCache LeaseHaulerRequest { get; }
        public IInsuranceListCache Insurance { get; }
        public IRequestedLeaseHaulerTruckListCache RequestedLeaseHaulerTruck { get; }
        public IDispatchListCache Dispatch { get; }
        public ILoadListCache Load { get; }
        public ITicketListCache Ticket { get; }
        public IFuelSurchargeCalculationListCache FuelSurchargeCalculation { get; }
        public IOfficeListCache Office { get; }
        public ITaxRateListCache TaxRate { get; }

        public ListCacheCollection(
            ListCacheDateKeyLookupService dateKeyLookup,
            IOrderListCache order,
            IOrderLineListCache orderLine,
            IOrderLineTruckListCache orderLineTruck,
            IDriverAssignmentListCache driverAssignment,
            IDriverListCache driver,
            ILeaseHaulerDriverListCache leaseHaulerDriver,
            ITruckListCache truck,
            ILeaseHaulerTruckListCache leaseHaulerTruck,
            IAvailableLeaseHaulerTruckListCache availableLeaseHaulerTruck,
            ILeaseHaulerListCache leaseHauler,
            IInsuranceListCache insurance,
            IUserListCache user,
            ILeaseHaulerUserListCache leaseHaulerUser,
            ICustomerListCache customer,
            ICustomerContactListCache customerContact,
            ILocationListCache location,
            IItemListCache item,
            IVehicleCategoryListCache vehicleCategory,
            IUnitOfMeasureListCache unitOfMeasure,
            IOrderLineVehicleCategoryListCache orderLineVehicleCategory,
            ILeaseHaulerRequestListCache leaseHaulerRequest,
            IRequestedLeaseHaulerTruckListCache requestedLeaseHaulerTruck,
            IDispatchListCache dispatch,
            ILoadListCache load,
            ITicketListCache ticket,
            IFuelSurchargeCalculationListCache fuelSurchargeCalculation,
            IOfficeListCache office,
            ITaxRateListCache taxRate
        )
        {
            DateKeyLookup = dateKeyLookup;
            Order = order;
            OrderLine = orderLine;
            OrderLineTruck = orderLineTruck;
            DriverAssignment = driverAssignment;
            Driver = driver;
            LeaseHaulerDriver = leaseHaulerDriver;
            Truck = truck;
            LeaseHaulerTruck = leaseHaulerTruck;
            AvailableLeaseHaulerTruck = availableLeaseHaulerTruck;
            LeaseHauler = leaseHauler;
            Insurance = insurance;
            User = user;
            LeaseHaulerUser = leaseHaulerUser;
            Customer = customer;
            CustomerContact = customerContact;
            Location = location;
            Item = item;
            VehicleCategory = vehicleCategory;
            UnitOfMeasure = unitOfMeasure;
            OrderLineVehicleCategory = orderLineVehicleCategory;
            LeaseHaulerRequest = leaseHaulerRequest;
            RequestedLeaseHaulerTruck = requestedLeaseHaulerTruck;
            Dispatch = dispatch;
            Load = load;
            Ticket = ticket;
            FuelSurchargeCalculation = fuelSurchargeCalculation;
            Office = office;
            TaxRate = taxRate;
        }

        public IListCache[] All
        {
            get
            {
                return
                [
                    Order,
                    OrderLine,
                    OrderLineTruck,
                    DriverAssignment,
                    Driver,
                    LeaseHaulerDriver,
                    Truck,
                    LeaseHaulerTruck,
                    AvailableLeaseHaulerTruck,
                    LeaseHauler,
                    Insurance,
                    User,
                    LeaseHaulerUser,
                    Customer,
                    CustomerContact,
                    Location,
                    Item,
                    VehicleCategory,
                    UnitOfMeasure,
                    OrderLineVehicleCategory,
                    LeaseHaulerRequest,
                    RequestedLeaseHaulerTruck,
                    Dispatch,
                    Load,
                    Ticket,
                    FuelSurchargeCalculation,
                    Office,
                    TaxRate,
                ];
            }
        }

        public IListCache<ListCacheDateKey>[] AllDateCaches
        {
            get
            {
                return
                [
                    Order,
                    OrderLine,
                    OrderLineTruck,
                    DriverAssignment,
                    AvailableLeaseHaulerTruck,
                    OrderLineVehicleCategory,
                    LeaseHaulerRequest,
                    RequestedLeaseHaulerTruck,
                    Dispatch,
                    Load,
                    Ticket,
                ];
            }
        }
    }
}
