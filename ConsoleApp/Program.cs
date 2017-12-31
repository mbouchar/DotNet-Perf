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

namespace ConsoleApp
{
    class Program
    {
        private static HttpClient client = null;

        private static string DEFAULT_HOSTNAME = "localhost";

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

            string restReverseProxyHttpUrl = "http://" + hostname + "/RestService";
            string restReverseProxyHttpsUrl = "https://" + hostname + "/RestService";
            string restKestrelHttpUrl = "http://" + hostname + ":5000";
            string restKestrelHttpsUrl = "https://" + hostname + ":5001";

            bool testWCF = System.Runtime.InteropServices.RuntimeInformation.IsOSPlatform(OSPlatform.Windows);
            ServiceReference.ServiceClient soapClient = null;

            if (testWCF)
            {
                // SOAP Client
                soapClient = new ServiceReference.ServiceClient(ServiceReference.ServiceClient.EndpointConfiguration.ServiceWithAnonymousAuthentication);
//            ServiceReference.ServiceClient soapClientHttps = new ServiceReference.ServiceClient(
//                ServiceReference.ServiceClient.EndpointConfiguration.ServiceWithAnonymousAuthentication,
//                "https://localhost/WcfService/Service.svc");
            }

            // REST Client
            var filter = new HttpClientHandler();
            filter.ServerCertificateCustomValidationCallback = (request, cert, chain, errors) => { return true; };
            client = new HttpClient(filter);

            int NUM_ITERATIONS = 0;
            int CONCURRENT_REQUESTS = 0;

            /*
             * Réveiller les services
             */

            NUM_ITERATIONS = 500;
            CONCURRENT_REQUESTS = 10;
            if (testWCF)
            {
                testSOAP(soapClient, NUM_ITERATIONS, CONCURRENT_REQUESTS, null);
//            testSOAP(soapClientHttps, NUM_ITERATIONS, CONCURRENT_REQUESTS, null);
            }
            testRest(restReverseProxyHttpUrl, NUM_ITERATIONS, CONCURRENT_REQUESTS, null);
            testRestLargeData(restReverseProxyHttpUrl, NUM_ITERATIONS, CONCURRENT_REQUESTS, null);
            testRestLargeData(restReverseProxyHttpsUrl, NUM_ITERATIONS, CONCURRENT_REQUESTS, null);
            testRestLargeData(restKestrelHttpUrl, NUM_ITERATIONS, CONCURRENT_REQUESTS, null);
            testRestLargeData(restKestrelHttpsUrl, NUM_ITERATIONS, CONCURRENT_REQUESTS, null);
            Thread.Sleep(5000);

            /*
             * Données simples
             */

            Console.WriteLine("Données simples, Authentification Anonyme");
            NUM_ITERATIONS = 100000;
            CONCURRENT_REQUESTS = 100;
            Console.WriteLine("Nombre d'itérations: " + NUM_ITERATIONS + ", Requêtes concurrentes: " + CONCURRENT_REQUESTS);
            if (testWCF)
            {
                testSOAP(soapClient, NUM_ITERATIONS, CONCURRENT_REQUESTS, "SOAP (HTTP)");
                Thread.Sleep(5000);
//            testSOAP(soapClientHttps, NUM_ITERATIONS, CONCURRENT_REQUESTS, "SOAP (HTTPS)");
//            Thread.Sleep(5000);
            }
            testRest(restReverseProxyHttpUrl, NUM_ITERATIONS, CONCURRENT_REQUESTS, "REST .Net Core 2.0 (HTTP, Reverse Proxy)");
            Thread.Sleep(5000);
            testRest(restReverseProxyHttpsUrl, NUM_ITERATIONS, CONCURRENT_REQUESTS, "REST .Net Core 2.0 (HTTPS, Reverse Proxy)");
            Thread.Sleep(5000);
            testRest(restKestrelHttpUrl, NUM_ITERATIONS, CONCURRENT_REQUESTS, "REST .Net Core 2.0 (HTTP, Kestrel)");
            Thread.Sleep(5000);
            testRest(restKestrelHttpsUrl, NUM_ITERATIONS, CONCURRENT_REQUESTS, "REST .Net Core 2.0 (HTTPS, Kestrel)");
            Thread.Sleep(5000);

            /* 
             * Larges données (20 Ko)
             */

