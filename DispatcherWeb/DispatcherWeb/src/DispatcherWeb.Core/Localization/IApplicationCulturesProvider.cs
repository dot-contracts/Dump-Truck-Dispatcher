using System.Globalization;

namespace DispatcherWeb.Localization
{
    public interface IApplicationCulturesProvider
    {
        CultureInfo[] GetAllCultures();
    }
}