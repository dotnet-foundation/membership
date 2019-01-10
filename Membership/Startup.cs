using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Net.Http.Headers;
using System.Reflection;
using System.Security.Claims;
using System.Threading.Tasks;
using Membership.Models;
using Membership.Services;
using Microsoft.ApplicationInsights.AspNetCore;
using Microsoft.ApplicationInsights.Channel;
using Microsoft.ApplicationInsights.Extensibility;
using Microsoft.ApplicationInsights.SnapshotCollector;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.AzureAD.UI;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Authorization;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.Graph;
using Microsoft.Identity.Client;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;

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
            // https://github.com/dotnet/coreclr/issues/21743 there's a regression in 2.2 for now
            // Configure SnapshotCollector from application settings
            //services.Configure<SnapshotCollectorConfiguration>(Configuration.GetSection(nameof(SnapshotCollectorConfiguration)));

            // Add SnapshotCollector telemetry processor.
            //    services.AddSingleton<ITelemetryProcessorFactory>(sp => new SnapshotCollectorTelemetryProcessorFactory(sp));

            JwtSecurityTokenHandler.DefaultInboundClaimTypeMap.Clear();


            services.Configure<CookiePolicyOptions>(options =>
            {
                // This lambda determines whether user consent for non-essential cookies is needed for a given request.
                options.CheckConsentNeeded = context => true;
                options.MinimumSameSitePolicy = SameSiteMode.None;
            });

            services.AddAuthentication(AzureADDefaults.AuthenticationScheme)
                .AddAzureAD(options => Configuration.Bind("AzureAd", options));

            services
              .AddTokenAcquisition()
              .AddDistributedMemoryCache()
              .AddSession()
              .AddSessionBasedTokenCache();

            services.Configure<OpenIdConnectOptions>(AzureADDefaults.OpenIdScheme, options =>
            {
                options.Authority = options.Authority + "/v2.0/";         // Azure AD v2.0
                

                options.ResponseType = OpenIdConnectResponseType.CodeIdToken;
                options.SaveTokens = true;
                options.UseTokenLifetime = true;                

                options.TokenValidationParameters.NameClaimType = "name";
                options.TokenValidationParameters.RoleClaimType = "roles";
                //options.Scope.Add("offline_access");
                //options.Scope.Add("https://graph.microsoft.com/Directory.AccessAsUser.All");
                //options.Scope.Add("https://graph.microsoft.com/User.ReadWrite");

                options.Events = new OpenIdConnectEvents
                {
                    OnRemoteFailure = context =>
                    {
                        if(context.Request.Form["error_description"].FirstOrDefault()?.Contains("AADSTS50105") == true)
                        {
                            context.HandleResponse();
                            context.Response.Redirect($"{context.Request.Scheme}://{context.Request.Host}/Home/AccessDenied");                            
                        }
                        return Task.CompletedTask;
                    },

                    OnAuthorizationCodeReceived = async context =>
                    {
                        var tokenAcquisition = context.HttpContext.RequestServices.GetRequiredService<ITokenAcquisition>();
                        var scopes = new [] { "https://graph.microsoft.com/Directory.AccessAsUser.All", "https://graph.microsoft.com/User.ReadWrite" };
                        context.Success();

                        // Adds the token to the cache, and also handles the incremental consent and claim challenges
                        await tokenAcquisition.AddAccountToCacheFromAuthorizationCode(context, scopes);
                    }
                };

            });

            services.Configure<JwtBearerOptions>(AzureADDefaults.JwtBearerAuthenticationScheme, options =>
            {
                options.TokenValidationParameters.NameClaimType = "name";
                options.TokenValidationParameters.RoleClaimType = "roles";
            });
            
            services.AddMvc(options =>
            {
                var policy = new AuthorizationPolicyBuilder()
                    .RequireAuthenticatedUser()                    
                    .Build();
                options.Filters.Add(new AuthorizeFilter(policy));
            })
            .SetCompatibilityVersion(CompatibilityVersion.Version_2_2);

            services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();


            services.Configure<AdminConfig>(options => options.MembersGroupId = Configuration["AzureAd:MembersGroupId"]);

            services.AddSingleton<IGraphApplicationClient>(sp =>
            {
                var oidc = sp.GetRequiredService<IOptionsMonitor<OpenIdConnectOptions>>().Get(AzureADDefaults.OpenIdScheme);
                var app = new ConfidentialClientApplication(oidc.ClientId, oidc.Authority, "https://not/used", new ClientCredential(oidc.ClientSecret), null, new TokenCache());
                
                return new GraphClient(new GraphServiceClient("https://graph.microsoft.com/beta", new DelegateAuthenticationProvider(async (requestMessage) =>
                {
                    var accessToken = await app.AcquireTokenForClientAsync(new[] { "https://graph.microsoft.com/.default" });

                    requestMessage
                        .Headers
                        .Authorization = new AuthenticationHeaderValue("Bearer", accessToken.AccessToken);
                })));
            });

            services.AddScoped<IGraphDelegatedClient>(sp =>
            {
                return new GraphClient(new GraphServiceClient("https://graph.microsoft.com/beta", new DelegateAuthenticationProvider(async (requestMessage) =>
                {
                    //var accessToken = await sp.GetRequiredService<IHttpContextAccessor>().HttpContext.GetTokenAsync("access_token");
                    var http = sp.GetRequiredService<IHttpContextAccessor>().HttpContext;
                    var tokens = sp.GetRequiredService<ITokenAcquisition>();
                    var accessToken = await tokens.GetAccessTokenOnBehalfOfUser(http, new[] { "https://graph.microsoft.com/Directory.AccessAsUser.All", "https://graph.microsoft.com/User.ReadWrite" });

                    requestMessage
                        .Headers
                        .Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
                })));
            });

            services.AddScoped<UsersService>();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app,
                              ILoggerFactory loggerFactory,
                              IServiceProvider serviceProvider,
                              IHostingEnvironment env)
        {
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

            loggerFactory.AddApplicationInsights(serviceProvider, Microsoft.Extensions.Logging.LogLevel.Information);

            TelemetryConfiguration.Active.TelemetryInitializers.Add(new VersionTelemetry());

            app.UseHttpsRedirection();
            app.UseStaticFiles();
            app.UseCookiePolicy();
            app.UseSession();
            app.UseAuthentication();

            app.UseMvc(routes =>
            {
                routes.MapRoute(
                    name: "default",
                    template: "{controller=Home}/{action=Index}/{id?}");
            });
        }


        class SnapshotCollectorTelemetryProcessorFactory : ITelemetryProcessorFactory
        {
            readonly IServiceProvider _serviceProvider;

            public SnapshotCollectorTelemetryProcessorFactory(IServiceProvider serviceProvider) =>
                _serviceProvider = serviceProvider;

            public ITelemetryProcessor Create(ITelemetryProcessor next)
            {
                var snapshotConfigurationOptions = _serviceProvider.GetService<IOptions<SnapshotCollectorConfiguration>>();
                return new SnapshotCollectorTelemetryProcessor(next, configuration: snapshotConfigurationOptions.Value);
            }
        }

        class VersionTelemetry : ITelemetryInitializer
        {
            public void Initialize(ITelemetry telemetry)
            {
                telemetry.Context.Component.Version = Program.AssemblyInformationalVersion;
            }
        }
    }
}
