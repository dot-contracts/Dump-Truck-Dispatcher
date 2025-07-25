using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Abp.AspNetCore.Mvc.Authorization;
using Abp.Auditing;
using Abp.Domain.Uow;
using Abp.Extensions;
using Abp.IO.Extensions;
using Abp.Runtime.Session;
using Abp.UI;
using Abp.Web.Models;
using DispatcherWeb.Authorization.Users;
using DispatcherWeb.Authorization.Users.Profile;
using DispatcherWeb.Authorization.Users.Profile.Dto;
using DispatcherWeb.Dto;
using DispatcherWeb.Friendships;
using DispatcherWeb.Images;
using DispatcherWeb.Infrastructure.AzureBlobs;
using DispatcherWeb.Net.MimeTypes;
using DispatcherWeb.Storage;
using DispatcherWeb.Web.Common;
using Microsoft.AspNetCore.Mvc;

namespace DispatcherWeb.Web.Controllers
{
    [AbpMvcAuthorize]
    [DisableAuditing]
    public class ProfileController : ProfileControllerBase
    {
        private readonly UserManager _userManager;
        private readonly IBinaryObjectManager _binaryObjectManager;
        private readonly IAppFolders _appFolders;
        private readonly IFriendshipManager _friendshipManager;
        private readonly ITempFileCacheManager _tempFileCacheManager;

        public ProfileController(
                UserManager userManager,
                IProfileAppService profileAppService,
                IBinaryObjectManager binaryObjectManager,
                ITempFileCacheManager tempFileCacheManager,
                IAppFolders appFolders,
                IFriendshipManager friendshipManager
            ) : base(tempFileCacheManager, profileAppService)
        {
            _userManager = userManager;
            _binaryObjectManager = binaryObjectManager;
            _appFolders = appFolders;
            _friendshipManager = friendshipManager;
            _tempFileCacheManager = tempFileCacheManager;
        }

        public async Task<FileResult> GetProfilePicture()
        {
            var user = await _userManager.GetUserByIdAsync(AbpSession.GetUserId());
            if (user.ProfilePictureId == null)
            {
                return GetDefaultProfilePicture();
            }

            return await GetProfilePictureById(user.ProfilePictureId.Value);
        }

        [ResponseCache(CacheProfileName = WebConsts.CacheProfiles.StaticFiles)]
        public async Task<FileResult> GetProfilePictureById(string id = "")
        {
            if (id.IsNullOrEmpty())
            {
                return GetDefaultProfilePicture();
            }

            return await GetProfilePictureById(Guid.Parse(id));
        }

        [UnitOfWork]
        [ResponseCache(CacheProfileName = WebConsts.CacheProfiles.StaticFiles)]
        public virtual async Task<FileResult> GetFriendProfilePictureById(long userId, int? tenantId, string id = "")
        {
            if (id.IsNullOrEmpty())
            {
                return GetDefaultProfilePicture();
            }

            using (CurrentUnitOfWork.SetTenantId(tenantId))
            {
                return await GetProfilePictureById(Guid.Parse(id));
            }
        }

        private async Task<FileResult> GetProfilePictureById(Guid profilePictureId)
        {
            var file = await _binaryObjectManager.GetOrNullAsync(profilePictureId);
            if (file == null)
            {
                return GetDefaultProfilePicture();
            }

            return File(file.Bytes, MimeTypeNames.ImageJpeg);
        }

        [RequestSizeLimit(int.MaxValue)]
        public async Task<UploadProfilePictureOutput> UploadSignaturePicture(FileDto input)
        {
            try
            {
                var form = await Request.ReadFormAsync();
                var signatureFile = form.Files.FirstOrDefault();

                //Check input
                if (signatureFile == null || signatureFile.Length == 0)
                {
                    throw new UserFriendlyException("File is required.");
                }

                if (signatureFile.Length > 1048576) //1MB.
                {
                    throw new UserFriendlyException(L("ProfilePicture_Warn_SizeLimit", AppConsts.MaxSignaturePictureBytesUserFriendlyValue));
                }

                byte[] fileBytes;
                await using (var stream = signatureFile.OpenReadStream())
                {
                    fileBytes = stream.GetAllBytes();
                }

                //Check file type, format, size
                await using (var ms = new MemoryStream(fileBytes))
                {
                    var fileImage = Image.FromStream(ms);
                    var acceptedFormats = new List<ImageFormat>
                    {
                        ImageFormat.Jpeg,
                        ImageFormat.Png,
                        ImageFormat.Gif,
                    };

                    if (!acceptedFormats.Contains(fileImage.RawFormat))
                    {
                        throw new ApplicationException(L("ProfilePicture_Warn_FileType"));
                    }

                    if (fileImage.Width > Authorization.Users.User.MaxSignaturePictureWidth
                        || fileImage.Height > Authorization.Users.User.MaxSignaturePictureHeight)
                    {
                        var newImage = ImageHelper.ResizePreservingRatio(fileImage,
                            Authorization.Users.User.MaxSignaturePictureWidth,
                            Authorization.Users.User.MaxSignaturePictureHeight);
                        await using (var saveStream = new MemoryStream())
                        {
                            newImage.Save(saveStream, fileImage.RawFormat);
                            saveStream.Seek(0, SeekOrigin.Begin);
                            fileBytes = saveStream.GetAllBytes();
                        }
                    }
                }

                input.FileToken = await _tempFileCacheManager.SetFileAsync(fileBytes);

                return new UploadProfilePictureOutput
                {
                    FileToken = input.FileToken,
                    FileName = input.FileName,
                    FileType = input.FileType,
                };
            }
            catch (UserFriendlyException ex)
            {
                Logger.Error(ex.ToString());
                return new UploadProfilePictureOutput(new ErrorInfo(ex.Message));
            }
        }
    }
}
