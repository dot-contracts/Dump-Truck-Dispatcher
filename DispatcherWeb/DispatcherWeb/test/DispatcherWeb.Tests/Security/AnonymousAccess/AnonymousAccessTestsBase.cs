using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using Abp.Application.Services;
using DispatcherWeb.Authorization;
using DispatcherWeb.Tests.Security.AnonymousAccess.Dto;
using Xunit;

namespace DispatcherWeb.Tests.Security.AnonymousAccess
{
    public abstract class AnonymousAccessTestsBase<TAppServiceBase, TAuthorizeAttribute, TAnonymousAttribute>
    {
        protected abstract string[] GetAssemblyNames();

        protected virtual BindingFlags GetMethodBindingFlags()
        {
            return BindingFlags.Public | BindingFlags.Instance;
        }

        [Fact]
        public virtual void TestImplicitAccessPermissions()
        {
            var result = "";

            var assemblyNames = GetAssemblyNames();

            var assemblies = assemblyNames.Select(assemblyName =>
            {
                var assembly = Assembly.Load(assemblyName);
                var appServices = assembly.GetTypes().Where(t =>
                    t.IsSubclassOf(typeof(TAppServiceBase))
                    && !t.IsAbstract
                );

                return new
                {
                    AssemblyName = assemblyName,
                    Assembly = assembly,
                    AppServices = appServices,
                    AnonymousAccessMethods = new List<string>(),
                    NoPermissionRequiredMethods = new List<string>(),
                };
            }).ToList();


            //Explicit Anonymous Access

            var explicitlyAnonymousTestingResult = "";
            foreach (var assembly in assemblies)
            {
                var assemblyResult = GetExplicitlyAnonymousMethods(assembly.AppServices);
                assembly.AnonymousAccessMethods.AddRange(assemblyResult);
                explicitlyAnonymousTestingResult += FormatAssemblyRunResult(assembly.AssemblyName, assemblyResult,
                    AnonymousAccessExceptions.ExplicitAnonymousAccess);
            }
            result += FormatTestRunResult("Anonymous users", "have explicitly defined access to the following methods",
                explicitlyAnonymousTestingResult, result);


            //Implicit Anonymous Access

            var implicitAnonymousTestingResult = "";
            foreach (var assembly in assemblies)
            {
                var assemblyResult = GetImplicitlyAnonymousMethods(assembly.AppServices)
                    .Except(assembly.AnonymousAccessMethods)
                    .ToList();
                assembly.AnonymousAccessMethods.AddRange(assemblyResult);
                implicitAnonymousTestingResult += FormatAssemblyRunResult(assembly.AssemblyName, assemblyResult,
                    AnonymousAccessExceptions.ImplicitAnonymousAccess);
            }
            result += FormatTestRunResult("Anonymous users", implicitAnonymousTestingResult, result);


            //Authenticated Access With No Permissions Required

            var noPermissionTestingResult = "";
            foreach (var assembly in assemblies)
            {
                var assemblyResult = GetPublicMethodsAvailableToPortalUsers(assembly.AppServices, "")
                    .Except(assembly.AnonymousAccessMethods)
                    .ToList();
                assembly.NoPermissionRequiredMethods.AddRange(assemblyResult);
                noPermissionTestingResult += FormatAssemblyRunResult(assembly.AssemblyName, assemblyResult,
                    AnonymousAccessExceptions.NoPermissionRequired);
            }
            result += FormatTestRunResult("All authenticated users (no permissions required)", noPermissionTestingResult, result);


            //Implicit Lease Hauler Portal Access

            //e.g. an app service having AbpAuthorize("Trucks", "LeaseHaulerPortal_Trucks") attribute,
            //but then one of the public methods not having any explicit AbpAuthorize attributes,
            //thus implicitly allowing access to LeaseHaulerPortal users to this method
            var leaseHaulerPortalTestingResult = "";
            foreach (var assembly in assemblies)
            {
                var assemblyResult = GetPublicMethodsAvailableToPortalUsers(assembly.AppServices, AppPermissions.LeaseHaulerPortal)
                    .Except(assembly.AnonymousAccessMethods)
                    .Except(assembly.NoPermissionRequiredMethods)
                    .ToList();
                leaseHaulerPortalTestingResult += FormatAssemblyRunResult(assembly.AssemblyName, assemblyResult,
                    AnonymousAccessExceptions.ImplicitLeaseHaulerPortalAccess);
            }
            result += FormatTestRunResult("Lease Hauler Portal users", leaseHaulerPortalTestingResult, result);


            //Customer Portal Access

            var customerPortalTestingResult = "";
            foreach (var assembly in assemblies)
            {
                var assemblyResult = GetPublicMethodsAvailableToPortalUsers(assembly.AppServices, AppPermissions.CustomerPortal)
                    .Except(assembly.AnonymousAccessMethods)
                    .Except(assembly.NoPermissionRequiredMethods)
                    .ToList();
                customerPortalTestingResult += FormatAssemblyRunResult(assembly.AssemblyName, assemblyResult,
                    AnonymousAccessExceptions.ImplicitCustomerPortalAccess);
            }
            result += FormatTestRunResult("Customer Portal users", customerPortalTestingResult, result);

            Assert.True(string.IsNullOrEmpty(result), result);
        }

