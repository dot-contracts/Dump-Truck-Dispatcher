using System;
using System.Globalization;
using Abp;
using Abp.Localization;
using Abp.Localization.Sources;

namespace DispatcherWeb.Core.Localization
{
    public class LocalizationHelper
    {
        private ILocalizationSource _localizationSource;

        public ILocalizationManager LocalizationManager { get; set; }

        public string LocalizationSourceName { get; set; }

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

        public virtual string L(string name)
        {
            return LocalizationSource.GetString(name);
        }

        public virtual string L(string name, params object[] args)
        {
            return LocalizationSource.GetString(name, args);
        }

        public virtual string L(string name, CultureInfo culture)
        {
            return LocalizationSource.GetString(name, culture);
        }

        public virtual string L(string name, CultureInfo culture, params object[] args)
        {
            return LocalizationSource.GetString(name, culture, args);
        }
    }
}
