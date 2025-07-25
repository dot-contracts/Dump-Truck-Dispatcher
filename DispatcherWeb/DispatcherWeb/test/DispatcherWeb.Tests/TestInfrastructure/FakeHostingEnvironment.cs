using System;
using System.IO;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.FileProviders;

namespace DispatcherWeb.Tests.TestInfrastructure
{
    public class FakeHostingEnvironment : IWebHostEnvironment
    {
        public string EnvironmentName { get; set; }
        public string ApplicationName { get; set; }
        public string WebRootPath { get; set; } = Path.Combine(Environment.CurrentDirectory, "wwwroot");
        public IFileProvider WebRootFileProvider { get; set; }
        public string ContentRootPath { get; set; } = Environment.CurrentDirectory;
        public IFileProvider ContentRootFileProvider { get; set; }
    }
}
