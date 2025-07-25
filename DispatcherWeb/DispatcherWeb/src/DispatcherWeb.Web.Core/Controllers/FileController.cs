using System.Threading.Tasks;
using Abp.Auditing;
using DispatcherWeb.Infrastructure.AzureBlobs;
using DispatcherWeb.Infrastructure.AzureBlobs.Dto;
using Microsoft.AspNetCore.Mvc;

namespace DispatcherWeb.Web.Controllers
{
    public class FileController : DispatcherWebControllerBase
    {
        private readonly ITempFileCacheManager _tempFileCacheManager;
        //private readonly IBinaryObjectManager _binaryObjectManager;
        //private readonly IMimeTypeMap _mimeTypeMap;

        public FileController(
            ITempFileCacheManager tempFileCacheManager
        )
        {
            _tempFileCacheManager = tempFileCacheManager;
        }

        [DisableAuditing]
        public async Task<ActionResult> DownloadTempFile(GetTempFileInput file)
        {
            var fileBytes = await _tempFileCacheManager.GetFileAsync(file.FileToken);
            if (fileBytes?.Length > 0)
            {
                return File(fileBytes, file.FileType, file.FileName);
            }

            return NotFound(L("RequestedFileDoesNotExists"));
        }

        //[DisableAuditing]
        //public async Task<ActionResult> DownloadBinaryFile(Guid id, string contentType, string fileName)
        //{
        //    var fileObject = await _binaryObjectManager.GetOrNullAsync(id);
        //    if (fileObject == null)
        //    {
        //        return StatusCode((int)HttpStatusCode.NotFound);
        //    }
        //
        //    if (fileName.IsNullOrEmpty())
        //    {
        //        if (!fileObject.Description.IsNullOrEmpty() &&
        //            !Path.GetExtension(fileObject.Description).IsNullOrEmpty())
        //        {
        //            fileName = fileObject.Description;
        //        }
        //        else
        //        {
        //            return StatusCode((int)HttpStatusCode.BadRequest);
        //        }
        //    }
        //
        //    if (contentType.IsNullOrEmpty())
        //    {
        //        if (!Path.GetExtension(fileName).IsNullOrEmpty())
        //        {
        //            contentType = _mimeTypeMap.GetMimeType(fileName);
        //        }
        //        else
        //        {
        //            return StatusCode((int)HttpStatusCode.BadRequest);
        //        }
        //    }
        //
        //    return File(fileObject.Bytes, contentType, fileName);
        //}
    }
}
