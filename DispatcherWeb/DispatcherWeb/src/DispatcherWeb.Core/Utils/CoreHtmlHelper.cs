using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace DispatcherWeb.Utils
{
    public static class CoreHtmlHelper
    {
        public static string Sanitize(string html)
        {
            return string.IsNullOrEmpty(html) ? "" : html.Replace("<", "&lt;").Replace(">", "&gt;").Replace("\"", "&quot;").Replace("'", "&#39;");
        }

        public static string EscapeJsString(string val)
        {
            if (val == null)
            {
                return "null";
            }
            return "'" + val.Replace(@"\", @"\\").Replace(@"'", @"\'") + "'";
        }

        public static string FormatInlineJsonObject(object val)
        {
            var str = JsonConvert.SerializeObject(val, new JsonSerializerSettings
            {
                ContractResolver = new CamelCasePropertyNamesContractResolver(),
                Formatting = Formatting.None,
            });

            return "JSON.parse(" + EscapeJsString(str) + ")";
        }
    }
}
