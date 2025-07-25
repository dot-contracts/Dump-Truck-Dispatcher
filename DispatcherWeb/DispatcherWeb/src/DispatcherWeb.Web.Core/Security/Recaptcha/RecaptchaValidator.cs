using System;
using System.Threading.Tasks;
using Abp.Dependency;
using Abp.Extensions;
using Abp.Json;
using Abp.UI;
using DispatcherWeb.Security.Recaptcha;
using Microsoft.AspNetCore.Http;
using Owl.reCAPTCHA;
using Owl.reCAPTCHA.v2;

namespace DispatcherWeb.Web.Security.Recaptcha
{
    public class RecaptchaValidator : DispatcherWebServiceBase, IRecaptchaValidator, ITransientDependency
    {
        public const string RecaptchaResponseKey = "g-recaptcha-response";

        private readonly IreCAPTCHASiteVerifyV2 _reCAPTCHASiteVerifyV2;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public RecaptchaValidator(IreCAPTCHASiteVerifyV2 reCAPTCHASiteVerifyV2, IHttpContextAccessor httpContextAccessor)
        {
            _reCAPTCHASiteVerifyV2 = reCAPTCHASiteVerifyV2;
            _httpContextAccessor = httpContextAccessor;
        }

        public async Task ValidateAsync(string captchaResponse)
        {
            var httpContext = _httpContextAccessor.HttpContext;
            if (httpContext == null)
            {
                throw new Exception("RecaptchaValidator should be used in a valid HTTP context!");
            }

            if (captchaResponse.IsNullOrEmpty())
            {
                throw new UserFriendlyException(L("CaptchaCanNotBeEmpty"));
            }

            var response = await _reCAPTCHASiteVerifyV2.Verify(new reCAPTCHASiteVerifyRequest
            {
                Response = captchaResponse,
                RemoteIp = _httpContextAccessor.HttpContext.Connection?.RemoteIpAddress?.ToString(),
            });

            if (!response.Success)
            {
                Logger.Warn(response.ToJsonString());
                throw new UserFriendlyException(L("IncorrectCaptchaAnswer"));
            }
        }
    }
}
