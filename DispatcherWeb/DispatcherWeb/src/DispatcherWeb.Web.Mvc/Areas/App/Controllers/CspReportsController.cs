using System.IO;
using System.Threading.Tasks;
using DispatcherWeb.CspReports;
using DispatcherWeb.CspReports.Dto;
using DispatcherWeb.Web.Controllers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;

namespace DispatcherWeb.Web.Areas.App.Controllers
{
    [Area("App")]
    public class CspReportsController : DispatcherWebControllerBase
    {
        private readonly ICspReportAppService _cspReportAppService;

        public CspReportsController(
            ICspReportAppService cspReportAppService
        )
        {
            _cspReportAppService = cspReportAppService;
        }

        [HttpPost]
        [AllowAnonymous]
        [IgnoreAntiforgeryToken]
        public async Task<IActionResult> Post()
        {
            using (var inputStream = new StreamReader(Request.Body))
            {
                string s = await inputStream.ReadToEndAsync();
                if (!string.IsNullOrWhiteSpace(s))
                {
                    PostReportDto postReport = JsonConvert.DeserializeObject<PostReportDto>(s);
                    _cspReportAppService.PostReport(postReport);
                    var cspLogger = Logger.CreateChildLogger("CspLogger");
                    cspLogger.Error($"document-uri={postReport.CspReport.DocumentUri}; referrer={postReport.CspReport.Referrer}; blocked-uri={postReport.CspReport.BlockedUri}; violated-directive={postReport.CspReport.ViolatedDirective}; effective-directive={postReport.CspReport.EffectiveDirective}; original-policy={postReport.CspReport.OriginalPolicy};");

                }
            }

            return Ok();
        }
    }
}
