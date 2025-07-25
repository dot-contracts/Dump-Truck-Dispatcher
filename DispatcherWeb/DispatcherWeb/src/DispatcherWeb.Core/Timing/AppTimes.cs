using System;
using Abp.Dependency;

namespace DispatcherWeb.Timing
{
    public class AppTimes : ISingletonDependency
    {
        public DateTime StartupTime { get; set; }
    }
}
