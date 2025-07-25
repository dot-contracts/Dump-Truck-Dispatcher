using System;

namespace DispatcherWeb.Infrastructure.Utilities
{
    public static class UnitConverter
    {
        public static double ConvertMetresToMiles(double metres)
        {
            return metres / 1000 * 0.621371192;
        }

        public static double ConvertMilesToMeters(double miles)
        {
            return Math.Round((double)miles * 1609.34, 2);
        }

        public static double ConvertSecondsToHours(double seconds)
        {
            return seconds / 3600;
        }

        public static decimal GetMiles(decimal value, WialonMeasureUnits units)
        {
            return Convert.ToDecimal(GetMiles(Convert.ToDouble(value), units));
        }

        public static double GetMiles(double value, WialonMeasureUnits units)
        {
            switch (units)
            {
                case WialonMeasureUnits.SI:
                    return ConvertMetresToMiles(value);
                default:
                case WialonMeasureUnits.US:
                    return value;
                case WialonMeasureUnits.Imperial:
                    return value;
                case WialonMeasureUnits.MetricWithGallons:
                    return ConvertMetresToMiles(value);
            }
        }
    }
}
