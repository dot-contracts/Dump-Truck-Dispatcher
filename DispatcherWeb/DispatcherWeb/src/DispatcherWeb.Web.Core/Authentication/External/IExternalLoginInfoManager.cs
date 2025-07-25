using System.Collections.Generic;
using System.Security.Claims;
using Abp.Dependency;
using Microsoft.AspNetCore.Identity;

namespace DispatcherWeb.Web.Authentication.External
{
    public interface IExternalLoginInfoManager : ITransientDependency
    {
        string GetUserNameFromClaims(List<Claim> claims);

        (string name, string surname) GetNameAndSurnameFromClaims(List<Claim> claims, IdentityOptions identityOptions);
    }
}
