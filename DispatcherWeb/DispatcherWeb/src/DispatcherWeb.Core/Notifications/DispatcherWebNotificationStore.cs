using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Abp;
using Abp.Domain.Repositories;
using Abp.Domain.Uow;
using Abp.Linq.Extensions;
using Abp.Notifications;
using DispatcherWeb.Notifications.Cache;
using Microsoft.EntityFrameworkCore;

namespace DispatcherWeb.Notifications
{
    public class DispatcherWebNotificationStore : NotificationStore
    {
        private readonly IRepository<TenantNotificationInfo, Guid> _tenantNotificationRepository;
        private readonly IRepository<UserNotificationInfo, Guid> _userNotificationRepository;
        private readonly ITop3UserNotificationCache _top3UserNotificationCache;
        private readonly IPriorityUserNotificationCache _priorityUserNotificationCache;
        private readonly IUnitOfWorkManager _unitOfWorkManager;

        public DispatcherWebNotificationStore(
            IRepository<NotificationInfo, Guid> notificationRepository,
            IRepository<TenantNotificationInfo, Guid> tenantNotificationRepository,
            IRepository<UserNotificationInfo, Guid> userNotificationRepository,
            IRepository<NotificationSubscriptionInfo, Guid> notificationSubscriptionRepository,
            ITop3UserNotificationCache top3UserNotificationCache,
            IPriorityUserNotificationCache priorityUserNotificationCache,
            IUnitOfWorkManager unitOfWorkManager
            ) : base(notificationRepository, tenantNotificationRepository, userNotificationRepository, notificationSubscriptionRepository, unitOfWorkManager)
        {
            _tenantNotificationRepository = tenantNotificationRepository;
            _userNotificationRepository = userNotificationRepository;
            _top3UserNotificationCache = top3UserNotificationCache;
            _priorityUserNotificationCache = priorityUserNotificationCache;
            _unitOfWorkManager = unitOfWorkManager;
        }

        public override async Task<List<UserNotificationInfoWithNotificationInfo>> GetUserNotificationsWithNotificationsAsync(
                UserIdentifier user,
                UserNotificationState? state = null,
                int skipCount = 0,
                int maxResultCount = int.MaxValue,
                DateTime? startDate = null,
                DateTime? endDate = null)
        {
            return await GetUserNotificationsWithNotificationsAsync(user, state, skipCount, maxResultCount, startDate, endDate, null);
        }

        public async Task<List<UserNotificationInfoWithNotificationInfo>> GetUserNotificationsWithNotificationsAsync(
                UserIdentifier user,
                UserNotificationState? state = null,
                int skipCount = 0,
                int maxResultCount = int.MaxValue,
                DateTime? startDate = null,
                DateTime? endDate = null,
                string notificationName = null)
        {
            var result = await _unitOfWorkManager.WithUnitOfWorkAsync(async () =>
            {
                using (_unitOfWorkManager.Current.SetTenantId(user.TenantId))
                {
                    var query = from userNotificationInfo in await _userNotificationRepository.GetQueryAsync()
                                join tenantNotificationInfo in await _tenantNotificationRepository.GetQueryAsync()
                                    on userNotificationInfo.TenantNotificationId equals tenantNotificationInfo.Id
                                where userNotificationInfo.UserId == user.UserId
                                orderby userNotificationInfo.CreationTime descending
                                select new
                                {
                                    userNotificationInfo,
                                    tenantNotificationInfo,
                                };

                    if (state.HasValue)
                    {
                        query = query.Where(x => x.userNotificationInfo.State == state.Value);
                    }

                    if (startDate.HasValue)
                    {
                        query = query.Where(x => x.tenantNotificationInfo.CreationTime >= startDate);
                    }

                    if (endDate.HasValue)
                    {
                        query = query.Where(x => x.tenantNotificationInfo.CreationTime <= endDate);
                    }

                    if (!string.IsNullOrEmpty(notificationName))
                    {
                        query = query.Where(x => x.tenantNotificationInfo.NotificationName == notificationName);
                    }

                    query = query.PageBy(skipCount, maxResultCount);

                    var list = await query.ToListAsync();

                    return list.Select(
                        a => new UserNotificationInfoWithNotificationInfo(
                            a.userNotificationInfo,
                            a.tenantNotificationInfo
                        )
                    ).ToList();
                }
            });

            return await Task.FromResult(result);
        }

        private async Task InvalidateUserNotificationCachesAsync(UserIdentifier user)
        {
            await _top3UserNotificationCache.RemoveFromCacheAsync(user);
            await _priorityUserNotificationCache.RemoveFromCacheAsync(user);
        }

        public override async Task DeleteAllUserNotificationsAsync(UserIdentifier user, UserNotificationState? state = null, DateTime? startDate = null, DateTime? endDate = null)
        {
            await InvalidateUserNotificationCachesAsync(user);
            await base.DeleteAllUserNotificationsAsync(user, state, startDate, endDate);
        }

        public override async Task DeleteUserNotificationAsync(int? tenantId, Guid userNotificationId)
        {
            await _unitOfWorkManager.WithUnitOfWorkAsync(async () =>
            {
                using (_unitOfWorkManager.Current.SetTenantId(tenantId))
                {
                    var userNotification = await _userNotificationRepository.GetAsync(userNotificationId);
                    await InvalidateUserNotificationCachesAsync(new UserIdentifier(userNotification.TenantId, userNotification.UserId));
                }
            });
            await base.DeleteUserNotificationAsync(tenantId, userNotificationId);
        }

        public override async Task InsertUserNotificationAsync(UserNotificationInfo userNotification)
        {
            await InvalidateUserNotificationCachesAsync(new UserIdentifier(userNotification.TenantId, userNotification.UserId));
            await base.InsertUserNotificationAsync(userNotification);
        }

        public override async Task UpdateAllUserNotificationStatesAsync(UserIdentifier user, UserNotificationState state)
        {
            await InvalidateUserNotificationCachesAsync(user);
            await base.UpdateAllUserNotificationStatesAsync(user, state);
        }

        public override async Task UpdateUserNotificationStateAsync(int? tenantId, Guid userNotificationId, UserNotificationState state)
        {
            await _unitOfWorkManager.WithUnitOfWorkAsync(async () =>
            {
                using (_unitOfWorkManager.Current.SetTenantId(tenantId))
                {
                    var userNotification = await _userNotificationRepository.FirstOrDefaultAsync(userNotificationId);
                    if (userNotification != null)
                    {
                        await InvalidateUserNotificationCachesAsync(new UserIdentifier(userNotification.TenantId, userNotification.UserId));
                    }
                }
            });
            await base.UpdateUserNotificationStateAsync(tenantId, userNotificationId, state);
        }
    }
}
