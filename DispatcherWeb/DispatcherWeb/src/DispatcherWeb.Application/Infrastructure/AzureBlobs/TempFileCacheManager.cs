using System;
using System.Threading.Tasks;
using Abp.BackgroundJobs;
using Abp.Runtime.Session;

namespace DispatcherWeb.Infrastructure.AzureBlobs
{
    public class TempFileCacheManager : ITempFileCacheManager
    {
        private readonly IBackgroundJobManager _backgroundJobManager;
        private readonly IAttachmentHelper _attachmentHelper;
        private readonly IAbpSession _session;

        public TempFileCacheManager(
            IBackgroundJobManager backgroundJobManager,
            IAttachmentHelper attachmentHelper,
            IAbpSession session
        )
        {
            _backgroundJobManager = backgroundJobManager;
            _attachmentHelper = attachmentHelper;
            _session = session;
        }

        public async Task<string> SetFileAsync(byte[] content, string contentType = null)
        {
            //todo we should simplify our file uploading logic
            var tenantId = await _session.GetTenantIdOrNullAsync() ?? 0;
            var fileId = await _attachmentHelper.UploadToAzureBlobAsync(content, tenantId, contentType, BlobContainerNames.TempFiles);
            var path = $"{tenantId}/{fileId}";

            await _backgroundJobManager.EnqueueAsync<DeleteTempFileJob, string>(path, delay: TimeSpan.FromMinutes(1));

            return fileId.ToString();
        }

        public async Task<byte[]> GetFileAsync(string fileId)
        {
            var ownerId = await _session.GetTenantIdOrNullAsync() ?? 0;
            var path = $"{ownerId}/{fileId}";

            var file = await _attachmentHelper.GetFromAzureBlobAsync(path, BlobContainerNames.TempFiles);

            return file.Content;
        }
    }
}
