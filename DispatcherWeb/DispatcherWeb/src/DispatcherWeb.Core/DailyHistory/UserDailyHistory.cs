using System;
using Abp.Domain.Entities;
using DispatcherWeb.Authorization.Users;

namespace DispatcherWeb.DailyHistory
{
    public class UserDailyHistory : Entity, ISoftDelete, IMayHaveTenant
    {
        public bool IsDeleted { get; set; }

        public long UserId { get; set; }
        public User User { get; set; }

        public int? TenantId { get; set; }

        public DateTime Date { get; set; }

        public int NumberOfTransactions { get; set; }

    }
}
