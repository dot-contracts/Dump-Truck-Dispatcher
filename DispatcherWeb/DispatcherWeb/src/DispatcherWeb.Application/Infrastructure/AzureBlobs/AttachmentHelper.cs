using System;
using System.IO;
using System.Threading.Tasks;
using Abp.Dependency;
using Azure;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using DispatcherWeb.Configuration;
using Microsoft.Extensions.Configuration;

namespace DispatcherWeb.Infrastructure.AzureBlobs
{
    public class AttachmentHelper : IAttachmentHelper, ISingletonDependency
    {
        private readonly IConfigurationRoot _configuration;
        private readonly string _storageConnectionString;

        public AttachmentHelper(
            IAppConfigurationAccessor configurationAccessor
        )
        {
            _configuration = configurationAccessor.Configuration;
            _storageConnectionString = _configuration["Abp:StorageConnectionString"];
        }

        public async Task<BlobContainerClient> GetBlobContainerAsync(string containerName = null)
        {
            containerName ??= BlobContainerNames.Default;

            var blobServiceClient = new BlobServiceClient(_storageConnectionString, GetBlobClientOptions());
            var containerClient = blobServiceClient.GetBlobContainerClient(containerName);

            if (!await containerClient.ExistsAsync())
            {
                await containerClient.CreateIfNotExistsAsync();
            }
            return containerClient;
        }

        public BlobClientOptions GetBlobClientOptions()
        {
            return GetBlobClientOptions(_configuration);
        }

        public static BlobClientOptions GetBlobClientOptions(IConfigurationRoot configuration)
        {
            var version = BlobClientOptions.ServiceVersion.V2025_01_05;
            if (!string.IsNullOrEmpty(configuration["Abp:StorageApiVersion"]) && int.TryParse(configuration["Abp:StorageApiVersion"], out var versionInt))
            {
                version = (BlobClientOptions.ServiceVersion)versionInt;
            }
            return new BlobClientOptions(version);
        }

        public async Task<Guid> UploadToAzureBlobAsync(byte[] file, long ownerId, string contentType = null, string containerName = null)
        {
            var containerClient = await GetBlobContainerAsync(containerName);
            var fileId = Guid.NewGuid();
            var path = $"{ownerId}/{fileId}";
            var blobClient = containerClient.GetBlobClient(path);

            using var ms = new MemoryStream(file);
            var options = new BlobUploadOptions
            {
                Conditions = new BlobRequestConditions { IfNoneMatch = ETag.All },
                HttpHeaders = new BlobHttpHeaders { ContentType = contentType },
            };

            await blobClient.UploadAsync(ms, options);
            return fileId;
        }

        public async Task<Guid> UploadToAzureBlobAsync(Stream fileStream, long ownerId, string contentType = null, string containerName = null)
        {
            var containerClient = await GetBlobContainerAsync(containerName);
            var fileId = Guid.NewGuid();
            var path = $"{ownerId}/{fileId}";
            var blobClient = containerClient.GetBlobClient(path);

            var options = new BlobUploadOptions
            {
                Conditions = new BlobRequestConditions { IfNoneMatch = ETag.All },
                HttpHeaders = new BlobHttpHeaders { ContentType = contentType },
            };

            await blobClient.UploadAsync(fileStream, options);
            return fileId;
        }

        public async Task<ContentWithType> GetFromAzureBlobAsync(string path, string containerName = null)
        {
            var contentWithType = new ContentWithType();
            if (string.IsNullOrEmpty(path))
            {
                contentWithType.Content = [];
                return contentWithType;
            }

            var containerClient = await GetBlobContainerAsync(containerName);
            var blobClient = containerClient.GetBlobClient(path);

            if (!await blobClient.ExistsAsync())
            {
                contentWithType.Content = [];
                return contentWithType;
            }

            using (var ms = new MemoryStream())
            {
                var response = await blobClient.DownloadToAsync(ms);
                contentWithType.Content = ms.ToArray();
                contentWithType.ContentType = response.Headers.ContentType;
                return contentWithType;
            }
        }

        public async Task DeleteFromAzureBlobAsync(string path, string containerName = null)
        {
            if (string.IsNullOrEmpty(path))
            {
                return;
            }

            var containerClient = await GetBlobContainerAsync(containerName);
            var blobClient = containerClient.GetBlobClient(path);
            await blobClient.DeleteIfExistsAsync(DeleteSnapshotsOption.IncludeSnapshots);
        }
    }
}
