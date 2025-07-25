using System.Threading.Tasks;
using DispatcherWeb.Dto;
using DispatcherWeb.Infrastructure.AzureBlobs;

namespace DispatcherWeb.Infrastructure.Extensions
{
    public static class TempFileExtensions
    {
        public static async Task<FileDto> StoreTempFileAsync(this ITempFileCacheManager tempFileCacheManager, FileBytesDto file)
        {
            var tempFile = new FileDto(file.FileName, file.MimeType)
            {
                FileToken = await tempFileCacheManager.SetFileAsync(file.FileBytes, file.MimeType),
            };

            return tempFile;
        }
    }
}
