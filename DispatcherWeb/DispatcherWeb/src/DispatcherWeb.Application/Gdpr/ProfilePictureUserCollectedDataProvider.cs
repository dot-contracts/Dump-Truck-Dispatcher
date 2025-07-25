using System.Collections.Generic;
using System.Threading.Tasks;
using Abp;
using Abp.Dependency;
using DispatcherWeb.Authorization.Users;
using DispatcherWeb.Dto;
using DispatcherWeb.Infrastructure.AzureBlobs;
using DispatcherWeb.Infrastructure.Extensions;
using DispatcherWeb.Net.MimeTypes;
using DispatcherWeb.Storage;

namespace DispatcherWeb.Gdpr
{
    public class ProfilePictureUserCollectedDataProvider : IUserCollectedDataProvider, ITransientDependency
    {
        private readonly UserManager _userManager;
        private readonly IBinaryObjectManager _binaryObjectManager;
        private readonly ITempFileCacheManager _tempFileCacheManager;

        public ProfilePictureUserCollectedDataProvider(
            UserManager userManager,
            IBinaryObjectManager binaryObjectManager,
            ITempFileCacheManager tempFileCacheManager
        )
        {
            _userManager = userManager;
            _binaryObjectManager = binaryObjectManager;
            _tempFileCacheManager = tempFileCacheManager;
        }

        public async Task<List<FileDto>> GetFiles(UserIdentifier user)
        {
            var profilePictureId = (await _userManager.GetUserByIdAsync(user.UserId)).ProfilePictureId;
            if (!profilePictureId.HasValue)
            {
                return new List<FileDto>();
            }

            var profilePicture = await _binaryObjectManager.GetOrNullAsync(profilePictureId.Value);
            if (profilePicture == null)
            {
                return new List<FileDto>();
            }

            var file = await _tempFileCacheManager.StoreTempFileAsync(new FileBytesDto
            {
                FileName = "ProfilePicture.png",
                FileBytes = profilePicture.Bytes,
                MimeType = MimeTypeNames.ImagePng,
            });

            return new List<FileDto> { file };
        }
    }
}
