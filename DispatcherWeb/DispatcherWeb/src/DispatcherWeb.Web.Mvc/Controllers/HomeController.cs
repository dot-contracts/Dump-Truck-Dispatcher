using System.Threading.Tasks;
using DispatcherWeb.Identity;
using DispatcherWeb.Web.Models.Home;
using IdentityServer4.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Hosting;

namespace DispatcherWeb.Web.Controllers
{
    public class HomeController : DispatcherWebControllerBase
    {
        private readonly SignInManager _signInManager;
        private readonly IIdentityServerInteractionService _interaction;
        private readonly IWebHostEnvironment _environment;

        public HomeController(
            SignInManager signInManager,
            IIdentityServerInteractionService interaction,
            IWebHostEnvironment environment
            )
        {
            _signInManager = signInManager;
            _interaction = interaction;
            _environment = environment;
        }

        [AllowAnonymous]
        public async Task<IActionResult> Index(string redirect = "", bool forceNewRegistration = false)
        {
            if (forceNewRegistration)
            {
                await _signInManager.SignOutAsync();
            }

            if (redirect == "TenantRegistration")
            {
                return RedirectToAction("SelectEdition", "TenantRegistration");
            }

            return AbpSession.UserId.HasValue ?
                RedirectToAction("Index", "Home", new { area = "App" }) :
                RedirectToAction("Login", "Account");
        }

        [AllowAnonymous]
        public async Task<IActionResult> Error(string errorId)
        {
            var vm = new ErrorViewModel();

            // retrieve error details from identityserver
            var message = await _interaction.GetErrorContextAsync(errorId);
            if (message != null)
            {
                vm.Error = message;

                if (!_environment.IsDevelopment())
                {
                    message.ErrorDescription = null;
                }
            }

            return View("Error", vm);
        }
    }
}
