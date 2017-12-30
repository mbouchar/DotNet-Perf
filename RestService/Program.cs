using System.Net;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;

namespace RestService
{
    public class Program
    {
        public static void Main(string[] args)
        {
            BuildWebHost(args).Run();
        }

        public static IWebHost BuildWebHost(string[] args)
        {
/*            var config = new ConfigurationBuilder()
                .AddCommandLine(args)
                .AddEnvironmentVariables(prefix: "ASPNETCORE_")
                .AddJsonFile("hosting.json", optional: true)
                .Build();*/

            return WebHost.CreateDefaultBuilder(args)
//                .UseIISIntegration()
                .UseLibuv(opts => opts.ThreadCount = 4)
                .UseStartup<Startup>()
                .UseKestrel(options =>
                {
                    options.Limits.MaxConcurrentConnections = 500;
                    options.Limits.MaxConcurrentUpgradedConnections = 500;
                    options.Listen(IPAddress.Any, 5000);
                    options.Listen(IPAddress.Any, 5001, listenOptions =>
                    {
                        listenOptions.UseHttps("localhost.pfx", "localhost");
                    });
                })
                .Build();
        }
    }
}
