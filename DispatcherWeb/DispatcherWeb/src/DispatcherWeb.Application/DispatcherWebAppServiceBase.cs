using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Abp.Application.Services;
using Abp.Authorization;
using Abp.Collections.Extensions;
using Abp.Configuration;
using Abp.Domain.Repositories;
using Abp.IdentityFramework;
using Abp.Localization;
using Abp.Runtime.Session;
using Abp.Threading;
using Abp.Timing;
using DispatcherWeb.Authorization.Roles;
using DispatcherWeb.Authorization.Users;
using DispatcherWeb.Authorization.Users.Cache;
using DispatcherWeb.Configuration;
using DispatcherWeb.Dispatching;
using DispatcherWeb.Exceptions;
using DispatcherWeb.Localization;
using DispatcherWeb.MultiTenancy;
using DispatcherWeb.Orders;
using DispatcherWeb.Runtime.Session;
using DispatcherWeb.Sessions;
using DispatcherWeb.Trucks;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using LocalizationHelper = DispatcherWeb.Localization.LocalizationHelper;

namespace DispatcherWeb
{
    /// <summary>
    /// Derive your application services from this class.
    /// </summary>
    public abstract class DispatcherWebAppServiceBase : ApplicationService, ILocalizationHelperProvider
    {
        public TenantManager TenantManager { get; set; }

        public UserManager UserManager { get; set; }

        public IExtendedAbpSession Session { get; set; }

        public ICancellationTokenProvider CancellationTokenProvider { get; set; }

        public LocalizationHelper LocalizationHelper { get; set; }

        public IUserOrganizationUnitCache UserOrganizationUnitCache { get; set; }

        protected DispatcherWebAppServiceBase()
        {
            LocalizationSourceName = DispatcherWebConsts.LocalizationSourceName;
            CancellationTokenProvider = NullCancellationTokenProvider.Instance;
        }

        protected virtual async Task<User> GetCurrentUserAsync()
        {
            var user = await UserManager.FindByIdAsync(AbpSession.GetUserId().ToString());
            if (user == null)
            {
                throw new Exception("There is no current user!");
            }

            return user;
        }

        protected virtual string L(ILocalizableString ls)
        {
            return ls is LocalizableString lsInstance
                ? L(lsInstance.Name)
                : ls?.ToString();
        }

        protected virtual async Task<Tenant> GetCurrentTenantAsync()
        {
            using (CurrentUnitOfWork.SetTenantId(null))
            {
                return await TenantManager.GetByIdAsync(await AbpSession.GetTenantIdAsync());
            }
        }

        protected async Task<string> GetTimezone()
        {
            try
            {
                if (SettingManager != null)
                {
                    return await SettingManager.GetSettingValueAsync(TimingSettingNames.TimeZone);
                }
                else
                {
                    // Return a default timezone if SettingManager is not available
                    return "UTC";
                }
            }
            catch (Exception)
            {
                // Return a default timezone if there's an error
                return "UTC";
            }
        }

        protected async Task<string> GetTimezone(int? tenantId, long userId)
        {
            try
            {
                if (SettingManager != null)
                {
                    return await SettingManager.GetSettingValueForUserAsync(TimingSettingNames.TimeZone, tenantId, userId);
                }
                else
                {
                    // Return a default timezone if SettingManager is not available
                    return "UTC";
                }
            }
            catch (Exception)
            {
                // Return a default timezone if there's an error
                return "UTC";
            }
        }

        protected async Task<DateTime> GetToday()
        {
            var timeZone = await GetTimezone();
            return TimeExtensions.GetToday(timeZone);
        }

        protected virtual void CheckErrors(IdentityResult identityResult)
        {
            identityResult.CheckErrors(LocalizationManager);
        }

        protected int OfficeId => Session.GetOfficeIdOrThrow();

        protected async Task<List<long?>> GetOrganizationUnitIds()
        {
            var organizationUnits = await UserOrganizationUnitCache.GetUserOrganizationUnitsAsync(Session.GetUserId());
            return organizationUnits.Select(x => (long?)x.OrganizationUnitId).ToList();
        }

