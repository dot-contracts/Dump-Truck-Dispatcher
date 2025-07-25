using System;
using System.IO;
using System.Threading.Tasks;

namespace DispatcherWeb.Infrastructure.AzureBlobs
{
    public interface ISecureFileBlobService
    {
        Task UploadSecureFileAsync(Stream fileStream, Guid id, string fileName);
        Task<Stream> GetStreamFromAzureBlobAsync(string path);
        Task AddChildBlob(string blobName, string childBlobName, string childContent);
        Task<string> GetChildBlobAsync(string blobName, string childBlobName);
    }
}
