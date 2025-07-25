using System.Globalization;
using Abp;
using Abp.Dependency;
using Abp.Localization;
using Abp.Localization.Sources;

namespace DispatcherWeb.Localization
{
    public class LocalizationHelper : ISingletonDependency
    {
        private ILocalizationSource _localizationSource;

        public LocalizationHelper()
        {
            LocalizationManager = NullLocalizationManager.Instance;
            LocalizationSourceName = DispatcherWebConsts.LocalizationSourceName;
        }

        protected ILocalizationSource LocalizationSource
        {
            get
            {
                if (LocalizationSourceName == null)
                {
                    throw new AbpException("Must set LocalizationSourceName before, in order to get LocalizationSource");
                }

                if (_localizationSource == null || _localizationSource.Name != LocalizationSourceName)
                {
                    _localizationSource = LocalizationManager.GetSource(LocalizationSourceName);
                }

                return _localizationSource;
            }
        }

        protected string LocalizationSourceName { get; set; }

        public ILocalizationManager LocalizationManager { get; set; }


        //
        // Summary:
        //     Gets localized string for given key name and current language.
        //
        // Parameters:
        //   name:
        //     Key name
        //
        // Returns:
        //     Localized string
        public virtual string L(string name)
        {
            return LocalizationSource.GetString(name);
        }

        //
        // Summary:
        //     Gets localized string for given key name and current language with formatting
        //     strings.
        //
        // Parameters:
        //   name:
        //     Key name
        //
        //   args:
        //     Format arguments
        //
        // Returns:
        //     Localized string
        public virtual string L(string name, params object[] args)
        {
            return LocalizationSource.GetString(name, args);
        }

        //
        // Summary:
        //     Gets localized string for given key name and specified culture information.
        //
        // Parameters:
        //   name:
        //     Key name
        //
        //   culture:
        //     culture information
        //
        // Returns:
        //     Localized string
        public virtual string L(string name, CultureInfo culture)
        {
            return LocalizationSource.GetString(name, culture);
        }

        //
        // Summary:
        //     Gets localized string for given key name and current language with formatting
        //     strings.
        //
        // Parameters:
        //   name:
        //     Key name
        //
        //   culture:
        //     culture information
        //
        //   args:
        //     Format arguments
        //
        // Returns:
        //     Localized string
        public virtual string L(string name, CultureInfo culture, params object[] args)
        {
            return LocalizationSource.GetString(name, culture, args);
        }
    }
}
