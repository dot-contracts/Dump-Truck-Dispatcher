using System;
using System.IO;
using System.Linq;
using System.Reflection;
using Abp.AspNetCore;
using Abp.AspNetCore.Configuration;
using Abp.AspNetCore.Mvc.Antiforgery;
using Abp.AspNetCore.Mvc.Caching;
using Abp.AspNetCore.Mvc.Extensions;
using Abp.Castle.Logging.Log4Net;
using Abp.Configuration;
using Abp.Hangfire;
using Abp.PlugIns;
using Abp.Timing;
using Azure.Storage.Blobs;
using Castle.Facilities.Logging;
using DispatcherWeb.Authorization;
using DispatcherWeb.Configuration;
using DispatcherWeb.EntityFrameworkCore;
using DispatcherWeb.Fulcrum;
using DispatcherWeb.Identity;
using DispatcherWeb.Infrastructure.AzureBlobs;
using DispatcherWeb.Infrastructure.AzureTables;
using DispatcherWeb.Infrastructure.RecurringJobs;
using DispatcherWeb.Web.ApplicationInsights;
using DispatcherWeb.Web.Authentication.JwtBearer;
using DispatcherWeb.Web.Common;
using DispatcherWeb.Web.Extensions;
using DispatcherWeb.Web.HealthCheck;
using DispatcherWeb.Web.IdentityServer;
using DispatcherWeb.Web.Resources;
using DispatcherWeb.Web.Session;
using DispatcherWeb.Web.SignalR;
using DispatcherWeb.Web.Swagger;
using Hangfire;
using Hangfire.Azure.ServiceBusQueue;
using HealthChecks.UI.Client;
using IdentityServer4.Configuration;
using Microsoft.ApplicationInsights.Extensibility;
using Microsoft.ApplicationInsights.SnapshotCollector;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Razor;
using Microsoft.AspNetCore.Server.Kestrel.Https;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.OpenApi.Models;
using Owl.reCAPTCHA;
using Stripe;

namespace DispatcherWeb.Web.Startup
{
    public class Startup
    {
        private readonly IConfigurationRoot _appConfiguration;
        private readonly IWebHostEnvironment _hostingEnvironment;

        public Startup(IWebHostEnvironment env)
        {
            _appConfiguration = env.GetAppConfiguration();
            _hostingEnvironment = env;
        }

        private string[] GetCorsOrigins()
        {
            return _appConfiguration["App:CorsOrigins"]?.Split(";").Where(x => !string.IsNullOrEmpty(x)).ToArray() ?? new string[0];
        }

        private BlobClient GetDataProtectionBlobClient()
        {
            var blobContainerClient = new BlobServiceClient(_appConfiguration["Abp:StorageConnectionString"], AttachmentHelper.GetBlobClientOptions(_appConfiguration))
                .GetBlobContainerClient(_appConfiguration["DataProtection:ContainerName"]);

            //If you're getting an error here you might have forgot to run azurite.
            //If you didn't know about azurite make sure to carefully re-read our wiki pages related to the development process to make sure nothing else was missed.
            blobContainerClient.CreateIfNotExists();

            return blobContainerClient.GetBlobClient(_appConfiguration["DataProtection:BlobName"]);
        }

