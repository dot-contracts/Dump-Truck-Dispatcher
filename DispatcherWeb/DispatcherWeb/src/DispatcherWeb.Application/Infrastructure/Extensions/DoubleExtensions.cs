using System;

namespace DispatcherWeb.Infrastructure.Extensions
{
    public static class DoubleExtensions
    {
        public static double? Round(this double? value, int digits)
        {
            if (value == null)
            {
                return null;
            }

            return Math.Round(value.Value, digits);
        }
    }
}
