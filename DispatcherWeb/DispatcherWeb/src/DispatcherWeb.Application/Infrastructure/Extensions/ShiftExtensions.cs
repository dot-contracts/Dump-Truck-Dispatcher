using System.Linq;

namespace DispatcherWeb.Infrastructure.Extensions
{
    public static class ShiftExtensions
    {
        public static Shift?[] ToNullableArrayWithNullElementIfEmpty(this Shift[] shifts)
        {
            return shifts?.ToNullIfEmpty()?.Select(x => (Shift?)x).ToArray() ?? new Shift?[] { null };
        }

        private static Shift[] ToNullIfEmpty(this Shift[] shifts)
        {
            return shifts.Length != 0 ? shifts : null;
        }
    }
}
