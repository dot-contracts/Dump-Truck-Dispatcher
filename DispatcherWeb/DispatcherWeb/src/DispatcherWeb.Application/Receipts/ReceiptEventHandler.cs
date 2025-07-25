using System.Linq;
using System.Threading.Tasks;
using Abp.Dependency;
using Abp.Domain.Repositories;
using Abp.Domain.Uow;
using Abp.Events.Bus.Entities;
using Abp.Events.Bus.Handlers;
using DispatcherWeb.Orders;
using Microsoft.EntityFrameworkCore;

namespace DispatcherWeb.Receipts
{
    public class ReceiptEventHandler : IAsyncEventHandler<EntityDeletingEventData<ReceiptLine>>, ITransientDependency
    {
        private readonly IRepository<Ticket> _ticketRepository;
        private readonly IUnitOfWorkManager _unitOfWorkManager;

        public ReceiptEventHandler(
            IRepository<Ticket> ticketRepository,
            IUnitOfWorkManager unitOfWorkManager
            )
        {
            _ticketRepository = ticketRepository;
            _unitOfWorkManager = unitOfWorkManager;
        }

        public async Task HandleEventAsync(EntityDeletingEventData<ReceiptLine> eventData)
        {
            await _unitOfWorkManager.WithUnitOfWorkAsync(async () =>
            {
                var tickets = await (await _ticketRepository.GetQueryAsync())
                    .Where(x => x.ReceiptLineId == eventData.Entity.Id)
                    .ToListAsync();

                if (tickets.Any())
                {
                    tickets.ForEach(x => x.ReceiptLineId = null);
                }
            });
        }
    }
}
