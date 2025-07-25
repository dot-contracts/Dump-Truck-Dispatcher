using System.Linq;
using System.Threading.Tasks;
using Abp.Domain.Repositories;

namespace DispatcherWeb.MultiTenancy.Payments
{
    public interface ISubscriptionPaymentRepository : IRepository<SubscriptionPayment, long>
    {
        Task<SubscriptionPayment> GetByGatewayAndPaymentIdAsync(SubscriptionPaymentGatewayType gateway, string paymentId);

        Task<IQueryable<SubscriptionPayment>> GetLastCompletedPaymentQueryAsync(int tenantId, SubscriptionPaymentGatewayType? gateway,
            bool? isRecurring);

        Task<SubscriptionPayment> GetLastPaymentOrDefaultAsync(int tenantId, SubscriptionPaymentGatewayType? gateway, bool? isRecurring);
    }
}
