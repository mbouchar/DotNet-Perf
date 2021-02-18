using System.Net;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;
using RedHat.AspNetCore.Server.Kestrel.Transport.Linux;

// Deprecated
using Microsoft.AspNetCore.Server.Kestrel.Transport.Libuv;

namespace RestService
{
    /* 
     * Génération du certificat pour Kestrel:
     *   openssl pkcs12 -inkey localhost.pem -in localhost.csr -export -out localhost.pfx
     *
     * dotnet build -c Release
     * Windows:
     *   set ASPNETCORE_ENVIRONMENT=Production
     *   dotnet run -c Release --no-build --no-restore
     * Autre:
     *   ASPNETCORE_ENVIRONMENT=Production dotnet run -c Release --no-build --no-restore
     */

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
                                //options.Listen(IPAddress.Any, 5001, listenOptions =>
                                //{
                                //    listenOptions.UseHttps("localhost.pfx", "localhost");
                                //});
                            });
                            // Kestrel par défaut: Transport Kestrel Socket
                            //webBuilder.UseSockets();
                            // Deprecated: Transport Kestrel libuv
                            //webBuilder.UseLibuv();
                            // Expérimental: Transport Quic
                            //webBuilder.UseQuic();

                            /* Windows seulement
                             */

                            // Intégration IIS out-of-process
                            //webBuilder.UseIISIntegration();
                            // Intégration IIS in-process
                            //webBuilder.UseIIS();

                            /* Linux seulement
                             */

                            // Red Hat: Transport Kestrel AIO
                            /*webBuilder.UseLinuxTransport(options =>
                            {
                                // @todo: bug dans la librairie, il semble y avoir une erreur sur le retour (taille des réponses ou utilisation du chunking)
                                options.DeferSend = false;
                                //options.ThreadCount = 16;
                            });*/

                            webBuilder.UseUrls("http://0.0.0.0:5000");
                        });
        }
    }
}
