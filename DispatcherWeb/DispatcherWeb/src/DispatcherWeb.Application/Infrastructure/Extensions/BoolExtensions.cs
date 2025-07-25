namespace DispatcherWeb.Infrastructure.Extensions
{
    public static class BoolExtensions
    {
        public static string ToYesNoString(this bool value)
        {
            return value ? "Yes" : "No";
        }
    }
}
