using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Abp.Dependency;
using Azure.Storage.Blobs;

namespace DispatcherWeb.Infrastructure.AzureBlobs
{
    public class SecureFileBlobService : ISecureFileBlobService, ISingletonDependency
    {
        private readonly IAttachmentHelper _attachmentHelper;

        public SecureFileBlobService(
            IAttachmentHelper attachmentHelper
        )
        {
            _attachmentHelper = attachmentHelper;
        }

        public async Task UploadSecureFileAsync(Stream fileStream, Guid id, string fileName)
        {
            var filesBlobContainer = await GetBlobContainerAsync();
            var path = $"{id}/{fileName}";
            var fileBlob = filesBlobContainer.GetBlobClient(path);
            await fileBlob.UploadAsync(fileStream, overwrite: true);
        }

        public async Task<Stream> GetStreamFromAzureBlobAsync(string path)
        {
            if (string.IsNullOrEmpty(path))
            {
                return null;
            }

            var filesBlobContainer = await GetBlobContainerAsync();
            var fileBlob = filesBlobContainer.GetBlobClient(path);

            if (!await fileBlob.ExistsAsync())
            {
                return null;
            }

            var memoryStream = new MemoryStream();
            await fileBlob.DownloadToAsync(memoryStream);
            memoryStream.Position = 0;
            return memoryStream;
        }

        public async Task AddChildBlob(string blobName, string childBlobName, string childContent)
        {
            var fullChildName = $"{blobName}/{childBlobName}";
            var blobContainer = await GetBlobContainerAsync();
            var fileBlob = blobContainer.GetBlobClient(fullChildName);

            using (var stream = new MemoryStream(Encoding.UTF8.GetBytes(childContent)))
            {
                await fileBlob.UploadAsync(stream, overwrite: true);
            }
        }

        public async Task<string> GetChildBlobAsync(string blobName, string childBlobName)
        {
            var blobContainer = await GetBlobContainerAsync();
            var fullChildName = $"{blobName}/{childBlobName}";
            var fileBlob = blobContainer.GetBlobClient(fullChildName);

            if (!await fileBlob.ExistsAsync())
            {
                return null;
            }

            var response = await fileBlob.DownloadContentAsync();
            using (var streamReader = new StreamReader(response.Value.Content.ToStream()))
            {
                return await streamReader.ReadToEndAsync();
            }
        }

        private async Task<BlobContainerClient> GetBlobContainerAsync()
        {
            return await _attachmentHelper.GetBlobContainerAsync(BlobContainerNames.SecureFiles);
        }
    }
}
