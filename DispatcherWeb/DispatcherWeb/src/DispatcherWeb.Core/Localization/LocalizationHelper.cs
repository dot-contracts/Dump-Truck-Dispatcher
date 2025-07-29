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

                // Defensive check for LocalizationManager
                if (LocalizationManager == null || LocalizationManager == NullLocalizationManager.Instance)
                {
                    // Return a fallback localization source
                    return new NullLocalizationSource(LocalizationSourceName);
                }

                try
                {
                    if (_localizationSource == null || _localizationSource.Name != LocalizationSourceName)
                    {
                        _localizationSource = LocalizationManager.GetSource(LocalizationSourceName);
                    }

                    return _localizationSource;
                }
                catch (Exception)
                {
                    // Return a fallback localization source if there's an error
                    return new NullLocalizationSource(LocalizationSourceName);
                }
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
            try
            {
                return LocalizationSource.GetString(name);
            }
            catch (Exception)
            {
                // Return the key name as fallback
                return name;
            }
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
            try
            {
                return LocalizationSource.GetString(name, args);
            }
            catch (Exception)
            {
                // Return the key name as fallback
                return name;
            }
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
            try
            {
                return LocalizationSource.GetString(name, culture);
            }
            catch (Exception)
            {
                // Return the key name as fallback
                return name;
            }
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
            try
            {
                return LocalizationSource.GetString(name, culture, args);
            }
            catch (Exception)
            {
                // Return the key name as fallback
                return name;
            }
        }
    }
}
