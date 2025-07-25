using System;
using System.Linq;
using System.Threading.Tasks;
using Abp.Dependency;
using Abp.Domain.Repositories;
using Abp.Domain.Uow;
using Abp.Threading.BackgroundWorkers;
using Abp.Threading.Timers;
using DispatcherWeb.Dispatching;
using DispatcherWeb.Orders;
using DispatcherWeb.Storage;
using Microsoft.EntityFrameworkCore;

namespace DispatcherWeb.BackgroundJobs
{
    public class DeferredBinaryObjectSyncBackgroundWorker : AsyncPeriodicBackgroundWorkerBase, ISingletonDependency
    {
        private readonly IRepository<DeferredBinaryObject, Guid> _deferredBinaryObjectRepository;
        private readonly IRepository<Load> _loadRepository;
        private readonly IRepository<Ticket> _ticketRepository;

        public DeferredBinaryObjectSyncBackgroundWorker(
            AbpAsyncTimer timer,
            IRepository<DeferredBinaryObject, Guid> deferredBinaryObjectRepository,
            IRepository<Load> loadRepository,
            IRepository<Ticket> ticketRepository
            )
            : base(timer)
        {
            _deferredBinaryObjectRepository = deferredBinaryObjectRepository;
            _loadRepository = loadRepository;
            _ticketRepository = ticketRepository;
            Timer.Period = (int)TimeSpan.FromMinutes(5).TotalMilliseconds;
            Timer.RunOnStart = false;
        }

        [UnitOfWork]
        protected override async Task DoWorkAsync()
        {
            using (CurrentUnitOfWork.DisableFilter(AbpDataFilters.MayHaveTenant, AbpDataFilters.MustHaveTenant))
            {
                var deferredBinaryObjects = await (await _deferredBinaryObjectRepository.GetQueryAsync()).ToListAsync();

                var matchedTicketCount = 0;
                var matchedLoadsCount = 0;
                var totalProcessedDeferredCount = 0;
                var unmatchedDeferredCount = 0;
                foreach (var deferredBinaryObjectsChunk in deferredBinaryObjects.Chunk(500))
                {
                    var loadDeferredIds = deferredBinaryObjectsChunk
                        .Where(x => x.Destination == DeferredBinaryObjectDestination.LoadSignature)
                        .Select(x => x.Id).ToList();
                    var loads = loadDeferredIds.Any()
                        ? await (await _loadRepository.GetQueryAsync())
                            .Where(x => x.DeferredSignatureId.HasValue && loadDeferredIds.Contains(x.DeferredSignatureId.Value))
                            .ToListAsync()
                        : new();

                    var ticketDeferredIds = deferredBinaryObjectsChunk
                        .Where(x => x.Destination == DeferredBinaryObjectDestination.TicketPhoto)
                        .Select(x => x.Id).ToList();
                    var tickets = ticketDeferredIds.Any()
                        ? await (await _ticketRepository.GetQueryAsync())
                            .Where(x => x.DeferredTicketPhotoId.HasValue && loadDeferredIds.Contains(x.DeferredTicketPhotoId.Value))
                            .ToListAsync()
                        : new();

                    foreach (var deferredBinaryObject in deferredBinaryObjectsChunk)
                    {
                        totalProcessedDeferredCount++;
                        switch (deferredBinaryObject.Destination)
                        {
                            case DeferredBinaryObjectDestination.LoadSignature:
                                var matchingLoad = loads.FirstOrDefault(x => x.DeferredSignatureId == deferredBinaryObject.Id);
                                if (matchingLoad != null)
                                {
                                    if (matchingLoad.SignatureId == null)
                                    {
                                        matchingLoad.DeferredSignatureId = null;
                                        matchingLoad.SignatureId = deferredBinaryObject.BinaryObjectId;
                                    }
                                    else
                                    {
                                        continue;
                                    }
                                    matchedLoadsCount++;
                                    await _deferredBinaryObjectRepository.DeleteAsync(deferredBinaryObject);
                                }
                                else
                                {
                                    unmatchedDeferredCount++;
                                }
                                break;
                            case DeferredBinaryObjectDestination.TicketPhoto:
                                var matchingTicket = tickets.FirstOrDefault(x => x.DeferredTicketPhotoId == deferredBinaryObject.Id);
                                if (matchingTicket != null)
                                {
                                    if (matchingTicket.TicketPhotoId == null)
                                    {
                                        matchingTicket.DeferredTicketPhotoId = null;
                                        matchingTicket.TicketPhotoId = deferredBinaryObject.BinaryObjectId;
                                    }
                                    else
                                    {
                                        continue;
                                    }

                                    matchedTicketCount++;
                                    await _deferredBinaryObjectRepository.DeleteAsync(deferredBinaryObject);

                                }
                                else
                                {
                                    unmatchedDeferredCount++;
                                }
                                break;
                        }
                    }
                    await CurrentUnitOfWork.SaveChangesAsync();
                }

                Logger.Info($"DeferredBinaryObjectSyncBackgroundWorker: processed {totalProcessedDeferredCount} records, matched {matchedLoadsCount} loads, {matchedTicketCount} tickets; {unmatchedDeferredCount} records unmatched");
            }
        }
    }
}
