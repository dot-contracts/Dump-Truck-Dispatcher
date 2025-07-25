using System;
using Abp.Domain.Entities;
using DispatcherWeb.MultiTenancy;

namespace DispatcherWeb.DailyHistory
{
    public class TenantDailyHistory : Entity, IMustHaveTenant
    {
        public int TenantId { get; set; }

        public Tenant Tenant { get; set; }

        public DateTime Date { get; set; }

        public int ActiveTrucks { get; set; }

        public int ActiveUsers { get; set; }

        public int UsersWithActivity { get; set; }

        public int OrderLinesScheduled { get; set; }

        public int OrderLinesCreated { get; set; }

        public int ActiveCustomers { get; set; }

        public int InternalTrucksScheduled { get; set; }

        public int InternalScheduledDeliveries { get; set; }

        public int LeaseHaulerScheduledDeliveries { get; set; }

        public int TicketsCreated { get; set; }

        public int SmsSent { get; set; }

        public int InvoicesCreated { get; set; }

        public int PayStatementsCreated { get; set; }
    }
}
