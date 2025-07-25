using System;
using System.Collections.Concurrent;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Abp.Dependency;
using Abp.Extensions;
using Abp.Threading;
using DispatcherWeb.Configuration;
using DispatcherWeb.Infrastructure.Extensions;
using DispatcherWeb.Runtime.Session;
using IdentityServer4.Stores;
using Microsoft.AspNetCore.Hosting;

namespace DispatcherWeb.Web.Resources
{
    public class ScriptPaths : ISingletonDependency
    {
        private readonly IWebHostEnvironment _hostingEnvironment;
        private readonly IAppConfigurationAccessor _configurationAccessor;
        private readonly IOfficeSettingsManager _officeSettingsManager;
        private readonly IClientStore _identityServerClientStore;

        private readonly ConcurrentDictionary<string, string> _scriptPaths;

        public IExtendedAbpSession Session { get; set; }

        public ScriptPaths(
            IWebHostEnvironment hostingEnvironment,
            IAppConfigurationAccessor configurationAccessor,
            IOfficeSettingsManager officeSettingsManager,
            IClientStore identityServerClientStore)
        {
            _hostingEnvironment = hostingEnvironment;
            _configurationAccessor = configurationAccessor;
            _officeSettingsManager = officeSettingsManager;
            _identityServerClientStore = identityServerClientStore;
            _scriptPaths = new ConcurrentDictionary<string, string>();
        }

        public async Task<string> GetHeartlandPublicKeyAsync()
        {
            var publicKey = await _officeSettingsManager.GetHeartlandPublicKeyAsync();
            return publicKey;
        }

        public string GoogleMapsApiKey
        {
            get
            {
                return _scriptPaths.GetOrAdd("GoogleMapsApiKey",
                    k => _configurationAccessor.Configuration["GoogleMaps:ApiKey"]);
            }
        }

        public string CookieDomain
        {
            get
            {
                return _scriptPaths.GetOrAdd("CookieDomain",
                    k => _configurationAccessor.Configuration.GetCookieDomain());
            }
        }

        public string SignalRRootAddress
        {
            get
            {
                return _scriptPaths.GetOrAdd("SignalRRootAddress",
                    k => _configurationAccessor.Configuration["App:SignalRRootAddress"].EnsureEndsWith('/'));
            }
        }

        public string SignalRHeartbeatInterval
        {
            get
            {
                return _scriptPaths.GetOrAdd("SignalRHeartbeatInterval",
                    k => _configurationAccessor.Configuration["SignalR:HeartbeatInterval"].NullIfEmpty() ?? "0");
            }
        }

        public string ReportCenter
        {
            get
            {
                return _scriptPaths.GetOrAdd("ReportCenter",
                    k =>
                    {
                        var client = AsyncHelper.RunSync(() => _identityServerClientStore.FindClientByIdAsync("reportcenter"));
                        var signInUrl = client.RedirectUris.First(u => u.EndsWith("signin-oidc"));
                        var signinUri = new Uri(signInUrl);
                        return $"{signinUri.Scheme}://{signinUri.Authority}".EnsureEndsWith('/');
                    });
            }
        }

        public string LogoPathSuffix
        {
            get
            {
                return _scriptPaths.GetOrAdd("LogoPathSuffix",
                    k => _configurationAccessor.Configuration["LogoPathSuffix"]);
            }
        }

        #region jQuery Validation

        public string JQuery_Validation_Localization
        {
            get
            {
                return _scriptPaths.GetOrAdd("jquery-validation#" + CultureInfo.CurrentUICulture.Name, k =>
                {
                    var path = GetLocalizationFileForjQueryValidationOrNull(CultureInfo.CurrentUICulture.Name.ToLower().Replace("-", "_"))
                           ?? GetLocalizationFileForjQueryValidationOrNull(CultureInfo.CurrentUICulture.Name.Left(2).ToLower())
                           ?? @"Common/Scripts/_empty.js";

                    return "/" + path;
                });
            }
        }

        private string GetLocalizationFileForjQueryValidationOrNull(string cultureCode)
        {
            try
            {
                var relativeFilePath = @"lib/jquery-validation/dist/localization/messages_" + cultureCode + ".js";
                var physicalFilePath = GetPhysicalPath(relativeFilePath);
                if (File.Exists(physicalFilePath))
                {
                    return relativeFilePath;
                }
            }
            catch { }

            return null;
        }

        #endregion

        #region jQuery Timeago

        public string JQuery_Timeago_Localization
        {
            get
            {
                return _scriptPaths.GetOrAdd("jquery-timeago#" + CultureInfo.CurrentUICulture.Name, k =>
                {
                    var path = GetLocalizationFileForjQueryTimeagoOrNull(CultureInfo.CurrentUICulture.Name.ToLower().Replace("-", "_"))
                       ?? GetLocalizationFileForjQueryTimeagoOrNull(CultureInfo.CurrentUICulture.Name.Left(2).ToLower())
                       ?? "lib/timeago/locales/jquery.timeago.en.js";
                    return "/" + path;
                });
            }
        }

        private string GetLocalizationFileForjQueryTimeagoOrNull(string cultureCode)
        {
            try
            {
                var relativeFilePath = "lib/timeago/locales/jquery.timeago." + cultureCode + ".js";
                var physicalFilePath = GetPhysicalPath(relativeFilePath);
                if (File.Exists(physicalFilePath))
                {
                    return relativeFilePath;
                }
            }
            catch { }

            return null;
        }

        #endregion

        #region Select2

        public string Select2_Localization
        {
            get
            {
                return _scriptPaths.GetOrAdd("select2#" + CultureInfo.CurrentUICulture.Name, k =>
                {
                    var path = GetLocalizationFileForSelect2(CultureInfo.CurrentUICulture.Name.ToLower())
                               ?? GetLocalizationFileForSelect2(CultureInfo.CurrentUICulture.Name.Left(2).ToLower())
                               ?? "lib/select2/dist/js/i18n/en.js";
                    return "/" + path;
                });
            }
        }

        private static string GetLocalizationFileForSelect2(string cultureCode)
        {
            try
            {
                foreach (var localizationFile in Select2LocalizationFileCultureCodeList)
                {
                    if (localizationFile.StartsWith(cultureCode))
                    {
                        return "lib/select2/dist/js/i18n/" + localizationFile + ".js";
                    }
                }
            }
            catch { }

            return null;
        }

        private static readonly string[] Select2LocalizationFileCultureCodeList =
        {
            "ar",
            "az",
            "bg",
            "ca",
            "cs",
            "da",
            "de",
            "el",
            "en",
            "es",
            "et",
            "eu",
            "fa",
            "fi",
            "fr",
            "gl",
            "he",
            "hi",
            "hr",
            "hu",
            "id",
            "is",
            "it",
            "ja",
            "km",
            "ko",
            "lt",
            "lv",
            "mk",
            "ms",
            "nb",
            "nl",
            "pl",
            "pt",
            "pt-BR",
            "ro",
            "ru",
            "sk",
            "sr",
            "sr-Cyrl",
            "sv",
            "th",
            "tr",
            "uk",
            "vi",
            "zh-CN",
            "zh-TW",
        };

        #endregion

        #region Helper methods

        private string GetPhysicalPath(string relativePath)
        {
            return Path.Combine(_hostingEnvironment.WebRootPath, relativePath);
        }

        #endregion
    }
}
