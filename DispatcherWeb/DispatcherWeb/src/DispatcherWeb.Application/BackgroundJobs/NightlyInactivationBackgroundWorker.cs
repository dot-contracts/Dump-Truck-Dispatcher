using System;
using System.Linq;
using System.Threading.Tasks;
using Abp.Dependency;
using Abp.Domain.Repositories;
using Abp.Domain.Uow;
using Abp.Threading.BackgroundWorkers;
using Abp.Threading.Timers;
using Abp.Timing;
using DispatcherWeb.Quotes;
using Microsoft.EntityFrameworkCore;

namespace DispatcherWeb.BackgroundJobs
{
    public class NightlyInactivationBackgroundWorker : AsyncPeriodicBackgroundWorkerBase, ISingletonDependency
    {
        private readonly IRepository<BackgroundJobHistory> _backgroundJobHistoryRepository;
        private readonly IRepository<Quote> _quoteRepository;

        public NightlyInactivationBackgroundWorker(
            AbpAsyncTimer timer,
            IRepository<BackgroundJobHistory> backgroundJobHistoryRepository,
            IRepository<Quote> quoteRepository
            )
            : base(timer)
        {
            _backgroundJobHistoryRepository = backgroundJobHistoryRepository;
            _quoteRepository = quoteRepository;

            Timer.Period = (int)TimeSpan.FromHours(1).TotalMilliseconds;
            Timer.RunOnStart = false;
        }

        [UnitOfWork]
        protected override async Task DoWorkAsync()
        {
            var timeZone = await SettingManager.GetSettingValueAsync(TimingSettingNames.TimeZone);

            var lastMidnightDate = DateTime.Now.ConvertTimeZoneTo(timeZone).Date;

            var lastRun = await (await _backgroundJobHistoryRepository.GetQueryAsync())
                .Where(x => x.Job == BackgroundJobEnum.NightlyInactivation)
                .OrderByDescending(x => x.StartTime)
                .Select(x => x.StartTime)
                .FirstOrDefaultAsync();

            if (lastRun >= lastMidnightDate)
            {
                return;
            }

            var historyRecord = new BackgroundJobHistory
            {
                Job = BackgroundJobEnum.NightlyInactivation,
                StartTime = DateTime.Now,
            };
            await _backgroundJobHistoryRepository.InsertAndGetIdAsync(historyRecord);

            var quotesToDeactivate = await (await _quoteRepository.GetQueryAsync())
                .Where(x => x.Status != QuoteStatus.Inactive
                            && x.InactivationDate < lastMidnightDate)
                .ToListAsync();

            var i = 0;
            var totalQuotesDeactivated = 0;
            foreach (var quote in quotesToDeactivate)
            {
                quote.Status = QuoteStatus.Inactive;

                totalQuotesDeactivated++;
                if (i++ >= 500)
                {
                    i = 0;
                    await CurrentUnitOfWork.SaveChangesAsync();
                }
            }

            await CurrentUnitOfWork.SaveChangesAsync();

            historyRecord.EndTime = DateTime.Now;
            historyRecord.Completed = true;
            historyRecord.Details = $"Total quotes deactivated: {totalQuotesDeactivated}.";
            await CurrentUnitOfWork.SaveChangesAsync();
        }
    }
}