        private List<string> GetPublicMethodsAvailableToPortalUsers(IEnumerable<Type> appServices, string portalPermissionPrefix)
        {
            //get all public methods from all app services that do not have AbpAuthorize(permissions) attribute,
            //excluding methods of app services that have AbpAuthorize(permissions) attribute (as long as those do not include LeaseHaulerPortal permissions)
            //
            //in other words, we'll get all unprotected methods that are available to lease hauler portal users

            var unprotectedMethodsWithAuthButNoPermissions = new List<string>();

            foreach (var appService in appServices)
            {
                var appServiceAuthAttributes = appService.CustomAttributes
                    .Where(a => a.AttributeType == typeof(TAuthorizeAttribute) && a.ConstructorArguments.Any())
                    .ToList();

                if (appServiceAuthAttributes.Any()
                    && appServiceAuthAttributes.All(a =>
                    {
                        var arguments = ((IReadOnlyCollection<CustomAttributeTypedArgument>)a.ConstructorArguments[0].Value).Select(x => (string)x.Value).ToArray();
                        return arguments.Any() && (string.IsNullOrEmpty(portalPermissionPrefix) || arguments.All(a => !a.StartsWith(portalPermissionPrefix)));
                    })
                )
                {
                    continue;
                }

                var methods = appService.GetMethods(GetMethodBindingFlags())
                    .Where(m => !IsIgnoredMethod(m)
                        && !HasDisabledRemoteService(m, appService)
                        && !m.CustomAttributes.Any(a =>
                            a.AttributeType == typeof(TAuthorizeAttribute)
                            && a.ConstructorArguments.Any(c => ((IReadOnlyCollection<CustomAttributeTypedArgument>)c.Value).Any())
                        )
                        && !m.CustomAttributes.Any(a => a.AttributeType == typeof(TAnonymousAttribute))
                    );

                foreach (var method in methods)
                {
                    unprotectedMethodsWithAuthButNoPermissions.Add($"{appService.Name}.{method.Name}");
                }
            }

            return unprotectedMethodsWithAuthButNoPermissions;
        }

