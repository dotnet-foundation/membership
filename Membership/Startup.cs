using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Net.Http.Headers;
using System.Runtime.InteropServices;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using Membership.Models;
using Membership.Services;
using Microsoft.ApplicationInsights.AspNetCore;
using Microsoft.ApplicationInsights.AspNetCore.TelemetryInitializers;
using Microsoft.ApplicationInsights.Channel;
using Microsoft.ApplicationInsights.DataContracts;
using Microsoft.ApplicationInsights.Extensibility;
using Microsoft.ApplicationInsights.SnapshotCollector;
using Microsoft.AspNetCore.Authentication.AzureAD.UI;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.Mvc.Authorization;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using Microsoft.Graph;
using Microsoft.Identity.Client;
using Microsoft.Identity.Web;
using Microsoft.Identity.Web.TokenCacheProviders.InMemory;
using Microsoft.Identity.Web.TokenCacheProviders.Session;

namespace Membership
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            // Configure SnapshotCollector from application settings
            services.Configure<SnapshotCollectorConfiguration>(Configuration.GetSection(nameof(SnapshotCollectorConfiguration)));
            services.AddApplicationInsightsTelemetry();

            // Add SnapshotCollector telemetry processor if we're on Windows
            if(RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                services.AddSingleton<ITelemetryProcessorFactory>(sp => new SnapshotCollectorTelemetryProcessorFactory(sp));
            }            
            services.AddSingleton<ITelemetryInitializer, VersionTelemetry>();
            services.AddSingleton<ITelemetryInitializer, UserTelemetry>();

            JwtSecurityTokenHandler.DefaultInboundClaimTypeMap.Clear();


            services.Configure<CookiePolicyOptions>(options =>
            {
                // This lambda determines whether user consent for non-essential cookies is needed for a given request.
                options.CheckConsentNeeded = context => true;
                options.MinimumSameSitePolicy = SameSiteMode.None;
            });


            // Token acquisition service based on MSAL.NET
            // and chosen token cache implementation
            services.AddMicrosoftIdentityPlatformAuthentication(Configuration)
                    .AddMsal(Configuration, new[]
                        {
                            //Constants.ScopeDirectoryAccessAsUserAll,
                            Constants.ScopeUserReadWrite
                        }
                            )
                    .AddInMemoryTokenCaches();
      

            services.Configure<OpenIdConnectOptions>(AzureADDefaults.OpenIdScheme, options =>
            {
                options.TokenValidationParameters.NameClaimType = "name";
                options.TokenValidationParameters.RoleClaimType = "roles";

                options.Events.OnRemoteFailure = context =>
                {
                    if (context.Request.Form["error_description"].FirstOrDefault()?.Contains("AADSTS50105") == true)
                    {
                        context.HandleResponse();
                        context.Response.Redirect($"{context.Request.Scheme}://{context.Request.Host}/Home/AccessDenied");
                    }
                    return Task.CompletedTask;
                };
            });

            services.AddMvc(options =>
            {
                var policy = new AuthorizationPolicyBuilder()
                    .RequireAuthenticatedUser()
                    .Build();
                options.Filters.Add(new AuthorizeFilter(policy));
            });

            services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();


            services.Configure<AdminConfig>(options => options.MembersGroupId = Configuration["AzureAd:MembersGroupId"]);

            services.AddSingleton<IGraphApplicationClient>(sp =>
            {
                var oidc = sp.GetRequiredService<IOptionsMonitor<OpenIdConnectOptions>>().Get(AzureADDefaults.OpenIdScheme);
                var app = ConfidentialClientApplicationBuilder.Create(oidc.ClientId)
                        .WithAuthority(oidc.Authority)
                        .WithClientSecret(oidc.ClientSecret)
                        .Build();

                return new GraphClient(new GraphServiceClient("https://graph.microsoft.com/beta", new DelegateAuthenticationProvider(async (requestMessage) =>
                {
                    var accessToken = await app.AcquireTokenForClient(new[] { "https://graph.microsoft.com/.default" }).ExecuteAsync();

                    requestMessage
                        .Headers
                        .Authorization = new AuthenticationHeaderValue("Bearer", accessToken.AccessToken);
                })));
            });

            //services.AddScoped<IGraphDelegatedClient>(sp =>
            //{
            //    return new GraphClient(new GraphServiceClient("https://graph.microsoft.com/beta", new DelegateAuthenticationProvider(async (requestMessage) =>
            //    {
            //        var tokens = sp.GetRequiredService<ITokenAcquisition>();
            //        var accessToken = await tokens.GetAccessTokenOnBehalfOfUserAsync(new[] { Constants.ScopeUserReadWrite });

            //        requestMessage
            //            .Headers
            //            .Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
            //    })));
            //});


            services.AddScoped<UsersService>();

            // Need this to get host headers in App Service
            services.Configure<ForwardedHeadersOptions>(options =>
            {
                options.ForwardedHeaders =
                    ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;
                options.RequireHeaderSymmetry = false;
                
                options.KnownNetworks.Clear();
                options.KnownProxies.Clear();
            });

            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app,
                              IWebHostEnvironment env)
        {

            app.UseForwardedHeaders();

            var basePath = Environment.GetEnvironmentVariable("BASE_PATH");

            if(!string.IsNullOrWhiteSpace(basePath))
            {
                app.UsePathBase(basePath);
            }

            if (env.IsDevelopment())
            {
                // Since IdentityModel version 5.2.1 (or since Microsoft.AspNetCore.Authentication.JwtBearer version 2.2.0),
                // PII hiding in log files is enabled by default for GDPR concerns.
                // For debugging/development purposes, one can enable additional detail in exceptions by setting IdentityModelEventSource.ShowPII to true.
                // Microsoft.IdentityModel.Logging.IdentityModelEventSource.ShowPII = true;
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            } 

            app.UseHttpsRedirection();
            app.UseStaticFiles();
            app.UseCookiePolicy();

            app.UseRouting();

            // Allow local development without KeyVault access using "Local Development" launch profile
            if (Configuration["AzureAd:ClientId"] != null)
            {
                //// Workaround Safari Same-site cookie issues
                //// https://brockallen.com/2019/01/11/same-site-cookies-asp-net-core-and-external-authentication-providers/
                //app.Use(async (ctx, next) =>
                //{
                //    var schemes = ctx.RequestServices.GetRequiredService<IAuthenticationSchemeProvider>();
                //    var handlers = ctx.RequestServices.GetRequiredService<IAuthenticationHandlerProvider>();
                //    foreach (var scheme in await schemes.GetRequestHandlerSchemesAsync())
                //    {
                //        var handler = await handlers.GetHandlerAsync(ctx, scheme.Name) as IAuthenticationRequestHandler;
                //        if (handler != null && await handler.HandleRequestAsync())
                //        {
                //        // start same-site cookie special handling
                //        string location = null;
                //            if (ctx.Response.StatusCode == 302)
                //            {
                //                location = ctx.Response.Headers["location"];
                //            }
                //            else if (ctx.Request.Method == "GET" && !ctx.Request.Query["skip"].Any())
                //            {
                //                location = ctx.Request.Path + ctx.Request.QueryString + "&skip=1";
                //            }

                //            if (location != null)
                //            {
                //                ctx.Response.StatusCode = 200;
                //                var html = $@"
                //        <html><head>
                //            <meta http-equiv='refresh' content='0;url={location}' />
                //        </head></html>";
                //                await ctx.Response.WriteAsync(html);
                //            }
                //        // end same-site cookie special handling

                //        return;
                //        }
                //    }


                //    await next();
                //});

                app.UseAuthentication();
                app.UseAuthorization();
            }

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllerRoute(
                    name: "default",
                    pattern: "{controller=Home}/{action=Index}/{id?}");
                endpoints.MapRazorPages();
            });
        }

        private class SnapshotCollectorTelemetryProcessorFactory : ITelemetryProcessorFactory
        {
            private readonly IServiceProvider _serviceProvider;

            public SnapshotCollectorTelemetryProcessorFactory(IServiceProvider serviceProvider) =>
                _serviceProvider = serviceProvider;

            public ITelemetryProcessor Create(ITelemetryProcessor next)
            {
                var snapshotConfigurationOptions = _serviceProvider.GetService<IOptions<SnapshotCollectorConfiguration>>();
                return new SnapshotCollectorTelemetryProcessor(next, configuration: snapshotConfigurationOptions.Value);
            }
        }

        private class VersionTelemetry : ITelemetryInitializer
        {
            public void Initialize(ITelemetry telemetry)
            {
                telemetry.Context.Component.Version = Program.AssemblyInformationalVersion;
            }
        }

        private class UserTelemetry : TelemetryInitializerBase
        {
            public UserTelemetry(IHttpContextAccessor httpContextAccessor) : base(httpContextAccessor)
            {
            }

            protected override void OnInitializeTelemetry(HttpContext platformContext, RequestTelemetry requestTelemetry, ITelemetry telemetry)
            {
                if (platformContext.RequestServices == null)
                    return;

                var identity = platformContext.User.Identity;
                if (!identity.IsAuthenticated)
                    return;


                var upn = ((ClaimsIdentity)identity).FindFirst("upn")?.Value;
                if (upn != null)
                    telemetry.Context.User.AuthenticatedUserId = upn;

                var userId = ((ClaimsIdentity)identity).FindFirst("oid")?.Value;

                if (userId == null)
                    return;

                telemetry.Context.User.Id = userId;
                telemetry.Context.User.AccountId = userId;
            }
        }
    }
}
