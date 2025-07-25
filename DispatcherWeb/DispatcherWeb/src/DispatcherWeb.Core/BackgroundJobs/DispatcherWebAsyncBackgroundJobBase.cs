using System;
using System.Threading.Tasks;
using Abp;
using Abp.BackgroundJobs;
using DispatcherWeb.Runtime.Session;

namespace DispatcherWeb.BackgroundJobs
{
    public abstract class DispatcherWebAsyncBackgroundJobBase<TArg> : AsyncBackgroundJob<TArg>
    {
        public DispatcherWebAsyncBackgroundJobBase(
            IExtendedAbpSession session
        )
        {
            Session = session;
        }

        public IExtendedAbpSession Session { get; }

        protected async Task<T> WithUnitOfWorkAsync<T>(UserIdentifier requestorUser, Func<Task<T>> action)
        {
            return await UnitOfWorkManager.WithUnitOfWorkAsync(async () =>
            {
                using (CurrentUnitOfWork.SetTenantId(requestorUser.TenantId))
                using (Session.Use(requestorUser.TenantId, requestorUser.UserId))
                {
                    return await action();
                }
            });

        }

        protected async Task WithUnitOfWorkAsync(UserIdentifier requestorUser, Func<Task> action)
        {
            await UnitOfWorkManager.WithUnitOfWorkAsync(async () =>
            {
                using (CurrentUnitOfWork.SetTenantId(requestorUser?.TenantId))
                using (Session.Use(requestorUser?.TenantId, requestorUser?.UserId))
                {
                    await action();
                }
            });

        }
    }
}
