using System;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Razor.TagHelpers;
using Microsoft.Extensions.Hosting;

namespace DispatcherWeb.Web.TagHelpers
{
    [HtmlTargetElement("link", Attributes = AbpHrefAttributeName)]
    public class DispatcherWebLinkHrefTagHelper : TagHelper
    {
        private const string AbpHrefAttributeName = "abp-href";

        private readonly IWebHostEnvironment _hostingEnvironment;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public override int Order => -1000 - 1;

        public DispatcherWebLinkHrefTagHelper(
            IWebHostEnvironment hostingEnvironment,
            IHttpContextAccessor httpContextAccessor)
        {
            _hostingEnvironment = hostingEnvironment;
            _httpContextAccessor = httpContextAccessor;
        }

        public override void Process(TagHelperContext context, TagHelperOutput output)
        {
            if (output.Attributes["abp-ignore-href-modification"]?.Value.ToString() == "true")
            {
                base.Process(context, output);
                return;
            }

            var abpHref = output.Attributes[AbpHrefAttributeName].Value;
            if (abpHref is HtmlString or string)
            {
                var href = abpHref.ToString();
                if (href?.StartsWith("~") ?? true)
                {
                    base.Process(context, output);
                    return;
                }

                var basePath = _httpContextAccessor.HttpContext?.Request.PathBase ?? string.Empty;

                if (_hostingEnvironment.IsProduction()
                    && !href.EndsWith(".min.css")
                    && href.EndsWith(".css"))
                {
                    href = href.Insert(href.LastIndexOf(".css", StringComparison.InvariantCultureIgnoreCase), ".min");
                }

                output.Attributes.Add(new TagHelperAttribute("href", basePath + href));

                if (_hostingEnvironment.IsProduction())
                {
                    output.Attributes.Remove(output.Attributes[AbpHrefAttributeName]);
                }
            }

            base.Process(context, output);
        }
    }
}
