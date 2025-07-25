namespace DispatcherWeb.Authorization.Roles
{
    public static class StaticRoleNames
    {
        public static class Host
        {
            public const string Admin = "Admin";
        }

        public static class Tenants
        {
            public const string Admin = "Admin";
            public const string Dispatching = "Dispatching";
            public const string LimitedQuoting = "LimitedQuoting";
            public const string Quoting = "Quoting";
            public const string Backoffice = "Backoffice";
            public const string Administrative = "Administrative";
            public const string Managers = "Managers";
            public const string Maintenance = "Maintenance";
            public const string MaintenanceSupervisor = "MaintenanceSupervisor";
            public const string User = "User";
            public const string Driver = "Driver";
            public const string LeaseHaulerDriver = "LeaseHaulerDriver";
            public const string Customer = "Customer";
            public const string LeaseHaulerAdministrator = "LeaseHaulerAdministrator";
            public const string LeaseHaulerDispatcher = "LeaseHaulerDispatcher";
        }
    }
}
