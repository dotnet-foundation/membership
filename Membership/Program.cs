using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Serilog;
using Serilog.Events;

namespace Membership
{
    public class Program
    {
        public static string AssemblyInformationalVersion => ThisAssembly.AssemblyInformationalVersion;

        public static void Main(string[] args)
        {
            Log.Logger = new LoggerConfiguration()
               .Enrich.FromLogContext()
               .MinimumLevel.Debug()
               .WriteTo.ColoredConsole(
                   LogEventLevel.Verbose,
                   "{NewLine}{Timestamp:HH:mm:ss} [{Level}] ({CorrelationToken}) {Message}{NewLine}{Exception}")
               //.WriteTo.File("log.txt", rollingInterval: RollingInterval.Day)
               .CreateLogger();

            try
            {
                CreateWebHostBuilder(args).Build().Run();
            }
            finally
            {
                Log.CloseAndFlush();
            }
        }

        public static IWebHostBuilder CreateWebHostBuilder(string[] args) =>
            WebHost.CreateDefaultBuilder(args)
                .UseApplicationInsights()
                .UseSerilog()
                .UseStartup<Startup>();
    }
}
