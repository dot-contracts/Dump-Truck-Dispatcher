using System;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Razor.TagHelpers;
using Microsoft.Extensions.Hosting;

namespace DispatcherWeb.Web.TagHelpers
{
    [HtmlTargetElement("script", Attributes = AbpSrcAttributeName)]
    public class DispatcherWebScriptSrcTagHelper : TagHelper
    {
        private const string AbpSrcAttributeName = "abp-src";

        private readonly IWebHostEnvironment _hostingEnvironment;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public override int Order => -1000 - 1;

        public DispatcherWebScriptSrcTagHelper(IWebHostEnvironment hostingEnvironment,
            IHttpContextAccessor httpContextAccessor)
        {
            _hostingEnvironment = hostingEnvironment;
            _httpContextAccessor = httpContextAccessor;
        }

        public override void Process(TagHelperContext context, TagHelperOutput output)
        {
            if (output.Attributes["abp-ignore-src-modification"]?.Value.ToString() == "true")
            {
                base.Process(context, output);
                return;
            }

            var abpSrc = output.Attributes[AbpSrcAttributeName].Value;
            if (abpSrc is null or not (HtmlString or string))
            {
                base.Process(context, output);
                return;
            }

            var href = abpSrc.ToString();
            if (href?.StartsWith("~") ?? true)
            {
                base.Process(context, output);
                return;
            }

            var basePath = _httpContextAccessor.HttpContext?.Request.PathBase ?? string.Empty;

            if (_hostingEnvironment.IsProduction()
                && !href.EndsWith(".min.js")
                && href.EndsWith(".js"))
            {
                href = href.Insert(href.LastIndexOf(".js", StringComparison.InvariantCultureIgnoreCase), ".min");
            }

            output.Attributes.Add(new TagHelperAttribute("src", basePath + href));

            if (_hostingEnvironment.IsProduction())
            {
                output.Attributes.Remove(output.Attributes[AbpSrcAttributeName]);
            }

            base.Process(context, output);
        }
    }
}
