using System.Threading.Tasks;
using Abp.BackgroundJobs;
using Abp.Dependency;

namespace DispatcherWeb.Infrastructure.AzureBlobs
{
    public class DeleteTempFileJob : AsyncBackgroundJob<string>, ITransientDependency
    {
        private readonly IAttachmentHelper _attachmentHelper;

        public DeleteTempFileJob(
            IAttachmentHelper attachmentHelper
        )
        {
            _attachmentHelper = attachmentHelper;
        }

        public override async Task ExecuteAsync(string path)
        {
            await _attachmentHelper.DeleteFromAzureBlobAsync(path, BlobContainerNames.TempFiles);
        }
    }
}