        public IServiceProvider ConfigureServices(IServiceCollection services)
        {
            if (!_appConfiguration.GetValue<bool>("App:DisableAppInsights"))
            {
                services.AddApplicationInsightsTelemetry();
                services.AddSnapshotCollector();
                services.AddSingleton<ITelemetryInitializer, AbpTelemetryInitializer>();
            }

            // Comment out Azure Blob Storage data protection to fix Azurite connection issue
            // services.AddDataProtection()
            //         .SetApplicationName(_appConfiguration["Authentication:DataProtectionApplicationName"] ?? "DispatcherWeb")
            //         .PersistKeysToAzureBlobStorage(GetDataProtectionBlobClient());

            // Use file system for data protection instead
            services.AddDataProtection()
                    .SetApplicationName(_appConfiguration["Authentication:DataProtectionApplicationName"] ?? "DispatcherWeb")
                    .PersistKeysToFileSystem(new DirectoryInfo(Path.Combine(_hostingEnvironment.ContentRootPath, "DataProtection")));

            services.AddSingleton<InterceptorConfiguration>(provider =>
            {
                return new InterceptorConfiguration
                {
                    AlwaysCreateUowFromInterceptor = _appConfiguration.GetValue<bool>("Abp:Interceptor:AlwaysCreateUowFromInterceptor"),
                };
            });
            services.AddSingleton<UserStoreConfiguration>(provider =>
            {
                return new UserStoreConfiguration
                {
                    DetermineProviderCultureResult = _appConfiguration.GetValue<bool>("Abp:UserStore:DetermineProviderCultureResult"),
                    UseDbStoredRoleClaims = _appConfiguration.GetValue<bool>("Abp:UserStore:UseDbStoredRoleClaims"),
                    UseDbStoredUserClaims = _appConfiguration.GetValue<bool>("Abp:UserStore:UseDbStoredUserClaims"),
                    UseDbStoredUserTokens = _appConfiguration.GetValue<bool>("Abp:UserStore:UseDbStoredUserTokens"),
                    UseOrganizationUnitRoles = _appConfiguration.GetValue<bool>("Abp:UserStore:UseOrganizationUnitRoles"),
                    FallbackToDatabaseOnCacheMisses = _appConfiguration.GetValue<bool>("Abp:UserStore:FallbackToDatabaseOnCacheMisses"),
                };
            });

            // MVC
            services.AddControllersWithViews(options =>
            {
                options.Filters.Add(new AbpAutoValidateAntiforgeryTokenAttribute());
                options.CacheProfiles.Add(WebConsts.CacheProfiles.StaticFiles, new CacheProfile
                {
                    Duration = (int)TimeSpan.FromDays(365).TotalSeconds,
                    Location = ResponseCacheLocation.Any,
                    VaryByQueryKeys = new string[] {
                        "*",
                    },
                });
            })
#if DEBUG
                .AddRazorRuntimeCompilation()
#endif
                .AddNewtonsoftJson();

            if (bool.Parse(_appConfiguration["KestrelServer:IsEnabled"]))
            {
                ConfigureKestrel(services);
            }

            GlobalJobFilters.Filters.Add(new AutomaticRetryAttribute { Attempts = 3 });

            services.AddHsts(options =>
            {
                options.Preload = true;
                options.IncludeSubDomains = true;
                options.MaxAge = TimeSpan.FromHours(2);
            });

            IdentityRegistrar.Register(services);

            //Identity server
            if (bool.Parse(_appConfiguration["IdentityServer:IsEnabled"]))
            {
                IdentityServerRegistrar.Register(services, _appConfiguration, options =>
                {
                    options.UserInteraction = new UserInteractionOptions
                    {
                        LoginUrl = "/Account/Login",
                        LogoutUrl = "/Account/LogOut",
                        ErrorUrl = "/Error",
                    };

                    var issuerUri = _appConfiguration["IdentityServer:IssuerUri"];
                    if (string.IsNullOrEmpty(issuerUri))
                    {
                        issuerUri = null;
                    }
                    options.IssuerUri = issuerUri;
                });
            }
            else
            {
                services.Configure<SecurityStampValidatorOptions>(opts =>
                {
                    opts.OnRefreshingPrincipal = SecurityStampValidatorCallback.UpdatePrincipal;
                });
            }

            AuthConfigurer.Configure(services, _appConfiguration);

            if (_appConfiguration.GetValue<bool>("Swagger:IsEnabled"))
            {
                ConfigureSwagger(services);
            }

            //Recaptcha
            services.AddreCAPTCHAV2(x =>
            {
                x.SiteKey = _appConfiguration["Recaptcha:SiteKey"];
                x.SiteSecret = _appConfiguration["Recaptcha:SecretKey"];
            });

            if (WebConsts.HangfireDashboardEnabled)
            {
                //Hangfire (Enable to use Hangfire instead of default job manager)
                services.AddHangfire(config =>
                {
                    var sqlStorage = config.UseSqlServerStorage(_appConfiguration.GetConnectionString("Default"));
                    var serviceBusConnectionString = _appConfiguration["Abp:ServiceBusConnectionString"];
                    if (!string.IsNullOrEmpty(serviceBusConnectionString))
                    {
                        sqlStorage.UseServiceBusQueues(serviceBusConnectionString);
                    }
                });
                if (_appConfiguration["App:HangfireServerEnabled"] == "true")
                {
                    services.AddHangfireServer(o => o.WorkerCount = 1);
                }
            }

            services.AddScoped<IWebResourceManager, WebResourceManager>();

            if (_appConfiguration["App:SignalRServerEnabled"] == "true")
            {
                var signalR = services.AddSignalR().AddNewtonsoftJsonProtocol();
                var azureSignalRConnectionString = _appConfiguration["Azure:SignalR:ConnectionString"];
                if (!string.IsNullOrEmpty(azureSignalRConnectionString))
                {
                    signalR.AddAzureSignalR(azureSignalRConnectionString);
                }
            }

            //if (WebConsts.GraphQL.Enabled)
            //{
            //    services.AddAndConfigureGraphQL();
            //}

            var corsOrigins = GetCorsOrigins();
            if (corsOrigins.Any())
            {
                services.AddCors(options =>
                {
                    options.AddPolicy("default", policy =>
                    {
                        policy.WithOrigins(corsOrigins)
                            .AllowAnyHeader()
                            .AllowAnyMethod()
                            .AllowCredentials();
                    });
                });
            }

            services.Configure<SnapshotCollectorConfiguration>(_appConfiguration.GetSection("SnapshotCollector"));
            services.Configure<SecurityStampValidatorOptions>(options =>
            {
                options.ValidationInterval = TimeSpan.Zero;
            });

            if (bool.Parse(_appConfiguration["HealthChecks:HealthChecksEnabled"]))
            {
                services.AddAbpZeroHealthCheck();
            }

            services.Configure<RazorViewEngineOptions>(options =>
            {
                options.ViewLocationExpanders.Add(new RazorViewLocationExpander());
            });

            // Fulcrum Client
            services.AddHttpClient<FulcrumHttpClient>(client =>
            {
                client.BaseAddress = new Uri(_appConfiguration["Fulcrum:BaseUrl"]);
                client.DefaultRequestHeaders.Add("Accept", "text/plain");

            });

            //Configure Abp and Dependency Injection
            var abpServiceProvider = services.AddAbp<DispatcherWebWebMvcModule>(options =>
            {
                //Configure Log4Net logging
                options.IocManager.IocContainer.AddFacility<LoggingFacility>(
                    f => f.UseAbpLog4Net().WithConfig(
                        "log4net.config"
                    //_hostingEnvironment.IsDevelopment()
                    //    ? "log4net.config"
                    //    : "log4net.Production.config"
                    )
                );

                options.PlugInSources.AddFolder(Path.Combine(_hostingEnvironment.WebRootPath, "Plugins"),
                    SearchOption.AllDirectories);
            });

            // Ensure ABP services are properly initialized
            try
            {
                using (var scope = abpServiceProvider.CreateScope())
                {
                    // Test core ABP services to ensure they're properly registered
                    var settingManager = scope.ServiceProvider.GetService<Abp.Configuration.ISettingManager>();
                    var logger = scope.ServiceProvider.GetService<Abp.Logging.ILogger>();
                    var session = scope.ServiceProvider.GetService<Abp.Runtime.Session.IAbpSession>();
                    
                    if (settingManager == null)
                    {
                        throw new InvalidOperationException("SettingManager is not properly registered");
                    }
                    if (logger == null)
                    {
                        throw new InvalidOperationException("Logger is not properly registered");
                    }
                    if (session == null)
                    {
                        throw new InvalidOperationException("AbpSession is not properly registered");
                    }
                }
            }
            catch (Exception ex)
            {
                // Log the error but don't fail startup - let the application try to run
                Console.WriteLine($"ABP service initialization warning: {ex.Message}");
            }

            return abpServiceProvider;
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env, ILoggerFactory loggerFactory)
        {
            // Verify ABP services are available at startup
            try
            {
                using (var scope = app.ApplicationServices.CreateScope())
                {
                    var settingManager = scope.ServiceProvider.GetService<Abp.Configuration.ISettingManager>();
                    var logger = scope.ServiceProvider.GetService<Abp.Logging.ILogger>();
                    var session = scope.ServiceProvider.GetService<Abp.Runtime.Session.IAbpSession>();
                    
                    if (settingManager != null && logger != null && session != null)
                    {
                        logger.Info("ABP services successfully initialized");
                    }
                    else
                    {
                        Console.WriteLine("Warning: Some ABP services are not available");
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"ABP service verification failed: {ex.Message}");
            }

            var policyCollection = new HeaderPolicyCollection()
                .AddContentSecurityPolicy(builder =>
                //.AddContentSecurityPolicyReportOnly(builder => // report-only
                {
                    builder.AddReportUri()
                        .To("/app/CspReports/Post");
                    var defaultSrc = builder.AddDefaultSrc()
                        .Self()
                        .From($"wss://{Utilities.GetDomainFromUrl(_appConfiguration["App:WebSiteRootAddress"])}")
                        .From($"wss://{Utilities.GetDomainFromUrl(_appConfiguration["App:SignalRRootAddress"])}")
                        .From($"https://{Utilities.GetDomainFromUrl(_appConfiguration["App:SignalRRootAddress"])}")
                        .From("https://fonts.googleapis.com/")
                        .From("https://api2-c.heartlandportico.com")
                        .From("https://hps.github.io/token/") //heartland
                        .From("https://www.google.com/recaptcha/")
                        .From("https://dumptruckdispatcher.com/")
#if DEBUG
                        .From("http://localhost:*") //browserLink
#endif
                        ;

                    if (_appConfiguration["App:CSP:EnableUnsafeInlineSrc"] == "true")
                    {
                        //this is only a temporary solution for production until we move from UserGuiding to a different solution that doesn't violate CSP
                        //this should NOT be used for local development, or dev/qa stages.
                        defaultSrc.UnsafeInline();
                    }
                    else
                    {
                        defaultSrc
                            .WithNonce()
                            .WithHash256("u+OupXgfekP+x/f6rMdoEAspPCYUtca912isERnoEjY=") // Identity Server's end session: <style>iframe{{display:none;width:0;height:0;}}</style>
#if DEBUG
                            .WithHash256("47DEQpj8HBSa+/TImW+5JCeuQeRkm5NMpJWZG3hSuFU=") //browserLink
                            .WithHash256("tVFibyLEbUGj+pO/ZSi96c01jJCvzWilvI5Th+wLeGE=") //browserLink
#endif
                            ;
                    }

                    var scriptSrc = builder.AddScriptSrc()
                        .Self()
                        .From("https://maps.googleapis.com/")
                        .From("https://api2-c.heartlandportico.com")
                        .From("https://hps.github.io/token/") //heartland
                        .From("https://www.google.com/recaptcha/")
                        .From("https://www.gstatic.com/recaptcha/")
                        .From("https://az416426.vo.msecnd.net/")  //appinsights
                        .From("https://js.monitor.azure.com/") //appinsights
#if DEBUG
                        .From("http://localhost:*") //browserLink
#endif
                        ;

                    if (_appConfiguration["App:CSP:EnableUnsafeInlineScript"] == "true")
                    {
                        //this is only a temporary solution for production until we finish testing CSP for the next release
                        //this should NOT be used for local development, or dev/qa stages.
                        scriptSrc.UnsafeInline();
                    }
                    else
                    {
                        scriptSrc
                            .WithNonce()
                            .WithHash256("orD0/VhH8hLqrLxKHD/HUEMdwqX6/0ve7c5hspX5VJ8=") // Identity Server's /connect/authorize: <script>window.addEventListener('load', function(){document.forms[0].submit();});</script>
                            .WithHash256("fa5rxHhZ799izGRP38+h4ud5QXNT0SFaFlh4eqDumBI=") // Identity Server's check session
                            ;
                    }

                    var connectSrc = builder.AddConnectSrc()
                        .Self()
                        .From($"wss://{Utilities.GetDomainFromUrl(_appConfiguration["App:WebSiteRootAddress"])}")
                        .From($"wss://{Utilities.GetDomainFromUrl(_appConfiguration["App:SignalRRootAddress"])}")
                        .From($"https://{Utilities.GetDomainFromUrl(_appConfiguration["App:SignalRRootAddress"])}")
                        .From("https://maps.googleapis.com/")
                        .From("https://api2-c.heartlandportico.com")
                        .From("https://hps.github.io/token/") //heartland
                        .From("https://dc.services.visualstudio.com/v2/track") //appinsights
                        .From("https://*.applicationinsights.azure.com/") //appinsights
#if DEBUG
                        .From("http://localhost:*") //browserLink
                        .From("ws://localhost:*")
#endif
                        ;

                    var signalRAlternateAddress = _appConfiguration["App:SignalRAlternateAddress"];
                    if (!string.IsNullOrEmpty(signalRAlternateAddress))
                    {
                        connectSrc
                            .From($"wss://{Utilities.GetDomainFromUrl(signalRAlternateAddress)}")
                            .From($"https://{Utilities.GetDomainFromUrl(signalRAlternateAddress)}");
                    }

                    if (_appConfiguration["App:CSP:EnableWorkerBlob"] == "true")
                    {
                        builder.AddCustomDirective("worker-src", "'self' blob:");
                    }

                    var imgSrc = builder.AddImgSrc()
                        .Self()
                        .From("https://maps.gstatic.com/")
                        .From("https://*.googleapis.com/") //maps., khms0., khms1.googleapis.com
                        .Data();

                    var fontSrc = builder.AddFontSrc()
                        .Self()
                        .From("https://fonts.gstatic.com/")
                        .From("https://fonts.googleapis.com/")
                        ;

                    if (_appConfiguration["App:CSP:EnableChromeExtensionFonts"] == "true")
                    {
                        fontSrc.From("chrome-extension:");
                    }
                    if (_appConfiguration["App:CSP:EnableDataFonts"] == "true")
                    {
                        fontSrc.Data();
                    }

                    builder.AddObjectSrc()
                        .None();
                    var corsOrigins = GetCorsOrigins();
                    var frameAncestors = builder.AddFrameAncestors()
                        .Self();
                    foreach (var corsOrigin in corsOrigins)
                    {
                        frameAncestors = frameAncestors.From(corsOrigin);
                    }
                });
            app.UseSecurityHeaders(policyCollection);

            var cookieDomain = _appConfiguration["App:CookieDomain"];
            var migrateCookies = _appConfiguration["App:MigrateCookies"];
            if (!string.IsNullOrEmpty(cookieDomain) && migrateCookies == "true")
            {
                CookieMigrationMiddleware.OldCookieDomain = "." + Utilities.GetDomainFromUrl(_appConfiguration["App:WebSiteRootAddress"]);
                CookieMigrationMiddleware.CookieDomain = cookieDomain;
                app.UseMiddleware<CookieMigrationMiddleware>();
            }

            if (_appConfiguration["SignalR:RedirectToNewHub"] == "true")
            {
                app.UseMiddleware<SignalRChatRedirectMiddleware>();
            }

            if (!_appConfiguration.GetValue<bool>("App:DisableThreadPoolMonitoringMiddleware")
                && !_appConfiguration.GetValue<bool>("App:DisableAppInsights"))
            {
                app.UseMiddleware<ThreadPoolMonitoringMiddleware>();
            }

            if (!_appConfiguration.GetValue<bool>("App:DisablePerformanceMonitoringMiddleware")
                && !_appConfiguration.GetValue<bool>("App:DisableAppInsights"))
            {
                app.UseMiddleware<PerformanceMonitoringMiddleware>();
            }

            app.UseGetScriptsResponsePerUserCache();
            app.UseResponseCaching();

            //Initializes ABP framework.
            app.UseAbp(options =>
            {
                options.UseAbpRequestLocalization = false; //used below: UseAbpRequestLocalization
            });

            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseDispatcherWebForwardedHeaders();
            }
            else
            {
                app.UseStatusCodePagesWithRedirects("~/Error?statusCode={0}");
                app.UseExceptionHandler("/Error");
                app.UseDispatcherWebForwardedHeaders();
                app.UseHsts();
            }

            app.UseHttpsRedirection();

            // Serve files from wwwroot/Common/Images/favicons as if they were in the root of wwwroot
            app.UseStaticFiles(new StaticFileOptions
            {
                FileProvider = new PhysicalFileProvider(
                    Path.Combine(env.WebRootPath, "Common", "Images", "favicons")),
                RequestPath = "",
            });
            // Continue serving all files from wwwroot
            app.UseStaticFiles();

            app.UseRouting();


            Clock.Provider = ClockProviders.Utc;

            app.UseAuthentication();
            app.UseCors("default");

            if (bool.Parse(_appConfiguration["Authentication:JwtBearer:IsEnabled"]))
            {
                app.UseJwtTokenMiddleware();
            }

            if (bool.Parse(_appConfiguration["IdentityServer:IsEnabled"]))
            {
                app.UseIdentityServer();
            }

            app.UseAuthorization();

            using (var scope = app.ApplicationServices.CreateScope())
            {
#pragma warning disable CS0618 // Type or member is obsolete - ignore sync call for now
                if (_appConfiguration["App:CheckDbExistence"] != "true"
                    || scope.ServiceProvider.GetService<DatabaseCheckHelper>()
                        .Exist(_appConfiguration["ConnectionStrings:Default"]))
#pragma warning restore CS0618 // Type or member is obsolete
                {
                    app.UseAbpRequestLocalization();
                }
            }

            if (WebConsts.HangfireDashboardEnabled)
            {
                //Hangfire dashboard & server (Enable to use Hangfire instead of default job manager)
                app.UseHangfireDashboard("/hangfire", new DashboardOptions
                {
                    AsyncAuthorization = new[]
                    {
                        new AbpHangfireAuthorizationFilter(AppPermissions.Pages_Administration_HangfireDashboard),
                    },
                });
            }

            if (_appConfiguration["App:RunMigrations"] == "true")
            {
                AzureTableManager.CreateAllTables(_appConfiguration);
            }

            if (_appConfiguration["App:HangfireServerEnabled"] == "true")
            {
                StaticRecurringJobs.CreateAll(_appConfiguration);
            }

            ConfigureSemaphores();

            if (bool.Parse(_appConfiguration["Payment:Stripe:IsActive"]))
            {
                StripeConfiguration.ApiKey = _appConfiguration["Payment:Stripe:SecretKey"];
            }

            //if (WebConsts.GraphQL.Enabled)
            //{
            //    app.UseGraphQL<MainSchema>();
            //    if (WebConsts.GraphQL.PlaygroundEnabled)
            //    {
            //        app.UseGraphQLPlayground(
            //            new GraphQLPlaygroundOptions()); //to explorer API navigate https://*DOMAIN*/ui/playground
            //    }
            //}

            app.UseEndpoints(endpoints =>
            {
                if (_appConfiguration["App:SignalRServerEnabled"] == "true")
                {
                    endpoints.MapHub<SignalRHub>("/signalr");
                    if (_appConfiguration["SignalR:LegacyChatHub"] == "true")
                    {
                        endpoints.MapHub<SignalRHub>("/signalr-chat");
                    }
                }

                endpoints.MapControllerRoute("defaultWithArea", "{area}/{controller=Home}/{action=Index}/{id?}");
                endpoints.MapControllerRoute("default", "{controller=Home}/{action=Index}/{id?}");

                if (bool.Parse(_appConfiguration["HealthChecks:HealthChecksEnabled"]))
                {
                    endpoints.MapHealthChecks("/health", new HealthCheckOptions
                    {
                        Predicate = _ => true,
                        ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse,
                    });
                }

                var abpConfiguration = app.ApplicationServices.GetRequiredService<IAbpAspNetCoreConfiguration>();
                abpConfiguration.EndpointConfiguration.ConfigureAllEndpoints(endpoints);
            });

            if (_appConfiguration.GetValue<bool>("Swagger:IsEnabled"))
            {
                app.UseMiddleware<SwaggerAuthorizationMiddleware>();

                // Enable middleware to serve generated Swagger as a JSON endpoint
                app.UseSwagger();
                //Enable middleware to serve swagger - ui assets(HTML, JS, CSS etc.)
                app.UseSwaggerUI(options =>
                {
                    options.SwaggerEndpoint(_appConfiguration["App:SwaggerEndPoint"], "DispatcherWeb API V1");
                    //options.IndexStream = () => Assembly.GetExecutingAssembly()
                    //    .GetManifestResourceStream("DispatcherWeb.Web.wwwroot.swagger.ui.index.html");
                    //options.InjectBaseUrl(_appConfiguration["App:WebSiteRootAddress"]);
                }); //URL: /swagger
            }
        }

