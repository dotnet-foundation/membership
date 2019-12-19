using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.ApplicationInsights;


namespace Membership
{
    public class Program
    {
        public static string AssemblyInformationalVersion => ThisAssembly.AssemblyInformationalVersion;

        public static void Main(string[] args)
        {
            CreateWebHostBuilder(args).Build().Run();
        }

        public static IWebHostBuilder CreateWebHostBuilder(string[] args) =>
            WebHost.CreateDefaultBuilder(args)
                .UseStartup<Startup>()
                .ConfigureLogging(logging =>
                {
                    logging.AddApplicationInsights();
                    logging.AddFilter<ApplicationInsightsLoggerProvider>("", LogLevel.Information);
                });
    }
}
