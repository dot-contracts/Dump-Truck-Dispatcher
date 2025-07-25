using System;

namespace DispatcherWeb.Infrastructure.Extensions
{
    public static class GuidExtensions
    {
        public static string ToShortGuid(this Guid guid)
        {
            return new ShortGuid(guid);
        }
    }
}
