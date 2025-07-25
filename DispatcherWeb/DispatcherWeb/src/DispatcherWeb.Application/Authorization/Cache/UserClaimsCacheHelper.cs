using System.Linq;
using System.Threading.Tasks;
using Abp.Dependency;
using Abp.Domain.Uow;
using DispatcherWeb.Authorization.Cache.Dto;
using DispatcherWeb.Authorization.Users;
using DispatcherWeb.Caching;
using DispatcherWeb.Runtime.Session;

namespace DispatcherWeb.Authorization.Cache
{
    public class UserClaimsCacheHelper : IUserClaimsCacheHelper, ISingletonDependency
    {
        private readonly ListCacheCollection _listCaches;
        private readonly EntityListCacheCollection _entityListCaches;
        private readonly IUnitOfWorkManager _unitOfWorkManager;
        private readonly IExtendedAbpSession _session;

        public UserClaimsCacheHelper(
            ListCacheCollection listCaches,
            EntityListCacheCollection entityListCaches,
            IUnitOfWorkManager unitOfWorkManager,
            IExtendedAbpSession session
        )
        {
            _listCaches = listCaches;
            _entityListCaches = entityListCaches;
            _unitOfWorkManager = unitOfWorkManager;
            _session = session;
        }

        protected async Task<ListCacheTenantKey> GetCacheKeyAsync()
        {
            return new ListCacheTenantKey(await GetTenantIdAsync() ?? 0);
        }

        protected async Task<int?> GetTenantIdAsync()
        {
            if (_unitOfWorkManager.Current != null)
            {
                return _unitOfWorkManager.Current.GetTenantId();
            }

            return await _session.GetTenantIdOrNullAsync();
        }

        public async Task<UserClaimsCacheItem> GetUserClaimsAsync(long userId)
        {
            var key = await GetCacheKeyAsync();
            var users = await _entityListCaches.User.GetList(key);
            var user = users.Find(userId);
            if (user == null || user.TenantId == null)
            {
                return new UserClaimsCacheItem();
            }

            var result = new UserClaimsCacheItem();

            await FillOfficeIfNeeded(result, user, key);
            await FillCustomerContactIfNeeded(result, user, key);
            await FillLeaseHaulerIfNeeded(result, user, key);

            return result;
        }

        private async Task FillOfficeIfNeeded(UserClaimsCacheItem result, User user, ListCacheTenantKey key)
        {
            if (user.OfficeId == null)
            {
                return;
            }
            var offices = await _listCaches.Office.GetList(key);
            var office = offices.Find(user.OfficeId);

            result.OfficeName = office?.Name;
            result.OfficeCopyChargeTo = office?.CopyDeliverToLoadAtChargeTo;
        }

        private async Task FillCustomerContactIfNeeded(UserClaimsCacheItem result, User user, ListCacheTenantKey key)
        {
            if (user.CustomerContactId == null)
            {
                return;
            }

            var customerContacts = await _listCaches.CustomerContact.GetList(key);
            var customerContact = customerContacts.Find(user.CustomerContactId);
            if (customerContact == null)
            {
                return;
            }

            var customers = await _listCaches.Customer.GetList(key);
            var customer = customers.Find(customerContact.CustomerId);
            if (customer == null)
            {
                return;
            }

            result.CustomerId = customer.Id;
            result.CustomerName = customer.Name;
            result.CustomerPortalAccessEnabled = customerContact.HasCustomerPortalAccess;
        }

        private async Task FillLeaseHaulerIfNeeded(UserClaimsCacheItem result, User user, ListCacheTenantKey key)
        {
            var leaseHaulerUsers = await _listCaches.LeaseHaulerUser.GetList(key);
            var leaseHaulerUser = leaseHaulerUsers.Items.FirstOrDefault(x => x.UserId == user.Id);
            if (leaseHaulerUser == null)
            {
                return;
            }

            result.LeaseHaulerId = leaseHaulerUser.LeaseHaulerId;
        }
    }
}
