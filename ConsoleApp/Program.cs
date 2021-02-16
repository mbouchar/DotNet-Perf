using System;
using System.Diagnostics;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

using System.Collections.Generic;
using System.Runtime.Serialization.Json;
using ConsoleApp.Models;
using System.IO;
using Newtonsoft.Json;
using System.Runtime.InteropServices;
using System.Net;

namespace ConsoleApp
{
    class Program
    {
        private static HttpClient client = null;

        private static string DEFAULT_HOSTNAME = "localhost";
        private static string DOTNET_VERSION = "3.1";

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

        static void Main(string[] args)
        {
            string hostname = DEFAULT_HOSTNAME;
            if (args.Length > 0)
                hostname = args[0];

            string restReverseProxyHttpUrl = $"http://{hostname}/RestService";
            string restReverseProxyHttpsUrl = $"https://{hostname}/RestService";
            string restKestrelHttpUrl = $"http://{hostname}:5000";
            string restKestrelHttpsUrl = $"https://{hostname}:5001";

            int NUM_ITERATIONS = 0;
            int CONCURRENT_REQUESTS = 0;

            bool testWCF = System.Runtime.InteropServices.RuntimeInformation.IsOSPlatform(OSPlatform.Windows);
            bool testReverseProxy = false;
            bool testHttps = false;

            ServiceReference.ServiceClient soapClient = null;
            if (testWCF) {
                // SOAP Client
                soapClient = new ServiceReference.ServiceClient(ServiceReference.ServiceClient.EndpointConfiguration.ServiceWithAnonymousAuthentication);
//            ServiceReference.ServiceClient soapClientHttps = new ServiceReference.ServiceClient(
//                ServiceReference.ServiceClient.EndpointConfiguration.ServiceWithAnonymousAuthentication,
//                "https://localhost/WcfService/Service.svc");
            } else {
                Console.WriteLine("OS is not Windows, skipping WCF test");
            }

            // REST Client
            System.Net.ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
            var filter = new HttpClientHandler();
            filter.ServerCertificateCustomValidationCallback = (request, cert, chain, errors) => { return true; };
            client = new HttpClient(filter);
            client.Timeout = new TimeSpan(0, 1, 0);

            /*
             * Réveiller les services
             */

            NUM_ITERATIONS = 500;
            CONCURRENT_REQUESTS = 10;
            if (testWCF) {
                testSOAP(soapClient, NUM_ITERATIONS, CONCURRENT_REQUESTS, null);
//            testSOAP(soapClientHttps, NUM_ITERATIONS, CONCURRENT_REQUESTS, null);
            }
            if (testReverseProxy) {
                testRestLargeData(restReverseProxyHttpUrl, NUM_ITERATIONS, CONCURRENT_REQUESTS, null);
                if (testHttps) {
                    testRestLargeData(restReverseProxyHttpsUrl, NUM_ITERATIONS, CONCURRENT_REQUESTS, null);
                }
            }
            testRestLargeData(restKestrelHttpUrl, NUM_ITERATIONS, CONCURRENT_REQUESTS, null);
            if (testHttps) {
                testRestLargeData(restKestrelHttpsUrl, NUM_ITERATIONS, CONCURRENT_REQUESTS, null);
            }
            Thread.Sleep(5000);

            /*
             * Données simples
             */

            Console.WriteLine("Données simples, Authentification Anonyme");
            NUM_ITERATIONS = 100000;
            CONCURRENT_REQUESTS = 50;
            Console.WriteLine("Nombre d'itérations: " + NUM_ITERATIONS + ", Requêtes concurrentes: " + CONCURRENT_REQUESTS);
            if (testWCF) {
                testSOAP(soapClient, NUM_ITERATIONS, CONCURRENT_REQUESTS, "SOAP (HTTP)");
                Thread.Sleep(5000);
//            testSOAP(soapClientHttps, NUM_ITERATIONS, CONCURRENT_REQUESTS, "SOAP (HTTPS)");
//            Thread.Sleep(5000);
            }
            if (testReverseProxy) {
                testRest(restReverseProxyHttpUrl, NUM_ITERATIONS, CONCURRENT_REQUESTS, $"REST .Net Core {DOTNET_VERSION} (HTTP, Reverse Proxy)");
                Thread.Sleep(5000);
                if (testHttps) {
                    testRest(restReverseProxyHttpsUrl, NUM_ITERATIONS, CONCURRENT_REQUESTS, $"REST .Net Core {DOTNET_VERSION} (HTTPS, Reverse Proxy)");
                    Thread.Sleep(5000);
                }
            }
            testRest(restKestrelHttpUrl, NUM_ITERATIONS, CONCURRENT_REQUESTS, $"REST .Net Core {DOTNET_VERSION} (HTTP, Kestrel)");
            Thread.Sleep(5000);
            if (testHttps) {
                testRest(restKestrelHttpsUrl, NUM_ITERATIONS, CONCURRENT_REQUESTS, $"REST .Net Core {DOTNET_VERSION} (HTTPS, Kestrel)");
                Thread.Sleep(5000);
            }

            /* 
             * Larges données (20 Ko)
             */

            Console.WriteLine("Larges données, Authentification Anonyme");
            NUM_ITERATIONS = 5000;
            CONCURRENT_REQUESTS = 15;
            Console.WriteLine("Nombre d'itérations: " + NUM_ITERATIONS + ", Requêtes concurrentes: " + CONCURRENT_REQUESTS);
            if (testWCF) {
                testSOAPLargeData(soapClient, NUM_ITERATIONS, CONCURRENT_REQUESTS, "SOAP (Large Data, HTTP)");
                Thread.Sleep(15000);
//            testSOAPLargeData(soapClientHttps, NUM_ITERATIONS, CONCURRENT_REQUESTS, "SOAP (Large Data, HTTPS)");
//            Thread.Sleep(15000);
            }
            if (testReverseProxy) {
                testRestLargeData(restKestrelHttpUrl, NUM_ITERATIONS, CONCURRENT_REQUESTS, $"REST .Net Core {DOTNET_VERSION} (HTTP, Large Data, Kestrel)");
                Thread.Sleep(15000);
                if (testHttps) {
                    testRestLargeData(restKestrelHttpsUrl, NUM_ITERATIONS, CONCURRENT_REQUESTS, $"REST .Net Core {DOTNET_VERSION} (HTTPS, Large Data, Kestrel)");
                    Thread.Sleep(15000);
                }
            }
            testRestLargeData(restReverseProxyHttpsUrl, NUM_ITERATIONS, CONCURRENT_REQUESTS, $"REST .Net Core {DOTNET_VERSION} (HTTP, Large Data, Reverse Proxy)");
            Thread.Sleep(15000);
            if (testHttps) {
                testRestLargeData(restReverseProxyHttpsUrl, NUM_ITERATIONS, CONCURRENT_REQUESTS, $"REST .Net Core {DOTNET_VERSION} (HTTPS, Large Data, Reverse Proxy)");
                Thread.Sleep(15000);
            }

            client.Dispose();
            Console.ReadLine();
        }

