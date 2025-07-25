using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Abp.Extensions;
using Abp.IO.Extensions;
using Abp.UI;
using Abp.Web.Models;
using DispatcherWeb.Authorization.Users.Profile;
using DispatcherWeb.Authorization.Users.Profile.Dto;
using DispatcherWeb.Dto;
using DispatcherWeb.Infrastructure.AzureBlobs;
using DispatcherWeb.Net.MimeTypes;
using DispatcherWeb.Web.Helpers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DispatcherWeb.Web.Controllers
{
    public abstract class ProfileControllerBase : DispatcherWebControllerBase
    {
        private readonly ITempFileCacheManager _tempFileCacheManager;
        private readonly IProfileAppService _profileAppService;

        private const int MaxProfilePictureSize = 5242880; //5MB

        protected ProfileControllerBase(
            ITempFileCacheManager tempFileCacheManager,
            IProfileAppService profileAppService)
        {
            _tempFileCacheManager = tempFileCacheManager;
            _profileAppService = profileAppService;
        }

        public async Task<UploadProfilePictureOutput> UploadProfilePicture(FileDto input)
        {
            try
            {
                var form = await Request.ReadFormAsync();
                var profilePictureFile = form.Files.First();

                //Check input
                if (profilePictureFile == null)
                {
                    throw new UserFriendlyException(L("ProfilePicture_Change_Error"));
                }

                if (profilePictureFile.Length > MaxProfilePictureSize)
                {
                    throw new UserFriendlyException(L("ProfilePicture_Warn_SizeLimit", AppConsts.MaxProfilePictureBytesUserFriendlyValue));
                }

                byte[] fileBytes;
                await using (var stream = profilePictureFile.OpenReadStream())
                {
                    fileBytes = stream.GetAllBytes();
                }

                if (!ImageFormatHelper.GetRawImageFormat(fileBytes).IsIn(ImageFormat.Jpeg, ImageFormat.Png, ImageFormat.Gif))
                {
                    throw new Exception(L("IncorrectImageFormat"));
                }

                input.FileToken = await _tempFileCacheManager.SetFileAsync(fileBytes);

                using (var ms = new MemoryStream(fileBytes))
                using (var bmpImage = new Bitmap(ms))
                {
                    return new UploadProfilePictureOutput
                    {
                        FileToken = input.FileToken,
                        FileName = input.FileName,
                        FileType = input.FileType,
                        Width = bmpImage.Width,
                        Height = bmpImage.Height,
                    };
                }
            }
            catch (UserFriendlyException ex)
            {
                return new UploadProfilePictureOutput(new ErrorInfo(ex.Message));
            }
        }

        [AllowAnonymous]
        public FileResult GetDefaultProfilePicture()
        {
            return GetDefaultProfilePictureInternal();
        }

        public async Task<FileResult> GetProfilePictureByUser(long userId)
        {
            var output = await _profileAppService.GetProfilePictureByUser(userId);
            if (output.ProfilePicture.IsNullOrEmpty())
            {
                return GetDefaultProfilePictureInternal();
            }

            return File(Convert.FromBase64String(output.ProfilePicture), MimeTypeNames.ImageJpeg);
        }

        protected FileResult GetDefaultProfilePictureInternal()
        {
            return File(Path.Combine("Common", "Images", "default-profile-picture.png"), MimeTypeNames.ImagePng);
        }
    }
}
