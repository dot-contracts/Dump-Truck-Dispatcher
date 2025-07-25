using System;
using Abp;
using DispatcherWeb.Caching;

namespace DispatcherWeb.Authorization.Users
{
    public class UserCacheItem : AuditableCacheItem<long>
    {
        public const string CacheName = "UserCache";
        public int? TenantId { get; set; }
        public string UserName { get; set; }
        public string Email { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public Guid? ProfilePictureId { get; set; }

        public UserIdentifier ToUserIdentifier()
        {
            return new UserIdentifier(TenantId, Id);
        }

        public string ToUserIdentifierString()
        {
            return ToUserIdentifier().ToUserIdentifierString();
        }
    }
}
