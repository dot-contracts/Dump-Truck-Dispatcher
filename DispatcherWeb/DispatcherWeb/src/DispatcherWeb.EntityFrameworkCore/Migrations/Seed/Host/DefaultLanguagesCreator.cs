using System.Collections.Generic;
using System.Linq;
using Abp.Localization;
using DispatcherWeb.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace DispatcherWeb.Migrations.Seed.Host
{
    public class DefaultLanguagesCreator
    {
        public static List<ApplicationLanguage> InitialLanguages => GetInitialLanguages(null);

        private readonly DispatcherWebDbContext _context;

        private static List<ApplicationLanguage> GetInitialLanguages(int? tenantId)
        {
            return new List<ApplicationLanguage>
            {
                new ApplicationLanguage { Name = "en", DisplayName = "English", Icon = "famfamfam-flags us"},
                new ApplicationLanguage { Name = "es-MX", DisplayName = "Español (México)", Icon = "famfamfam-flags mx" },
                new ApplicationLanguage { Name = "es", DisplayName = "Español (Spanish)", Icon = "famfamfam-flags es" },
            };
        }

        public DefaultLanguagesCreator(DispatcherWebDbContext context)
        {
            _context = context;
        }

        public void Create(int? tenantId)
        {
            CreateLanguages(tenantId);
        }

        private void CreateLanguages(int? tenantId)
        {
            foreach (var language in GetInitialLanguages(tenantId))
            {
                AddLanguageIfNotExists(language);
            }
        }

        private void AddLanguageIfNotExists(ApplicationLanguage language)
        {
            if (_context.Languages.IgnoreQueryFilters().Any(l => l.Name == language.Name))
            {
                return;
            }

            _context.Languages.Add(language);

            _context.SaveChanges();
        }
    }
}
