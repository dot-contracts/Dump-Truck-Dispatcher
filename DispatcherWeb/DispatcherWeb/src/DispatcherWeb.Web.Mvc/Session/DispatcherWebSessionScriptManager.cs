using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Abp.Dependency;
using Abp.Web.Sessions;
using DispatcherWeb.Authorization.Users.Cache;
using DispatcherWeb.Infrastructure;
using DispatcherWeb.Runtime.Session;
using DispatcherWeb.Web.Utils;

namespace DispatcherWeb.Web.Session
{
    public class DispatcherWebSessionScriptManager : ISessionScriptManager, ISingletonDependency
    {
        private readonly IUserOrganizationUnitCache _userOrganizationUnitCache;
        protected IExtendedAbpSession Session { get; set; }

        public DispatcherWebSessionScriptManager(
            IExtendedAbpSession session,
            IUserOrganizationUnitCache userOrganizationUnitCache
        )
        {
            Session = session;
            _userOrganizationUnitCache = userOrganizationUnitCache;
        }

        public async Task<string> GetScriptAsync()
        {
            var script = new StringBuilder();

            script.AppendLine("(function(){");
            script.AppendLine();

            script.AppendLine(@"    abp.session ??= {};");
            script.AppendLine($"    abp.session.userId = {FormatNullableInt(Session.UserId)};");
            script.AppendLine($"    abp.session.tenantId = {FormatNullableInt(await Session.GetTenantIdOrNullAsync())};");
            script.AppendLine($"    abp.session.impersonatorUserId = {FormatNullableInt(Session.ImpersonatorUserId)};");
            script.AppendLine($"    abp.session.impersonatorTenantId = {FormatNullableInt(Session.ImpersonatorTenantId)};");
            script.AppendLine($"    abp.session.multiTenancySide = {(int)await Session.GetMultiTenancySideAsync()};");

            script.AppendLine($"    abp.session.officeId = {FormatNullableInt(Session.OfficeId)};");
            script.AppendLine($"    abp.session.officeName = {HtmlHelper.EscapeJsString(Session.OfficeName)};");
            script.AppendLine($"    abp.session.officeCopyChargeTo = {(Session.OfficeCopyChargeTo ? "true" : "false")};");
            script.AppendLine($"    abp.session.customerId = {FormatNullableInt(Session.CustomerId)};");
            script.AppendLine($"    abp.session.customerName = {HtmlHelper.EscapeJsString(Session.CustomerName)};");
            script.AppendLine($"    abp.session.leaseHaulerId = {FormatNullableInt(Session.LeaseHaulerId)};");
            script.AppendLine($"    abp.session.userName = {HtmlHelper.EscapeJsString(Session.UserName)};");
            script.AppendLine($"    abp.session.userEmail = {HtmlHelper.EscapeJsString(Session.UserEmail)};");

            var organizationUnits = Session.UserId.HasValue
                ? await _userOrganizationUnitCache.GetUserOrganizationUnitsAsync(Session.UserId.Value)
                : new List<UserOrganizationUnitCacheItem>();

            script.AppendLine("    abp.session.organizationUnitIds = [");
            foreach (var organizationUnit in organizationUnits)
            {
                script.AppendLine($"        {organizationUnit.OrganizationUnitId},");
            }
            script.AppendLine("    ];");

            script.AppendLine("    abp.session.officeIds = [");
            foreach (var organizationUnit in organizationUnits)
            {
                if (organizationUnit.OfficeId.HasValue)
                {
                    script.AppendLine($"        {organizationUnit.OfficeId},");
                }
            }
            script.AppendLine("    ];");

            var hasAccessToAllOffices = await _userOrganizationUnitCache.HasAccessToAllOffices();
            script.AppendLine($"    abp.session.hasAccessToAllOffices = {hasAccessToAllOffices.ToString().ToLower()};");

            //todo we should move these to a more static location, i.e. the second script that's supposed to get loaded once per app startup per user and then be cached
            script.AppendLine(@"    abp.entityStringFieldLengths ??= {};");
            script.AppendLine(@"    abp.entityStringFieldLengths.orderLine ??= {};");
            script.AppendLine($"    abp.entityStringFieldLengths.orderLine.jobNumber = {EntityStringFieldLengths.OrderLine.JobNumber};");
            script.AppendLine(@"    abp.entityStringFieldLengths.order ??= {};");
            script.AppendLine($"    abp.entityStringFieldLengths.order.poNumber = {EntityStringFieldLengths.Order.PoNumber};");
            script.AppendLine(@"    abp.entityStringFieldLengths.insurance ??= {};");
            script.AppendLine($"    abp.entityStringFieldLengths.insurance.issuedBy = {EntityStringFieldLengths.Insurance.IssuedBy};");
            script.AppendLine($"    abp.entityStringFieldLengths.insurance.brokerName = {EntityStringFieldLengths.Insurance.BrokerName};");
            script.AppendLine($"    abp.entityStringFieldLengths.insurance.comments = {EntityStringFieldLengths.Insurance.Comments};");
            script.AppendLine($"    abp.entityStringFieldLengths.insurance.fileName = {EntityStringFieldLengths.Insurance.FileName};");
            script.AppendLine($"    abp.entityStringFieldLengths.insurance.insuranceTypeName = {EntityStringFieldLengths.Insurance.InsuranceTypeName};");
            script.AppendLine(@"    abp.entityStringFieldLengths.charge ??= {};");
            script.AppendLine($"    abp.entityStringFieldLengths.charge.description = {EntityStringFieldLengths.Charge.Description};");

            script.AppendLine();
            script.Append("})();");

            return script.ToString();
        }

        private static string FormatNullableInt(int? value)
        {
            return value?.ToString() ?? "null";
        }

        private static string FormatNullableInt(long? value)
        {
            return value?.ToString() ?? "null";
        }
    }
}
