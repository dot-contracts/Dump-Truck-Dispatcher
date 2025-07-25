using System;
using System.Linq;
using System.Threading.Tasks;
using Abp.Authorization;
using Abp.Dependency;
using Abp.Domain.Repositories;
using Abp.Domain.Uow;
using Abp.Runtime.Session;
using Abp.UI;
using DispatcherWeb.DriverApplication.Dto;
using DispatcherWeb.Drivers;
using DispatcherWeb.Infrastructure.Extensions;
using Microsoft.EntityFrameworkCore;

namespace DispatcherWeb.DriverApplication
{
    public class DriverApplicationAuthProvider : IDriverApplicationAuthProvider, ITransientDependency
    {
        private readonly IAbpSession _abpSession;
        private readonly IUnitOfWorkManager _unitOfWorkManager;
        private readonly IRepository<Driver> _driverRepository;

        public DriverApplicationAuthProvider(
            IAbpSession abpSession,
            IUnitOfWorkManager unitOfWorkManager,
            IRepository<Driver> driverRepository
            )
        {
            _abpSession = abpSession;
            _unitOfWorkManager = unitOfWorkManager;
            _driverRepository = driverRepository;
        }

        public async Task<AuthDriverByDriverGuidResult> AuthDriverByDriverGuidIfNeeded(Guid? driverGuid)
        {
            if (!_abpSession.UserId.HasValue && !driverGuid.HasValue)
            {
                throw new AbpAuthorizationException("Current user did not log in to the application!");
            }

            if (!_abpSession.UserId.HasValue)
            {
                var info = await AuthDriverByDriverGuid(driverGuid.Value);
                return info;
            }
            else
            {
                var driverId = await _driverRepository.GetDriverIdByUserIdOrThrow(_abpSession.UserId.Value);
                return new AuthDriverByDriverGuidResult
                {
                    DriverId = driverId,
                    TenantId = await _abpSession.GetTenantIdAsync(),
                    UserId = _abpSession.UserId.Value,
                };
            }
        }

        public async Task<AuthDriverByDriverGuidResult> AuthDriverByDriverGuid(Guid driverGuid)
        {
            var result = new AuthDriverByDriverGuidResult();
            using (_unitOfWorkManager.Current.DisableFilter(AbpDataFilters.MustHaveTenant, AbpDataFilters.MayHaveTenant))
            {
                var driver = await (await _driverRepository.GetQueryAsync())
                    .Where(x => x.Guid == driverGuid)
                    .Select(x => new
                    {
                        x.TenantId,
                        x.Id,
                        x.UserId,
                    }).FirstOrDefaultAsync();
                if (driver == null)
                {
                    throw new ApplicationException("No driver was found with guid " + driverGuid);
                }
                if (!driver.UserId.HasValue)
                {
                    throw new UserFriendlyException("The current driver isn't a user!");
                }
                result.TenantId = driver.TenantId;
                result.DriverId = driver.Id;
                result.UserId = driver.UserId.Value;
            }

            _unitOfWorkManager.Current.SetTenantId(result.TenantId);
            //this doesn't work outside of 'using' block, after leaving this function the values resets to null
            //_abpSession.Use(result.TenantId, result.UserId);
            return result;
        }
    }
}
