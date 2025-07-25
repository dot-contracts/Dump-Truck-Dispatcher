using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Abp.Dependency;
using Azure;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Castle.Core.Logging;
using DispatcherWeb.Configuration;
using DispatcherWeb.Storage;
using Microsoft.Extensions.Configuration;

namespace DispatcherWeb.Infrastructure.AzureBlobs
{
    public class AzureBlobBinaryObjectManager : IBinaryObjectManager, ITransientDependency
    {
        private const string TenantId = "TenantId";
        private readonly IConfigurationRoot _configuration;
        private readonly string _storageConnectionString;

        public ILogger Logger { get; set; }

        public AzureBlobBinaryObjectManager(
            IAppConfigurationAccessor configurationAccessor
            )
        {
            Logger = NullLogger.Instance;
            _configuration = configurationAccessor.Configuration;
            _storageConnectionString = _configuration["Abp:StorageConnectionString"];
        }

        public async Task<BinaryObject> GetOrNullAsync(Guid id)
        {
            var filesBlobContainer = await GetBlobContainerAsync();
            var fileBlob = filesBlobContainer.GetBlobClient($"{id}");
            if (!await fileBlob.ExistsAsync())
            {
                Logger.Error($"The blob with id={id} doesn't exist.");
                return null;
            }

            BlobProperties properties = await fileBlob.GetPropertiesAsync();
            using (var ms = new MemoryStream())
            {
                await fileBlob.DownloadToAsync(ms);
                byte[] fileBytes = ms.ToArray();

                int? tenantId = null;
                if (properties.Metadata.TryGetValue(TenantId, out string tenantIdValue) && int.TryParse(tenantIdValue, out int parsedTenantId))
                {
                    tenantId = parsedTenantId;
                }

                var binaryObject = new BinaryObject(tenantId, fileBytes) { Id = id };
                return binaryObject;
            }
        }

        public async Task SaveAsync(BinaryObject file)
        {
            var filesBlobContainer = await GetBlobContainerAsync();
            var path = $"{file.Id}";
            var fileBlob = filesBlobContainer.GetBlobClient(path);

            var metadata = new Dictionary<string, string>();
            if (file.TenantId.HasValue)
            {
                metadata.Add(TenantId, file.TenantId.Value.ToString());
            }

            using (var ms = new MemoryStream(file.Bytes))
            {
                var options = new BlobUploadOptions
                {
                    Metadata = metadata,
                    Conditions = new BlobRequestConditions { IfNoneMatch = ETag.All },
                };

                await fileBlob.UploadAsync(ms, options);
            }
        }

        public async Task DeleteAsync(Guid id)
        {
            var filesBlobContainer = await GetBlobContainerAsync();
            var fileBlob = filesBlobContainer.GetBlobClient($"{id}");
            await fileBlob.DeleteIfExistsAsync(DeleteSnapshotsOption.IncludeSnapshots);
        }

        private async Task<BlobContainerClient> GetBlobContainerAsync()
        {
            var blobServiceClient = new BlobServiceClient(_storageConnectionString, AttachmentHelper.GetBlobClientOptions(_configuration));
            var filesBlobContainer = blobServiceClient.GetBlobContainerClient(BlobContainerNames.BinaryObjects);

            if (!await filesBlobContainer.ExistsAsync())
            {
                await filesBlobContainer.CreateIfNotExistsAsync();
            }

            return filesBlobContainer;
        }
    }
}
