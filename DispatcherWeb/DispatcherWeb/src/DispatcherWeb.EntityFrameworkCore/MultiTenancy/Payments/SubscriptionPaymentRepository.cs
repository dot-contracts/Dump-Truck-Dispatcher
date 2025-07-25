using System.Linq;
using System.Threading.Tasks;
using Abp.EntityFrameworkCore;
using Abp.Linq.Extensions;
using DispatcherWeb.EntityFrameworkCore;
using DispatcherWeb.EntityFrameworkCore.Repositories;
using Microsoft.EntityFrameworkCore;

namespace DispatcherWeb.MultiTenancy.Payments
{
    public class SubscriptionPaymentRepository : DispatcherWebRepositoryBase<SubscriptionPayment, long>, ISubscriptionPaymentRepository
    {
        public SubscriptionPaymentRepository(IDbContextProvider<DispatcherWebDbContext> dbContextProvider)
            : base(dbContextProvider)
        {
        }

        public async Task<SubscriptionPayment> GetByGatewayAndPaymentIdAsync(SubscriptionPaymentGatewayType gateway, string paymentId)
        {
            return await SingleAsync(p => p.ExternalPaymentId == paymentId && p.Gateway == gateway);
        }

        public async Task<IQueryable<SubscriptionPayment>> GetLastCompletedPaymentQueryAsync(int tenantId, SubscriptionPaymentGatewayType? gateway, bool? isRecurring)
        {
            return (await GetQueryAsync())
                .Where(p => p.TenantId == tenantId)
                .Where(p => p.Status == SubscriptionPaymentStatus.Completed)
                .WhereIf(gateway.HasValue, p => p.Gateway == gateway.Value)
                .WhereIf(isRecurring.HasValue, p => p.IsRecurring == isRecurring.Value)
                .OrderByDescending(x => x.Id);
        }

        public async Task<SubscriptionPayment> GetLastPaymentOrDefaultAsync(int tenantId, SubscriptionPaymentGatewayType? gateway, bool? isRecurring)
        {
            var query = await GetQueryAsync();
            return await query
                .Where(p => p.TenantId == tenantId)
                .WhereIf(gateway.HasValue, p => p.Gateway == gateway.Value)
                .WhereIf(isRecurring.HasValue, p => p.IsRecurring == isRecurring.Value)
                .OrderByDescending(x => x.Id)
                .FirstOrDefaultAsync();
        }
    }
}