        protected async Task<List<int?>> GetOfficeIds()
        {
            var organizationUnits = await UserOrganizationUnitCache.GetUserOrganizationUnitsAsync(Session.GetUserId());
            return organizationUnits.Where(x => x.OfficeId.HasValue).Select(x => x.OfficeId).ToList();
        }

        protected virtual async Task<IQueryable<User>> GetCurrentUserQueryAsync()
        {
            return (await UserManager.GetQueryAsync()).Where(x => x.Id == Session.UserId);
        }

        protected async Task SaveOrThrowConcurrencyErrorAsync()
        {
            try
            {
                await CurrentUnitOfWork.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                throw new ConcurrencyException();
            }
        }

        protected async Task CheckUseShiftSettingCorrespondsInput(Shift? shift)
        {
            try
            {
                bool useShifts = false;
                if (SettingManager != null)
                {
                    useShifts = await SettingManager.GetSettingValueAsync<bool>(AppSettings.General.UseShifts);
                }

                if (useShifts)
                {
                    if (!shift.HasValue)
                    {
                        throw new ArgumentException("UseShifts is turned on but there are no shifts in the input.");
                    }
                }
                else
                {
                    if (shift.HasValue)
                    {
                        throw new ArgumentException("UseShifts is turned off but there are shifts in the input.");
                    }
                }
            }
            catch (Exception)
            {
                // If SettingManager is not available, assume shifts are not used
                if (shift.HasValue)
                {
                    throw new ArgumentException("UseShifts is turned off but there are shifts in the input.");
                }
            }
        }

        protected async Task CheckUseShiftSettingCorrespondsInput(Shift[] shifts)
        {
            try
            {
                bool useShifts = false;
                if (SettingManager != null)
                {
                    useShifts = await SettingManager.GetSettingValueAsync<bool>(AppSettings.General.UseShifts);
                }

                if (useShifts)
                {
                    if (shifts.IsNullOrEmpty())
                    {
                        throw new ArgumentException("UseShifts is turned on but there are no shifts in the input.");
                    }
                }
                else
                {
                    if (!shifts.IsNullOrEmpty())
                    {
                        throw new ArgumentException("UseShifts is turned off but there are shifts in the input.");
                    }
                }
            }
            catch (Exception)
            {
                // If SettingManager is not available, assume shifts are not used
                if (!shifts.IsNullOrEmpty())
                {
                    throw new ArgumentException("UseShifts is turned off but there are shifts in the input.");
                }
            }
        }

        protected async Task<bool> CanEditAnyOrderDirectionsAsync()
        {
            var user = await UserManager.FindByIdAsync(AbpSession.GetUserId().ToString());
            if (user == null)
            {
                return false;
            }
            var isDispatcher = await UserManager.IsInRoleAsync(user, StaticRoleNames.Tenants.Dispatching);
            return isDispatcher;
        }

        protected async Task CheckCustomerSpecificPermissions(string anyCustomerPermissionName, string specificCustomerPermissionName, params int?[] entityCustomerIds)
        {
            var permissions = new
            {
                ViewAnyCustomerEntities = await IsGrantedAsync(anyCustomerPermissionName),
                ViewCustomerSpecificEntitiesOnly = await IsGrantedAsync(specificCustomerPermissionName),
            };

            if (permissions.ViewAnyCustomerEntities)
            {
                //do not additionally restrict the data
            }
            else if (permissions.ViewCustomerSpecificEntitiesOnly)
            {
                var customerId = Session.GetCustomerIdOrThrow(this);
                if (!entityCustomerIds.Any() || entityCustomerIds.Any(entityCustomerId => entityCustomerId != customerId))
                {
                    throw new AbpAuthorizationException();
                }
            }
            else
            {
                throw new AbpAuthorizationException();
            }
        }