        private static void testSOAP(ServiceReference.ServiceClient client, int num_iterations, int concurrent_requests, string identifier)
        {
            var watch = Stopwatch.StartNew();

            var requests_per_thread = num_iterations / concurrent_requests;
            var result = Parallel.For(0, concurrent_requests, (i, state) => {
                for (var j = 0; j < requests_per_thread; j++)
                {
                    var task = client.GetDataAsync("ping");
                    task.Wait();
                    var value = task.Result as string;
                    task.Dispose();
                }
            });
            if (!result.IsCompleted)
            {
                throw new Exception("Parallel execution didn't finish correctly");
            }

            watch.Stop();
            if (identifier != null)
                Console.WriteLine($"{identifier} : {watch.ElapsedMilliseconds} millisecondes");
        }

        private static void testSOAPLargeData(ServiceReference.ServiceClient client, int num_iterations, int concurrent_requests, string identifier)
        {
            var watch = Stopwatch.StartNew();

            var requests_per_thread = num_iterations / concurrent_requests;
            var result = Parallel.For(0, concurrent_requests, (i, state) => {
                for (var j = 0; j < requests_per_thread; j++)
                {
                    var task = client.GetLargeDataAsync();
                    task.Wait();
                    var data = task.Result as ServiceReference.LargeDataStructures;
                    task.Dispose();
                }
            });
            if (!result.IsCompleted)
            {
                throw new Exception("Parallel execution didn't finish correctly");
            }

            watch.Stop();
            if (identifier != null)
                Console.WriteLine($"{identifier} : {watch.ElapsedMilliseconds} millisecondes");
        }

        private static void testRest(string baseUrl, int num_iterations, int concurrent_requests, string identifier)
        {
            var watch = Stopwatch.StartNew();

            var requests_per_thread = num_iterations / concurrent_requests;
            var result = Parallel.For(0, concurrent_requests, (i, state) => {
                for (var j = 0; j < requests_per_thread; j++)
                {
                    try
                    {
                        var task = client.GetStringAsync(baseUrl + "/api/values/");
                        task.Wait();
                        var data = task.Result;
                        task.Dispose();
                    }
                    catch (Exception ex)
                    {
                        // Nouvel essai
                        j--;
                    }
                }
            });
            if (!result.IsCompleted)
            {
                throw new Exception("Parallel execution didn't finish correctly");
            }

            watch.Stop();
            if (identifier != null)
                Console.WriteLine($"{identifier} '{baseUrl}' : {watch.ElapsedMilliseconds}");
        }

        private static List<LargeDataStructure> parseJson(Stream data)
        {
            List<LargeDataStructure> list = null;
            var serializer = new JsonSerializer();

            using (var sr = new StreamReader(data))
            {
                using (var jsonTextReader = new JsonTextReader(sr))
                {
                    list = serializer.Deserialize<List<LargeDataStructure>>(jsonTextReader);
                }
            }

            return list;
        }

        private static void testRestLargeData(string baseUrl, int num_iterations, int concurrent_requests, string identifier)
        {
            var watch = Stopwatch.StartNew();

            var requests_per_thread = num_iterations / concurrent_requests;
            var result = Parallel.For(0, concurrent_requests, (i, state) => {
                for (var j = 0; j < requests_per_thread; j++)
                {
                    try
                    {
                        var task = client.GetStreamAsync(baseUrl + "/api/largevalues/");
                        task.Wait();
                        var data = parseJson(task.Result);
                        task.Dispose();
                    }
                    catch (Exception ex)
                    {
                        // Nouvel essai
                        j--;
                    }
                }
            });
            if (!result.IsCompleted)
            {
                throw new Exception("Parallel execution didn't finish correctly");
            }

            watch.Stop();
            if (identifier != null)
                Console.WriteLine($"{identifier} '{baseUrl}' : {watch.ElapsedMilliseconds}");
        }
    }
}