        private void ConfigureSemaphores()
        {
            Sms.SmsAppService.ConfigureSmsCallbackSemaphore(_appConfiguration);
        }

        private void ConfigureKestrel(IServiceCollection services)
        {
            services.Configure<Microsoft.AspNetCore.Server.Kestrel.Core.KestrelServerOptions>(options =>
            {
                options.Listen(new System.Net.IPEndPoint(System.Net.IPAddress.Any, 443),
                    listenOptions =>
                    {
                        var certPassword = _appConfiguration.GetValue<string>("Kestrel:Certificates:Default:Password");
                        var certPath = _appConfiguration.GetValue<string>("Kestrel:Certificates:Default:Path");
                        var cert = System.Security.Cryptography.X509Certificates.X509Certificate2.CreateFromEncryptedPemFile(certPath, certPassword);
                        listenOptions.UseHttps(new HttpsConnectionAdapterOptions
                        {
                            ServerCertificate = cert,
                        });
                    });
            });
        }

        private void ConfigureSwagger(IServiceCollection services)
        {
            services.AddSwaggerGen(options =>
            {
                options.SwaggerDoc("v1", new OpenApiInfo() { Title = "DispatcherWeb API", Version = "v1" });
                options.DocInclusionPredicate((docName, description) => true);
                options.ParameterFilter<SwaggerEnumParameterFilter>();
                options.SchemaFilter<SwaggerEnumSchemaFilter>();
                options.OperationFilter<SwaggerOperationIdFilter>();
                options.OperationFilter<SwaggerOperationFilter>();
                options.CustomDefaultSchemaIdSelector();
                options.CustomSchemaIds(type => type.FullName);

                // Add summaries to swagger
                var canShowSummaries = _appConfiguration.GetValue<bool>("Swagger:ShowSummaries");
                if (!canShowSummaries)
                {
                    return;
                }

                var mvcXmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
                var mvcXmlPath = Path.Combine(AppContext.BaseDirectory, mvcXmlFile);
                options.IncludeXmlComments(mvcXmlPath);

                var applicationXml = $"DispatcherWeb.Application.xml";
                var applicationXmlPath = Path.Combine(AppContext.BaseDirectory, applicationXml);
                options.IncludeXmlComments(applicationXmlPath);

                var webCoreXmlFile = $"DispatcherWeb.Web.Core.xml";
                var webCoreXmlPath = Path.Combine(AppContext.BaseDirectory, webCoreXmlFile);
                options.IncludeXmlComments(webCoreXmlPath);
            }).AddSwaggerGenNewtonsoftSupport();
        }

    }
}
