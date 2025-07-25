using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Abp.Application.Services.Dto;
using DispatcherWeb.Dto;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace DispatcherWeb.Web.Utils
{
    public static class Extensions
    {
        public static SelectList ToSelectList(this ListResultDto<SelectListDto> items)
        {
            return new SelectList(items.Items, "Id", "Name");
        }

        public static IEnumerable<SelectListItem> GetEnumSelectList<TEnum>(this IHtmlHelper html, TEnum defaultValue) where TEnum : struct
        {
            var list = html.GetEnumSelectList<TEnum>();

            foreach (var item in list)
            {
                if (item.Value == ((int)(object)defaultValue).ToString())
                {
                    item.Selected = true;
                }
            }
            return list;
        }

        /// <summary>
        /// Retrieve the raw body as a string from the Request.Body stream
        /// </summary>
        /// <param name="request">Request instance to apply to</param>
        /// <param name="encoding">Optional - Encoding, defaults to UTF8</param>
        /// <returns></returns>
        public static async Task<string> GetRawBodyStringAsync(this HttpRequest request, Encoding encoding = null)
        {
            encoding ??= Encoding.UTF8;

            using (var reader = new StreamReader(request.Body, encoding))
            {
                return await reader.ReadToEndAsync();
            }
        }

        /// <summary>
        /// Retrieves the raw body as a byte array from the Request.Body stream
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public static async Task<byte[]> GetRawBodyBytesAsync(this HttpRequest request)
        {
            using (var ms = new MemoryStream(2048))
            {
                await request.Body.CopyToAsync(ms);
                return ms.ToArray();
            }
        }

    }
}
