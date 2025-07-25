using System;
using System.Linq;
using System.Threading.Tasks;
using Abp.AspNetCore.Mvc.Authorization;
using Abp.IO.Extensions;
using Abp.UI;
using Abp.Web.Models;
using DispatcherWeb.Chat;
using DispatcherWeb.Storage;
using Microsoft.AspNetCore.Mvc;

namespace DispatcherWeb.Web.Controllers
{
    public class ChatControllerBase : DispatcherWebControllerBase
    {
        protected readonly IBinaryObjectManager BinaryObjectManager;
        protected readonly IChatMessageManager ChatMessageManager;

        public ChatControllerBase(IBinaryObjectManager binaryObjectManager, IChatMessageManager chatMessageManager)
        {
            BinaryObjectManager = binaryObjectManager;
            ChatMessageManager = chatMessageManager;
        }

        [HttpPost]
        [AbpMvcAuthorize]
        public async Task<JsonResult> UploadFile()
        {
            try
            {
                var form = await Request.ReadFormAsync();
                var file = form.Files.First();

                //Check input
                if (file == null)
                {
                    throw new UserFriendlyException(L("File_Empty_Error"));
                }

                if (file.Length > 10000000) //10MB
                {
                    throw new UserFriendlyException(L("File_SizeLimit_Error"));
                }

                byte[] fileBytes;
                await using (var stream = file.OpenReadStream())
                {
                    fileBytes = stream.GetAllBytes();
                }

                var fileObject = new BinaryObject(null, fileBytes, $"File uploaded from chat by {AbpSession.UserId}, File name: {file.FileName} {DateTime.UtcNow}");
                using (CurrentUnitOfWork.SetTenantId(null))
                {
                    await BinaryObjectManager.SaveAsync(fileObject);
                    await CurrentUnitOfWork.SaveChangesAsync();
                }
                return Json(new AjaxResponse(new
                {
                    id = fileObject.Id,
                    name = file.FileName,
                    contentType = file.ContentType,
                }));
            }
            catch (UserFriendlyException ex)
            {
                return Json(new AjaxResponse(new ErrorInfo(ex.Message)));
            }
        }

        [HttpPost]
        [AbpMvcAuthorize]

        public async Task DeleteFile(Guid fileId)
        {
            await BinaryObjectManager.DeleteAsync(fileId);
        }

    }
}
