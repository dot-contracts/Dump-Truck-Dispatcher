using System;
using System.Text;
using Abp.Dependency;
using Abp.IO.Extensions;
using Abp.Reflection.Extensions;

namespace DispatcherWeb.Emailing
{
    public class EmailTemplateProvider : IEmailTemplateProvider, ISingletonDependency
    {
        private string _defaultTemplate;

        public EmailTemplateProvider()
        {
        }

        public string GetDefaultTemplate(int? tenantId)
        {
            if (!string.IsNullOrEmpty(_defaultTemplate))
            {
                return _defaultTemplate;
            }

            using (var stream = typeof(EmailTemplateProvider).GetAssembly().GetManifestResourceStream("DispatcherWeb.Emailing.EmailTemplates.default.html"))
            {
                var bytes = stream.GetAllBytes();
                var template = Encoding.UTF8.GetString(bytes, 3, bytes.Length - 3);
                template = template.Replace("{THIS_YEAR}", DateTime.Now.Year.ToString());
                _defaultTemplate = template;
                return _defaultTemplate;
            }
        }
    }
}
