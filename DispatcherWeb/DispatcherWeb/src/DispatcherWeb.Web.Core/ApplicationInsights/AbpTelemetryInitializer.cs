using System;
using Abp.Runtime.Session;
using DispatcherWeb.Runtime.Session;
using Microsoft.ApplicationInsights.Channel;
using Microsoft.ApplicationInsights.DataContracts;
using Microsoft.ApplicationInsights.Extensibility;
using Microsoft.Extensions.DependencyInjection;

public class AbpTelemetryInitializer : ITelemetryInitializer
{
    private readonly IServiceProvider _serviceProvider;

    private IAbpSession AbpSession { get; set; }

    public AbpTelemetryInitializer(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    public void Initialize(ITelemetry telemetry)
    {
        if (telemetry is not ISupportProperties supportProperties)
        {
            return;
        }

        if (AbpSession == null)
        {
            if (!AspNetZeroAbpSession.SessionIsReady)
            {
                return;
            }

            AbpSession = _serviceProvider.GetService<IAbpSession>();
            if (AbpSession == null)
            {
                return;
            }
        }

        var tenantId = AbpSession.UnverifiedTenantId;
        if (tenantId.HasValue)
        {
            supportProperties.Properties["TenantId"] = tenantId?.ToString();
        }

        var userId = AbpSession.UserId;
        if (userId.HasValue)
        {
            supportProperties.Properties["UserId"] = userId.ToString();
            telemetry.Context.User.AuthenticatedUserId = userId.ToString();
        }
    }
}
