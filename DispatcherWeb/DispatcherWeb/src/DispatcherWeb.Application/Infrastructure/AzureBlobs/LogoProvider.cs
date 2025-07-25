using System;
using System.Linq;
using System.Threading.Tasks;
using Abp.Domain.Repositories;
using Abp.Runtime.Session;
using DispatcherWeb.Infrastructure.Extensions;
using DispatcherWeb.MultiTenancy;
using DispatcherWeb.Offices;
using DispatcherWeb.Storage;
using Microsoft.EntityFrameworkCore;

namespace DispatcherWeb.Infrastructure.AzureBlobs
{
    public class LogoProvider : DispatcherWebDomainServiceBase, ILogoProvider
    {
        private readonly IBinaryObjectManager _binaryObjectManager;
        private readonly IRepository<Office> _officeRepository;

        public TenantManager TenantManager { get; }

        public LogoProvider(
            TenantManager tenantManager,
            IBinaryObjectManager binaryObjectManager,
            IRepository<Office> officeRepository
            )
        {
            TenantManager = tenantManager;
            _binaryObjectManager = binaryObjectManager;
            _officeRepository = officeRepository;
        }

        public async Task<byte[]> GetReportLogoAsBytesAsync(int? officeId)
        {
            Guid? reportsLogoId = null;

            if (officeId != null)
            {
                var office = await (await _officeRepository.GetQueryAsync())
                    .Where(x => x.Id == officeId)
                    .Select(x => new
                    {
                        x.ReportsLogoId,
                    })
                    .FirstAsync();

                reportsLogoId = office.ReportsLogoId;
            }

            if (reportsLogoId == null)
            {
                var tenantId = await Session.GetTenantIdAsync();
                var tenant = await (await TenantManager.GetQueryAsync())
                    .Where(x => x.Id == tenantId)
                    .Select(x => new
                    {
                        x.ReportsLogoId,
                    })
                    .FirstAsync();

                reportsLogoId = tenant.ReportsLogoId;
            }

            return await _binaryObjectManager.GetImageAsBytesAsync(reportsLogoId);
        }
    }
}
