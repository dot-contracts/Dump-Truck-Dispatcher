using System;
using System.Linq;
using System.Threading.Tasks;
using Abp.Application.Services;
using Abp.Authorization;
using Abp.BackgroundJobs;
using Abp.Configuration;
using Abp.Domain.Repositories;
using Abp.Notifications;
using Abp.Runtime.Session;
using Abp.UI;
using DispatcherWeb.Authorization;
using DispatcherWeb.BackgroundJobs;
using DispatcherWeb.BackgroundJobs.Dto;
using DispatcherWeb.Configuration;
using DispatcherWeb.Dto;
using DispatcherWeb.Infrastructure.AzureBlobs;
using DispatcherWeb.Notifications;
using DispatcherWeb.Storage;
using DispatcherWeb.TempFiles.Dto;
using Microsoft.EntityFrameworkCore;

namespace DispatcherWeb.TempFiles
{
    [AbpAuthorize]
    public class TempFileAppService : DispatcherWebAppServiceBase, ITempFileAppService
    {
        private readonly IRepository<TempFile> _tempFileRepository;
        private readonly AzureBlobBinaryObjectManager _blobManager;
        private readonly IAttachmentHelper _attachmentHelper;
        private readonly IBackgroundJobManager _backgroundJobManager;
        private readonly INotificationPublisher _notificationPublisher;
        public TempFileAppService(IRepository<TempFile> tempFileRepository,
            AzureBlobBinaryObjectManager blobManager,
            IAttachmentHelper attachmentHelper,
            IBackgroundJobManager backgroundJobManager,
            INotificationPublisher notificationPublisher)
        {
            _tempFileRepository = tempFileRepository;
            _blobManager = blobManager;
            _attachmentHelper = attachmentHelper;
            _backgroundJobManager = backgroundJobManager;
            _notificationPublisher = notificationPublisher;
        }

        [AbpAuthorize(AppPermissions.Pages_TempFiles)]
        public async Task<TempFileDto> ProcessTempFile(ProcessTempFileInput input)
        {
            var fileId = await _attachmentHelper.UploadToAzureBlobAsync(input.FileBytes, Session.GetUserId(), input.MimeType, BlobContainerNames.TempFiles);
            var tempFileExpirationTime = await SettingManager.GetSettingValueAsync<int>(AppSettings.HostManagement.TempFileExpirationTime);
            string expireDurationMessage;
            var tempFile = new TempFileDto
            {
                FileGuid = fileId,
                ExpirationDateTime = DateTime.UtcNow.AddMinutes(tempFileExpirationTime),
                FileName = input.FileName,
                MimeType = input.MimeType,
                TenantId = await AbpSession.GetTenantIdOrNullAsync(),
                CreatorUserId = Session.UserId,
            };

            var tempFileId = await InsertAndGetIdAsync(tempFile);
            tempFile.Id = tempFileId;
            await _backgroundJobManager.EnqueueAsync<TempFileDeleteJob, TempFileDeleteJobArgs>(
                   new TempFileDeleteJobArgs
                   {
                       TempFileId = tempFileId,
                       RequestorUser = await Session.ToUserIdentifierAsync(),
                   },
                   delay: TimeSpan.FromMinutes(tempFileExpirationTime)
                   );
            if (tempFileExpirationTime >= 60)
            {
                var hours = tempFileExpirationTime / 60;
                expireDurationMessage = $"This link will expire in approximately {hours} hour(s) from creation time.";
            }
            else
            {
                expireDurationMessage = $"This link will expire in {tempFileExpirationTime} minute(s) from creation time.";
            }
            await _notificationPublisher.PublishAsync(
                    AppNotificationNames.DownloadFileCompleted,
                    new MessageNotificationData($"{input.Message} You can click this notification to download it. {expireDurationMessage}")
                    {
                        ["tempFileId"] = $"{tempFile.Id}",
                    },
                    null,
                    NotificationSeverity.Success,
                    userIds: new[] { await Session.ToUserIdentifierAsync() }
                );
            return tempFile;
        }

        [RemoteService(false)]
        public async Task DeleteTempFile(int tempFileId)
        {
            var tempFile = await GetAsync(tempFileId);
            if (tempFile != null && tempFile.ExpirationDateTime <= DateTime.UtcNow)
            {
                await _blobManager.DeleteAsync(tempFile.FileGuid);
            }
        }

        [AbpAuthorize(AppPermissions.Pages_TempFiles)]
        public async Task<FileBytesDto> DownloadTempFile(int tempFileId)
        {
            var result = new FileBytesDto();
            var tempFile = await GetAsync(tempFileId);
            if (tempFile.ExpirationDateTime < DateTime.UtcNow)
            {
                throw new UserFriendlyException("This link has expired.");
            }
            if (tempFile.CreatorUserId != AbpSession.UserId)
            {
                throw new UserFriendlyException("You are not authorized to access this file.");
            }
            var azureFilePath = $"{tempFile.CreatorUserId}/{tempFile.FileGuid}";
            var file = await _attachmentHelper.GetFromAzureBlobAsync(azureFilePath, BlobContainerNames.TempFiles);

            result.FileName = tempFile.FileName;
            result.FileBytes = file.Content;
            result.MimeType = tempFile.MimeType;
            return result;
        }

        private async Task<int> InsertAndGetIdAsync(TempFileDto tempFileDto)
        {
            return await _tempFileRepository.InsertAndGetIdAsync(new TempFile()
            {
                Id = tempFileDto.Id,
                FileGuid = tempFileDto.FileGuid,
                FileName = tempFileDto.FileName,
                MimeType = tempFileDto.MimeType,
                ExpirationDateTime = tempFileDto.ExpirationDateTime,
                TenantId = tempFileDto.TenantId,
                CreatorUserId = tempFileDto.CreatorUserId,
            }
            );
        }

        private async Task<TempFileDto> GetAsync(int tempFileId)
        {
            return await (await _tempFileRepository.GetQueryAsync())
                .Select(t => new TempFileDto()
                {
                    Id = t.Id,
                    FileGuid = t.FileGuid,
                    FileName = t.FileName,
                    MimeType = t.MimeType,
                    ExpirationDateTime = t.ExpirationDateTime,
                    TenantId = t.TenantId,
                    CreatorUserId = t.CreatorUserId,
                })
                .FirstOrDefaultAsync(t => t.Id == tempFileId);
        }
    }
}