        private List<string> GetImplicitlyAnonymousMethods(IEnumerable<Type> appServices)
        {
            //get all public methods from all app services that do not have AbpAuthorize attribute,
            //excluding methods of app services that have AbpAuthorize attribute and methods that have AbpAllowAnonymous attribute (since those are explicitly allowed)

            var unprotectedMethodsWithNoAuth = new List<string>();

            foreach (var appService in appServices)
            {
                if (appService.CustomAttributes.Any(a => a.AttributeType == typeof(TAuthorizeAttribute)))
                {
                    continue;
                }

                var methods = appService.GetMethods(GetMethodBindingFlags())
                    .Where(m => !IsIgnoredMethod(m)
                        && !HasDisabledRemoteService(m, appService)
                        && !m.CustomAttributes.Any(a => a.AttributeType == typeof(TAuthorizeAttribute))
                        && !m.CustomAttributes.Any(a => a.AttributeType == typeof(TAnonymousAttribute))
                    );

                foreach (var method in methods)
                {
                    unprotectedMethodsWithNoAuth.Add($"{appService.Name}.{method.Name}");
                }
            }

            return unprotectedMethodsWithNoAuth;
        }


        // This method is not a part of the test, but we might still need it sometimes
        private List<string> GetExplicitlyAnonymousMethods(IEnumerable<Type> appServices)
        {
            var result = new List<string>();

            foreach (var appService in appServices)
            {
                var methods = appService.GetMethods(GetMethodBindingFlags())
                    .Where(m => !IsIgnoredMethod(m)
                        && !HasDisabledRemoteService(m, appService)
                    );
                if (!appService.CustomAttributes.Any(a => a.AttributeType == typeof(TAnonymousAttribute)))
                {
                    methods = methods.Where(m => m.CustomAttributes.Any(a => a.AttributeType == typeof(TAnonymousAttribute)));
                }

                foreach (var method in methods)
                {
                    result.Add($"{appService.Name}.{method.Name}");
                }
            }

            return result;
        }

        private static bool IsIgnoredMethod(MethodInfo method)
        {
            return method.IsSpecialName
                || method.DeclaringType == typeof(object);
        }

        private bool HasDisabledRemoteService(MethodInfo methodInfo, Type appService)
        {
            if (appService.CustomAttributes.Any(IsDisableRemoteServiceAttribute))
            {
                return !methodInfo.CustomAttributes.Any(IsEnableRemoteServiceAttribute);
            }
            else
            {
                return methodInfo.CustomAttributes.Any(IsDisableRemoteServiceAttribute);
            }
        }

        private bool IsDisableRemoteServiceAttribute(CustomAttributeData attributeData)
        {
            return attributeData.AttributeType == typeof(RemoteServiceAttribute)
                   && attributeData.ConstructorArguments.Any()
                   && (bool)attributeData.ConstructorArguments[0].Value == false;
        }

        private bool IsEnableRemoteServiceAttribute(CustomAttributeData attributeData)
        {
            return attributeData.AttributeType == typeof(RemoteServiceAttribute)
                   && (!attributeData.ConstructorArguments.Any()
                       || (bool)attributeData.ConstructorArguments[0].Value == true);
        }

        private static string FormatAssemblyRunResult(string assemblyName, List<string> methods, AnonymousAccessExceptionDto exceptions)
        {
            exceptions.Filter(assemblyName, methods);

            if (!methods.Any())
            {
                return null;
            }
            var result = new StringBuilder();
            result.AppendLine(assemblyName);
            result.AppendLine("------------------------------------");
            result.AppendLine(string.Join("\r\n", methods));
            result.AppendLine();
            return result.ToString();
        }

        private static string FormatTestRunResult(string testUserGroupDescription, string testResult, string accumulatedResult)
        {
            return FormatTestRunResult(
                testUserGroupDescription,
                "have implicitly defined access to the following methods (and also to everything listed above)",
                testResult,
                accumulatedResult);
        }

        private static string FormatTestRunResult(string testUserGroupDescription, string allowedActionDescription, string currentTestResult, string accumulatedResult)
        {
            if (string.IsNullOrEmpty(currentTestResult))
            {
                if (string.IsNullOrEmpty(accumulatedResult))
                {
                    return null;
                }
                else
                {
                    currentTestResult = "No additional methods found\r\n";
                }
            }
            return "############################################\r\n"
                    + $"{testUserGroupDescription} {allowedActionDescription}:\r\n"
                    + "############################################\r\n"
                    + currentTestResult + "\r\n";

        }
    }
}
