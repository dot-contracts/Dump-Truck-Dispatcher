using System.Threading.Tasks;
using Abp.Dependency;
using DispatcherWeb.BackgroundJobs.Dto;
using DispatcherWeb.Runtime.Session;
using DispatcherWeb.TempFiles;

namespace DispatcherWeb.BackgroundJobs
{
    public class TempFileDeleteJob : DispatcherWebAsyncBackgroundJobBase<TempFileDeleteJobArgs>, ITransientDependency
    {
        private readonly ITempFileAppService _tempFilesService;

        public TempFileDeleteJob(
            ITempFileAppService tempFilesService,
            IExtendedAbpSession session)
            : base(session)
        {
            _tempFilesService = tempFilesService;
        }

        public override async Task ExecuteAsync(TempFileDeleteJobArgs args)
        {
            using (Session.Use(args.RequestorUser))
            {
                await WithUnitOfWorkAsync(args.RequestorUser, async () =>
                {
                    await _tempFilesService.DeleteTempFile(args.TempFileId);
                });
            }
        }
    }
}
