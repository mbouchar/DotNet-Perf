using System.Net;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;

namespace RestService
{
    public class Program
    {
        public static void Main(string[] args)
        {
            CreateHostBuilder(args).Build().Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args)
        {
/*            var config = new ConfigurationBuilder()
                .AddCommandLine(args)
                .AddEnvironmentVariables(prefix: "ASPNETCORE_")
                .AddJsonFile("hosting.json", optional: true)
                .Build();*/

            return Host.CreateDefaultBuilder(args)
                       .ConfigureWebHostDefaults(webBuilder =>
                       {
                            webBuilder.UseStartup<Startup>();
                            webBuilder.UseKestrel(options =>
                            {
                                options.Limits.MaxConcurrentConnections = 500;
                                options.Limits.MaxConcurrentUpgradedConnections = 500;
                                options.Listen(IPAddress.Any, 5000);
                                /*options.Listen(IPAddress.Any, 5001, listenOptions =>
                                {
                                    listenOptions.UseHttps("localhost.pfx", "localhost");
                                });*/
                            });
                        });
//                .UseIISIntegration()
//                .UseLibuv(opts => opts.ThreadCount = 4)
        }
    }
}
