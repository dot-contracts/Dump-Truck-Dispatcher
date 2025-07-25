using Abp.Configuration.Startup;
using Abp.Localization.Dictionaries;
using Abp.Localization.Dictionaries.Xml;
using Abp.Reflection.Extensions;

namespace DispatcherWeb.Localization
{
    public static class DispatcherWebLocalizationConfigurer
    {
        public static void Configure(ILocalizationConfiguration localizationConfiguration)
        {
            localizationConfiguration.Sources.Add(
                new DictionaryBasedLocalizationSource(
                    DispatcherWebConsts.LocalizationSourceName,
                    new XmlEmbeddedFileLocalizationDictionaryProvider(
                        typeof(DispatcherWebLocalizationConfigurer).GetAssembly(),
                        "DispatcherWeb.Localization.DispatcherWeb"
                    )
                )
            );

            localizationConfiguration.Sources.Extensions.Add(
                new Abp.Localization.Sources.LocalizationSourceExtensionInfo(
                    "Abp",
                    new XmlEmbeddedFileLocalizationDictionaryProvider(
                        typeof(DispatcherWebLocalizationConfigurer).GetAssembly(),
                        "DispatcherWeb.Localization.Abp"
                    )
                )
            );
        }
    }
}