        protected async Task CheckEntitySpecificPermissions(
            string anyEntityPermissionName,
            string specificEntityPermissionName,
            int? sessionEntityId,
            params int?[] entityIds)
        {
            await CheckEntitySpecificPermissions(
                anyEntityPermissionName,
                specificEntityPermissionName,
                sessionEntityId,
                entityIds,
                () => throw new AbpAuthorizationException()
            );
        }

        protected async Task CheckEntitySpecificPermissions(
            string anyEntityPermissionName,
            string specificEntityPermissionName,
            int? sessionEntityId,
            int?[] entityIds,
            Func<Task> asyncFallbackAction)
        {
            var permissions = new
            {
                ViewAnyEntities = !string.IsNullOrEmpty(anyEntityPermissionName) && await IsGrantedAsync(anyEntityPermissionName),
                ViewSpecificEntitiesOnly = await IsGrantedAsync(specificEntityPermissionName),
            };

            if (permissions.ViewAnyEntities)
            {
                // do not additionally restrict the data
            }
            else if (permissions.ViewSpecificEntitiesOnly)
            {
                if (!sessionEntityId.HasValue)
                {
                    throw new AbpAuthorizationException();
                }

                if (entityIds == null || !entityIds.Any() || entityIds.Any(entityId => entityId != sessionEntityId))
                {
                    await asyncFallbackAction();
                }
            }
            else
            {
                await asyncFallbackAction();
            }
        }

        protected async Task<int?> GetLeaseHaulerIdFilterAsync(string allRecordsPermission, string lhRecordsPermission)
        {
            if (await IsGrantedAsync(allRecordsPermission))
            {
                return null;
            }
            if (await IsGrantedAsync(lhRecordsPermission))
            {
                return Session.GetLeaseHaulerIdOrThrow(this);
            }
            throw new AbpAuthorizationException($"Neither {allRecordsPermission} nor {lhRecordsPermission} permission is granted");
        }

        protected async Task CheckOrderLineEditPermissions(string allRecordsPermission, string leaseHaulerRecordsPermission,
            IRepository<OrderLine> orderLineRepository, int orderLineId)
        {
            if (await IsGrantedAsync(allRecordsPermission))
            {
                return;
            }
            else if (await IsGrantedAsync(leaseHaulerRecordsPermission))
            {
                await CheckLeaseHaulerEditOrderLinePermission(orderLineRepository, orderLineId);
            }
            else
            {
                throw new AbpAuthorizationException($"Neither {allRecordsPermission} nor {leaseHaulerRecordsPermission} permission is granted");
            }
        }

        protected async Task CheckOrderEditPermissions(string allRecordsPermission, string leaseHaulerRecordsPermission,
            IRepository<OrderLine> orderLineRepository, int? orderId)
        {
            if (await IsGrantedAsync(allRecordsPermission))
            {
                return;
            }
            else if (await IsGrantedAsync(leaseHaulerRecordsPermission))
            {
                await CheckLeaseHaulerEditOrderPermission(orderLineRepository, orderId);
            }
            else
            {
                throw new AbpAuthorizationException($"Neither {allRecordsPermission} nor {leaseHaulerRecordsPermission} permission is granted");
            }
        }

        /// <summary>
        /// This method assumes that "edit all records" permission has already been manually checked and failed, so it only performs the remaining "LH records" permission check
        /// </summary>
        /// <exception cref="AbpAuthorizationException"></exception>
        protected async Task CheckLeaseHaulerEditOrderLinePermission(IRepository<OrderLine> orderLineRepository, int? orderLineId)
        {
            if (!orderLineId.HasValue || !Session.LeaseHaulerId.HasValue)
            {
                throw new AbpAuthorizationException();
            }

            var query = (await orderLineRepository.GetQueryAsync()).Where(ol => ol.Id == orderLineId);

            if (!await IsOrderLineAssignedToLeaseHauler(query, Session.LeaseHaulerId.Value))
            {
                throw new AbpAuthorizationException();
            }
        }

