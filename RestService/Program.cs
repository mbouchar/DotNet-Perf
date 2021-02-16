using System.Net;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;
using RedHat.AspNetCore.Server.Kestrel.Transport.Linux;

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
/*                            webBuilder.UseKestrel(options =>
                            {
                                options.Limits.MaxConcurrentConnections = 500;
                                options.Limits.MaxConcurrentUpgradedConnections = 500;
                                /*options.Listen(IPAddress.Any, 5001, listenOptions =>
                                {
                                    listenOptions.UseHttps("localhost.pfx", "localhost");
                                });*/
                            //});
                            webBuilder.UseLinuxTransport();
                            // Intégration Socket
                            //webBuilder.UseSockets();
                            //webBuilder.UseUrls("http://0.0.0.0:5000");
                            // Intégration IIS out-of-process
                            //webBuilder.UseIISIntegration();
                            // Intégration IIS in-process
                            //webBuilder.UseIIS();
                            webBuilder.UseUrls("http://0.0.0.0:5000");
                        });
        }
    }
}
