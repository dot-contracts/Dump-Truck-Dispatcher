using System.Linq;
using Abp.Configuration;
using Abp.Localization;
using Abp.Net.Mail;
using DispatcherWeb.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace DispatcherWeb.Migrations.Seed.Host
{
    public class DefaultSettingsCreator
    {
        private readonly DispatcherWebDbContext _context;

        public DefaultSettingsCreator(DispatcherWebDbContext context)
        {
            _context = context;
        }

        public void Create(int? tenantId)
        {
            //Emailing
            AddSettingIfNotExists(EmailSettingNames.DefaultFromAddress, "admin@mydomain.com", tenantId);
            AddSettingIfNotExists(EmailSettingNames.DefaultFromDisplayName, "mydomain.com mailer", tenantId);

            //Languages
            AddSettingIfNotExists(LocalizationSettingNames.DefaultLanguage, "en", tenantId);
        }

        private void AddSettingIfNotExists(string name, string value, int? tenantId = null)
        {
            if (_context.Settings.IgnoreQueryFilters().Any(s => s.Name == name && s.TenantId == tenantId && s.UserId == null))
            {
                return;
            }

            _context.Settings.Add(new Setting(tenantId, null, name, value));
            _context.SaveChanges();
        }
    }
}
