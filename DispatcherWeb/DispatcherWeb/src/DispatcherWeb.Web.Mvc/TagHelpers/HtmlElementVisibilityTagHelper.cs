using System.Linq;
using System.Text.Encodings.Web;
using Abp.Extensions;
using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Mvc.TagHelpers;
using Microsoft.AspNetCore.Razor.TagHelpers;

namespace DispatcherWeb.Web.TagHelpers
{
    [HtmlTargetElement(Attributes = AbpHiddenAttributeName)]
    public class HtmlElementVisibilityTagHelper : TagHelper
    {
        private const string AbpHiddenAttributeName = "abp-hidden";

        public HtmlElementVisibilityTagHelper()
        {
        }

        public override void Process(TagHelperContext context, TagHelperOutput output)
        {
            if (!output.Attributes.Any(x => x.Name == AbpHiddenAttributeName))
            {
                base.Process(context, output);
                return;
            }

            var abpHidden = output.Attributes[AbpHiddenAttributeName].Value;

            if (abpHidden is HtmlString
                || abpHidden is string)
            {
                var hidden = abpHidden.ToString();
                if (hidden.IsIn("true", "", AbpHiddenAttributeName))
                {
                    output.AddClass("d-none", HtmlEncoder.Default);
                }
            }

            base.Process(context, output);
        }
    }
}
