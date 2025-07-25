using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Abp.Application.Services.Dto;
using Abp.Authorization;
using Abp.Domain.Repositories;
using Abp.Localization;
using Abp.MultiTenancy;
using Abp.UI;
using DispatcherWeb.Authorization;
using DispatcherWeb.Localization.Dto;
using Microsoft.EntityFrameworkCore;

namespace DispatcherWeb.Localization
{
    [AbpAuthorize(AppPermissions.Pages_Administration_Languages)]
    public class LanguageAppService : DispatcherWebAppServiceBase, ILanguageAppService
    {
        private readonly IApplicationLanguageManager _applicationLanguageManager;
        private readonly IRepository<ApplicationLanguage> _languageRepository;
        private readonly IApplicationCulturesProvider _applicationCulturesProvider;

        public LanguageAppService(
            IApplicationLanguageManager applicationLanguageManager,
            IRepository<ApplicationLanguage> languageRepository,
            IApplicationCulturesProvider applicationCulturesProvider)
        {
            _applicationLanguageManager = applicationLanguageManager;
            _languageRepository = languageRepository;
            _applicationCulturesProvider = applicationCulturesProvider;
        }

        public async Task<GetLanguagesOutput> GetLanguages()
        {
            var languages =
                (await _applicationLanguageManager.GetLanguagesAsync())
                .Select(x => new ApplicationLanguageListDto
                {
                    Id = x.Id,
                    Name = x.Name,
                    DisplayName = x.DisplayName,
                    Icon = x.Icon,
                    IsDisabled = x.IsDisabled,
                })
                .OrderBy(l => l.DisplayName)
                .ToList();
            var defaultLanguage = await _applicationLanguageManager.GetDefaultLanguageOrNullAsync(await AbpSession.GetTenantIdOrNullAsync());

            return new GetLanguagesOutput(languages, defaultLanguage?.Name);
        }

        [AbpAuthorize(AppPermissions.Pages_Administration_Languages_Create,
            AppPermissions.Pages_Administration_Languages_Edit)]
        public async Task<GetLanguageForEditOutput> GetLanguageForEdit(NullableIdDto input)
        {
            ApplicationLanguageEditDto language = null;
            if (input.Id.HasValue)
            {
                language = await (await _languageRepository.GetQueryAsync())
                    .Where(x => x.Id == input.Id.Value)
                    .Select(x => new ApplicationLanguageEditDto
                    {
                        Id = x.Id,
                        Name = x.Name,
                        Icon = x.Icon,
                        IsEnabled = !x.IsDisabled,
                    }).FirstOrDefaultAsync();
            }

            var output = new GetLanguageForEditOutput
            {
                Language = language ?? new ApplicationLanguageEditDto(),
            };

            //Language names
            output.LanguageNames = _applicationCulturesProvider
                .GetAllCultures()
                .Select(c => new ComboboxItemDto(c.Name, c.EnglishName + " (" + c.Name + ")")
                { IsSelected = output.Language.Name == c.Name })
                .ToList();

            //Flags
            output.Flags = FamFamFamFlagsHelper
                .FlagClassNames
                .OrderBy(f => f)
                .Select(f => new ComboboxItemDto(f, FamFamFamFlagsHelper.GetCountryCode(f))
                { IsSelected = output.Language.Icon == f })
                .ToList();

            return output;
        }

        public async Task CreateOrUpdateLanguage(CreateOrUpdateLanguageInput input)
        {
            if (input.Language.Id.HasValue)
            {
                await UpdateLanguageAsync(input);
            }
            else
            {
                await CreateLanguageAsync(input);
            }
        }

        public async Task DeleteLanguage(EntityDto input)
        {
            var language = await _languageRepository.GetAsync(input.Id);
            await _applicationLanguageManager.RemoveAsync(await AbpSession.GetTenantIdOrNullAsync(), language.Name);
        }

        [AbpAuthorize(AppPermissions.Pages_Administration_Languages_ChangeDefaultLanguage)]
        public async Task SetDefaultLanguage(SetDefaultLanguageInput input)
        {
            await _applicationLanguageManager.SetDefaultLanguageAsync(
                await AbpSession.GetTenantIdOrNullAsync(),
                CultureHelper.GetCultureInfoByChecking(input.Name).Name
            );
        }

        [AbpAuthorize(AppPermissions.Pages_Administration_Languages_Create)]
        protected virtual async Task CreateLanguageAsync(CreateOrUpdateLanguageInput input)
        {
            if (await AbpSession.GetMultiTenancySideAsync() != MultiTenancySides.Host)
            {
                throw new UserFriendlyException(L("TenantsCannotCreateLanguage"));
            }

            var culture = CultureHelper.GetCultureInfoByChecking(input.Language.Name);

            await CheckLanguageIfAlreadyExists(culture.Name);

            await _applicationLanguageManager.AddAsync(
                new ApplicationLanguage
                {
                    Name = culture.Name,
                    DisplayName = culture.DisplayName,
                    Icon = input.Language.Icon,
                    IsDisabled = !input.Language.IsEnabled,
                }
            );
        }

        [AbpAuthorize(AppPermissions.Pages_Administration_Languages_Edit)]
        protected virtual async Task UpdateLanguageAsync(CreateOrUpdateLanguageInput input)
        {
            Debug.Assert(input.Language.Id != null, "input.Language.Id != null");

            var culture = CultureHelper.GetCultureInfoByChecking(input.Language.Name);

            await CheckLanguageIfAlreadyExists(culture.Name, input.Language.Id.Value);

            var language = await _languageRepository.GetAsync(input.Language.Id.Value);

            language.Name = culture.Name;
            language.DisplayName = culture.DisplayName;
            language.Icon = input.Language.Icon;
            language.IsDisabled = !input.Language.IsEnabled;

            await _applicationLanguageManager.UpdateAsync(await AbpSession.GetTenantIdOrNullAsync(), language);
        }

        private async Task CheckLanguageIfAlreadyExists(string languageName, int? expectedId = null)
        {
            var existingLanguage = (await _applicationLanguageManager.GetLanguagesAsync())
                .FirstOrDefault(l => l.Name == languageName);

            if (existingLanguage == null)
            {
                return;
            }

            if (expectedId != null && existingLanguage.Id == expectedId.Value)
            {
                return;
            }

            throw new UserFriendlyException(L("ThisLanguageAlreadyExists"));
        }
    }
}
