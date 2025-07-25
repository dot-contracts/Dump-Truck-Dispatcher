using System;
using System.Globalization;
using System.Text;
using System.Threading.Tasks;
using Abp.Configuration;
using Abp.Dependency;
using Abp.Extensions;
using Abp.Timing;
using Abp.Timing.Timezone;
using Abp.Web.Timing;

namespace DispatcherWeb.Web.Timing
{
    public class DispatcherWebTimingScriptManager : ITimingScriptManager, ITransientDependency
    {
        private readonly ISettingManager _settingManager;

        public DispatcherWebTimingScriptManager(
            ISettingManager settingManager
        )
        {
            _settingManager = settingManager;
        }

        public async Task<string> GetScriptAsync()
        {
            var script = new StringBuilder();

            script.AppendLine("(function(){");

            script.AppendLine("    abp.clock.provider = abp.timing." + Clock.Provider.GetType().Name.ToCamelCase() + " || abp.timing.localClockProvider;");
            script.AppendLine("    abp.clock.provider.supportsMultipleTimezone = " + Clock.SupportsMultipleTimezone.ToString().ToLower(CultureInfo.InvariantCulture) + ";");

            //if (Clock.SupportsMultipleTimezone)
            //{
            script.AppendLine("    abp.timing.timeZoneInfo = " + await GetUsersTimezoneScriptsAsync());
            //}

            script.Append("})();");

            return script.ToString();
        }

        private async Task<string> GetUsersTimezoneScriptsAsync()
        {
            var timezoneId = await _settingManager.GetSettingValueAsync(TimingSettingNames.TimeZone);
            var timezone = TimeZoneInfo.FindSystemTimeZoneById(timezoneId);

            return " {" +
                   "        windows: {" +
                   "            timeZoneId: '" + timezoneId + "'," +
                   "            baseUtcOffsetInMilliseconds: '" + timezone.BaseUtcOffset.TotalMilliseconds + "'," +
                   "            currentUtcOffsetInMilliseconds: '" + timezone.GetUtcOffset(Clock.Now).TotalMilliseconds + "'," +
                   "            isDaylightSavingTimeNow: '" + timezone.IsDaylightSavingTime(Clock.Now) + "'" +
                   "        }," +
                   "        iana: {" +
                   "            timeZoneId:'" + TimezoneHelper.WindowsToIana(timezoneId) + "'" +
                   "        }," +
                   "    }";
        }
    }
}
