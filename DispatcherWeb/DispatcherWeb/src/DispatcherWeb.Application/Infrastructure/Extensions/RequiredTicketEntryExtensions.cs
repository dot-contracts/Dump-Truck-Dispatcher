using System;

namespace DispatcherWeb.Infrastructure.Extensions
{
    public static class RequiredTicketEntryExtensions
    {
        public static bool IsRequireTicketInputVisible(this RequiredTicketEntryEnum requiredTicketEntry)
        {
            switch (requiredTicketEntry)
            {
                case RequiredTicketEntryEnum.None:
                case RequiredTicketEntryEnum.Always:
                    return false;
                case RequiredTicketEntryEnum.ByJobDefaultingToRequired:
                case RequiredTicketEntryEnum.ByJobDefaultingToNotRequired:
                    return true;
                default:
                    throw new ArgumentOutOfRangeException(nameof(requiredTicketEntry), requiredTicketEntry, null);
            }
        }

        public static bool GetRequireTicketDefaultValue(this RequiredTicketEntryEnum requiredTicketEntry)
        {
            switch (requiredTicketEntry)
            {
                case RequiredTicketEntryEnum.None:
                case RequiredTicketEntryEnum.ByJobDefaultingToNotRequired:
                    return false;
                case RequiredTicketEntryEnum.Always:
                case RequiredTicketEntryEnum.ByJobDefaultingToRequired:
                    return true;
                default:
                    throw new ArgumentOutOfRangeException(nameof(requiredTicketEntry), requiredTicketEntry, null);
            }
        }
    }
}