            Console.WriteLine("Larges données, Authentification Anonyme");
            NUM_ITERATIONS = 5000;
            CONCURRENT_REQUESTS = 25;
            Console.WriteLine("Nombre d'itérations: " + NUM_ITERATIONS + ", Requêtes concurrentes: " + CONCURRENT_REQUESTS);
            if (testWCF)
            {
                testSOAPLargeData(soapClient, NUM_ITERATIONS, CONCURRENT_REQUESTS, "SOAP (Large Data, HTTP)");
                Thread.Sleep(5000);
//            testSOAPLargeData(soapClientHttps, NUM_ITERATIONS, CONCURRENT_REQUESTS, "SOAP (Large Data, HTTPS)");
//            Thread.Sleep(5000);
            }
            testRestLargeData(restReverseProxyHttpsUrl, NUM_ITERATIONS, CONCURRENT_REQUESTS, "REST .Net Core 2.0 (HTTP, Large Data, Reverse Proxy)");
            Thread.Sleep(5000);
            testRestLargeData(restReverseProxyHttpsUrl, NUM_ITERATIONS, CONCURRENT_REQUESTS, "REST .Net Core 2.0 (HTTPS, Large Data, Reverse Proxy)");
            Thread.Sleep(5000);
            testRestLargeData(restKestrelHttpUrl, NUM_ITERATIONS, CONCURRENT_REQUESTS, "REST .Net Core 2.0 (HTTP, Large Data, Kestrel)");
            Thread.Sleep(5000);
            testRestLargeData(restKestrelHttpsUrl, NUM_ITERATIONS, CONCURRENT_REQUESTS, "REST .Net Core 2.0 (HTTPS, Large Data, Kestrel)");

            client.Dispose();
            Console.ReadLine();
        }

        private static void testSOAP(ServiceReference.ServiceClient client, int num_iterations, int concurrent_requests, string identifier)
        {
            var watch = Stopwatch.StartNew();

            var requests_per_thread = num_iterations / concurrent_requests;
            var result = Parallel.For(0, concurrent_requests, (i, state) => {
                var task = client.GetDataAsync("ping");
                task.Wait();
                var value = task.Result as string;
                task.Dispose();
            });
            if (!result.IsCompleted)
            {
                throw new Exception("Parallel execution didn't finish correctly");
            }

            watch.Stop();
            if (identifier != null)
                Console.WriteLine(identifier + ": " + watch.ElapsedMilliseconds);
        }

        private static void testSOAPLargeData(ServiceReference.ServiceClient client, int num_iterations, int concurrent_requests, string identifier)
        {
            var watch = Stopwatch.StartNew();

            var requests_per_thread = num_iterations / concurrent_requests;
            var result = Parallel.For(0, concurrent_requests, (i, state) => {
                var task = client.GetLargeDataAsync();
                task.Wait();
                var data = task.Result as ServiceReference.LargeDataStructures;
                task.Dispose();
            });
            if (!result.IsCompleted)
            {
                throw new Exception("Parallel execution didn't finish correctly");
            }

            watch.Stop();
            if (identifier != null)
                Console.WriteLine(identifier + ": " + watch.ElapsedMilliseconds);
        }

        private static void testRest(string baseUrl, int num_iterations, int concurrent_requests, string identifier)
        {
            var watch = Stopwatch.StartNew();

            var requests_per_thread = num_iterations / concurrent_requests;
            var result = Parallel.For(0, concurrent_requests, (i, state) => {
                for (var j = 0; j < requests_per_thread; j++)
                {
                    var task = client.GetStringAsync(baseUrl + "/api/values/");
                    task.Wait();
                    var data = task.Result;
                    task.Dispose();
                }
            });
            if (!result.IsCompleted)
            {
                throw new Exception("Parallel execution didn't finish correctly");
            }

            watch.Stop();
            if (identifier != null)
                Console.WriteLine(identifier + ": " + watch.ElapsedMilliseconds);
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
                    var task = client.GetStreamAsync(baseUrl + "/api/largevalues/");
                    task.Wait();
                    var data = parseJson(task.Result);
                    task.Dispose();
                }
            });
            if (!result.IsCompleted)
            {
                throw new Exception("Parallel execution didn't finish correctly");
            }

            watch.Stop();
            if (identifier != null)
                Console.WriteLine(identifier + ": " + watch.ElapsedMilliseconds);
        }
    }
}