        /// <summary>
        /// This method assumes that "edit all records" permission has already been manually checked and failed, so it only performs the remaining "LH records" permission check
        /// </summary>
        /// <exception cref="AbpAuthorizationException"></exception>
        protected async Task CheckLeaseHaulerEditOrderPermission(IRepository<OrderLine> orderLineRepository, int? orderId)
        {
            if (!orderId.HasValue || !Session.LeaseHaulerId.HasValue)
            {
                throw new AbpAuthorizationException();
            }

            var query = (await orderLineRepository.GetQueryAsync()).Where(ol => ol.OrderId == orderId);

            if (!await IsOrderLineAssignedToLeaseHauler(query, Session.LeaseHaulerId.Value))
            {
                throw new AbpAuthorizationException();
            }
        }

        private static async Task<bool> IsOrderLineAssignedToLeaseHauler(IQueryable<OrderLine> orderLineQuery, int leaseHaulerId)
        {
            // this uses Any instead of All because it is expected that either a query returning a single (or none) OrderLines will be passed,
            // or this will be used to see if an entire order can be edited, in which case we have to check that at least one OrderLine is assigned to the LeaseHauler
            // so usage of Any is warranted in this case, unless we want to use this to check multiple different OrderLines at once in the future,
            // in which case we'll need a different method
            return await orderLineQuery
                .AnyAsync(ol =>
                    ol.OrderLineTrucks.Any(olt => olt.Truck.LeaseHaulerTruck.LeaseHaulerId == leaseHaulerId)
                    || ol.LeaseHaulerRequests.Any(lhr => lhr.LeaseHaulerId == leaseHaulerId)
                );
        }

        protected async Task CheckDispatchEditPermissions(string allDispatchesPermission, string leaseHaulerDispatchesPermission,
            IRepository<Dispatch> dispatchRepository, params int[] dispatchIds)
        {
            if (!dispatchIds.Any())
            {
                return;
            }
            if (await IsGrantedAsync(allDispatchesPermission))
            {
                return;
            }
            else if (await IsGrantedAsync(leaseHaulerDispatchesPermission))
            {
                var dispatchData = await (await dispatchRepository.GetQueryAsync())
                    .Where(d => dispatchIds.Contains(d.Id))
                    .Select(d => new
                    {
                        LeaseHaulerId = (int?)d.Truck.LeaseHaulerTruck.LeaseHaulerId,
                    })
                    .ToListAsync();

                await CheckEntitySpecificPermissions(
                    allDispatchesPermission,
                    leaseHaulerDispatchesPermission,
                    Session.GetLeaseHaulerIdOrThrow(this),
                    dispatchData.Select(d => d.LeaseHaulerId).ToArray()
                );
            }
            else
            {
                throw new AbpAuthorizationException($"Neither {allDispatchesPermission} nor {leaseHaulerDispatchesPermission} is granted");
            }
        }

        protected async Task CheckTruckEditPermissions(string allTrucksPermission, string leaseHaulerTrucksPermission,
            IRepository<Truck> truckRepository, params int[] truckIds)
        {
            if (!truckIds.Any())
            {
                return;
            }
            if (await IsGrantedAsync(allTrucksPermission))
            {
                return;
            }
            else if (await IsGrantedAsync(leaseHaulerTrucksPermission))
            {
                var truckData = await (await truckRepository.GetQueryAsync())
                    .Where(t => truckIds.Contains(t.Id))
                    .Select(t => new
                    {
                        LeaseHaulerId = (int?)t.LeaseHaulerTruck.LeaseHaulerId,
                    })
                    .ToListAsync();

                await CheckEntitySpecificPermissions(
                    allTrucksPermission,
                    leaseHaulerTrucksPermission,
                    Session.GetLeaseHaulerIdOrThrow(this),
                    truckData.Select(t => t.LeaseHaulerId).ToArray()
                );
            }
            else
            {
                throw new AbpAuthorizationException($"Neither {allTrucksPermission} nor {leaseHaulerTrucksPermission} is granted");
            }
        }
    }
}
