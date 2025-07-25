using System;
using System.Threading.Tasks;
using Abp.Dependency;
using Abp.Domain.Uow;
using Abp.EntityFrameworkCore;
using Abp.Extensions;
using Microsoft.EntityFrameworkCore;

namespace DispatcherWeb.EntityFrameworkCore
{
    public class DatabaseCheckHelper : ITransientDependency
    {
        private readonly IDbContextProvider<DispatcherWebDbContext> _dbContextProvider;
        private readonly IUnitOfWorkManager _unitOfWorkManager;

        public DatabaseCheckHelper(
            IDbContextProvider<DispatcherWebDbContext> dbContextProvider,
            IUnitOfWorkManager unitOfWorkManager
        )
        {
            _dbContextProvider = dbContextProvider;
            _unitOfWorkManager = unitOfWorkManager;
        }

        [Obsolete("Use ExistAsync instead.")]
        public bool Exist(string connectionString)
        {
            if (connectionString.IsNullOrEmpty())
            {
                //connectionString is null for unit tests
                return true;
            }

            try
            {
                using (var uow = _unitOfWorkManager.Begin(new UnitOfWorkOptions { IsTransactional = false }))
                {
                    // Switching to host is necessary for single tenant mode.
                    using (_unitOfWorkManager.Current.SetTenantId(null))
                    {
                        _dbContextProvider.GetDbContext().Database.OpenConnection();
                        uow.Complete();
                    }
                }
            }
            catch
            {
                return false;
            }

            return true;
        }

        public async Task<bool> ExistAsync(string connectionString)
        {
            if (connectionString.IsNullOrEmpty())
            {
                //connectionString is null for unit tests
                return true;
            }

            try
            {
                await _unitOfWorkManager.WithUnitOfWorkAsync(new UnitOfWorkOptions { IsTransactional = false }, async () =>
                {
                    using (_unitOfWorkManager.Current.SetTenantId(null))
                    {
                        var dbContext = await _dbContextProvider.GetDbContextAsync();
                        await dbContext.Database.OpenConnectionAsync();
                    }
                });
            }
            catch
            {
                return false;
            }

            return true;
        }
    }
}
