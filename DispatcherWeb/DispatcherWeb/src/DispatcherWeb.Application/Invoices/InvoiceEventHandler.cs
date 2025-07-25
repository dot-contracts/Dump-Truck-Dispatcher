using System.Threading.Tasks;
using Abp.Dependency;
using Abp.Domain.Uow;
using Abp.Events.Bus.Entities;
using Abp.Events.Bus.Handlers;

namespace DispatcherWeb.Invoices
{
    public class InvoiceEventHandler : IAsyncEventHandler<EntityDeletingEventData<InvoiceLine>>, ITransientDependency
    {
        private readonly IUnitOfWorkManager _unitOfWorkManager;

        public InvoiceEventHandler(
            IUnitOfWorkManager unitOfWorkManager
        )
        {
            _unitOfWorkManager = unitOfWorkManager;
        }

        public async Task HandleEventAsync(EntityDeletingEventData<InvoiceLine> eventData)
        {
            await _unitOfWorkManager.WithUnitOfWorkAsync(async () =>
            {
                if (eventData.Entity.TicketId != null)
                {
                    eventData.Entity.TicketId = null;
                }
                if (eventData.Entity.ChargeId != null)
                {
                    eventData.Entity.ChargeId = null;
                }

                await Task.CompletedTask;
            });
        }
    }
}
