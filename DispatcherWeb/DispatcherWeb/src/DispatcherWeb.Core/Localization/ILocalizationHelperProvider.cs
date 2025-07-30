using DispatcherWeb.Core.Localization;

namespace DispatcherWeb.Localization
{
    public interface ILocalizationHelperProvider
    {
        LocalizationHelper LocalizationHelper { get; }
    }
}
