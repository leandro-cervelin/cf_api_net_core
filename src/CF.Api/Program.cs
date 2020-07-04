using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using NLog.Web;

namespace CF.Api
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            await CreateHostBuilder(args).Build().RunAsync();
        }

        public static IHostBuilder CreateHostBuilder(string[] args)
        {
            var config = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("hosting.json", optional: true)
                .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
                .AddCommandLine(args)
                .Build();

            var host = Host.CreateDefaultBuilder(args)
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.UseStartup<Startup>();
                    webBuilder.ConfigureLogging(options =>
                    {
                        options.AddConfiguration(config);
                        options.ClearProviders();
                        options.SetMinimumLevel(LogLevel.Trace);
                    });
                })
                .UseNLog();
#if DEBUG
            host.UseEnvironment("Development");
#endif
            return host;
        }
    }
}
