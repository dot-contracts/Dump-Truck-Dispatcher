using System;
using System.IO;
using System.Threading.Tasks;
using Azure.Storage.Blobs;

namespace DispatcherWeb.Infrastructure.AzureBlobs
{
    public interface IAttachmentHelper
    {
        Task DeleteFromAzureBlobAsync(string path, string containerName = null);
        Task<BlobContainerClient> GetBlobContainerAsync(string containerName = null);
        Task<ContentWithType> GetFromAzureBlobAsync(string path, string containerName = null);
        Task<Guid> UploadToAzureBlobAsync(byte[] file, long ownerId, string contentType = null, string containerName = null);
        Task<Guid> UploadToAzureBlobAsync(Stream fileStream, long ownerId, string contentType = null, string containerName = null);
    }
}
