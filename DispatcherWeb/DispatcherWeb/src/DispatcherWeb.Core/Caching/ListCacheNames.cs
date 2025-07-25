namespace DispatcherWeb.Caching
{
    public static class ListCacheNames
    {
        private const string ListCacheSuffix = "ListCache";
        public const string Order = "Order" + ListCacheSuffix;
        public const string OrderLine = "OrderLine" + ListCacheSuffix;
        public const string OrderLineTruck = "OrderLineTruck" + ListCacheSuffix;
        public const string DriverAssignment = "DriverAssignment" + ListCacheSuffix;
        public const string Driver = "Driver" + ListCacheSuffix;
        public const string LeaseHaulerDriver = "LeaseHaulerDriver" + ListCacheSuffix;
        public const string Truck = "Truck" + ListCacheSuffix;
        public const string LeaseHaulerTruck = "LeaseHaulerTruck" + ListCacheSuffix;
        public const string AvailableLeaseHaulerTruck = "AvailableLeaseHaulerTruck" + ListCacheSuffix;
        public const string LeaseHauler = "LeaseHauler" + ListCacheSuffix;
        public const string User = "User" + ListCacheSuffix;
        public const string LeaseHaulerUser = "LeaseHaulerUser" + ListCacheSuffix;
        public const string Customer = "Customer" + ListCacheSuffix;
        public const string CustomerContact = "CustomerContact" + ListCacheSuffix;
        public const string Location = "Location" + ListCacheSuffix;
        public const string Item = "Item" + ListCacheSuffix;
        public const string VehicleCategory = "VehicleCategory" + ListCacheSuffix;
        public const string UnitOfMeasure = "UnitOfMeasure" + ListCacheSuffix;
        public const string OrderLineVehicleCategory = "OrderLineVehicleCategory" + ListCacheSuffix;
        public const string LeaseHaulerRequest = "LeaseHaulerRequest" + ListCacheSuffix;
        public const string Insurance = "Insurance" + ListCacheSuffix;
        public const string RequestedLeaseHaulerTruck = "RequestedLeaseHaulerTruck" + ListCacheSuffix;
        public const string Dispatch = "Dispatch" + ListCacheSuffix;
        public const string Load = "Load" + ListCacheSuffix;
        public const string Ticket = "Ticket" + ListCacheSuffix;
        public const string FuelSurchargeCalculation = "FuelSurchargeCalculation" + ListCacheSuffix;
        public const string Office = "Office" + ListCacheSuffix;
        public const string TaxRate = "TaxRate" + ListCacheSuffix;

        private const string EntityListCacheSuffix = "EntityListCache";
        public const string UserEntity = "User" + EntityListCacheSuffix;
        public const string UserLoginEntity = "UserLogin" + EntityListCacheSuffix;
        public const string RoleEntity = "Role" + EntityListCacheSuffix;
        public const string UserRoleEntity = "UserRole" + EntityListCacheSuffix;
        public const string UserOrganizationUnitEntity = "UserOrganizationUnit" + EntityListCacheSuffix;
        public const string OrganizationUnitRoleEntity = "OrganizationUnitRole" + EntityListCacheSuffix;

        public static string[] All =
        [
            // List caches:
            Order,
            OrderLine,
            OrderLineTruck,
            DriverAssignment,
            Driver,
            LeaseHaulerDriver,
            Truck,
            LeaseHaulerTruck,
            Insurance,
            AvailableLeaseHaulerTruck,
            LeaseHauler,
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

            // Entity list caches:
            UserEntity,
            UserLoginEntity,
            RoleEntity,
            UserRoleEntity,
            UserOrganizationUnitEntity,
            OrganizationUnitRoleEntity,
        ];
    }
}
