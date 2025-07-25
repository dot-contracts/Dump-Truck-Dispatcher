using System;
using System.Transactions;
using Abp.Dependency;
using Abp.Domain.Uow;
using Abp.EntityFrameworkCore.Uow;
using Abp.MultiTenancy;
using DispatcherWeb.EntityFrameworkCore;
using DispatcherWeb.Migrations.Seed.Host;
using Microsoft.EntityFrameworkCore;

namespace DispatcherWeb.Migrations.Seed
{
    public static class SeedHelper
    {
        [Obsolete]
        public static void SeedHostDb(IIocResolver iocResolver)
        {
            WithDbContext<DispatcherWebDbContext>(iocResolver, SeedHostDb);
        }

        public static void SeedHostDb(DispatcherWebDbContext context)
        {
            context.SuppressAutoSetTenantId = true;

            //Host seed
            new InitialHostDbBuilder(context).Create();

            //commented out since the actual tenant creation logic (from the UI, not seed) is more complex than what is used below.
            //We can create a 'Default' tenant manually as a host admin

            //Default tenant seed (in host database).
            //new DefaultTenantBuilder(context).Create();
            //new TenantRoleAndUserBuilder(context, 1).Create();
            //
            //try
            //{
            //    //locations are created before migrations are applied, so the first run could throw an error, while the next run would work fine
            //    new DefaultLocationsCreator(context, 1).Create();
            //}
            //catch (Exception e)
            //{
            //    context.Logger.Error("Exception during DefaultSuppliersCreator: ", e);
            //}
            //new DefaultServiceCreator(context, 1).Create();
            //new DefaultUnitOfMeasureCreator(context, 1).Create();
            ////new DefaultJobCreator(context, 1).Create();
        }

        [Obsolete]
        private static void WithDbContext<TDbContext>(IIocResolver iocResolver, Action<TDbContext> contextAction)
            where TDbContext : DbContext
        {
            using (var uowManager = iocResolver.ResolveAsDisposable<IUnitOfWorkManager>())
            {
#pragma warning disable CS0618 // Type or member is obsolete - Begin has to be synchronous in this case
                using (var uow = uowManager.Object.Begin(TransactionScopeOption.Suppress))
#pragma warning restore CS0618 // Type or member is obsolete
                {
                    var context = uowManager.Object.Current.GetDbContext<TDbContext>(MultiTenancySides.Host);

                    contextAction(context);

                    uow.Complete();
                }
            }
        }
    }
}
