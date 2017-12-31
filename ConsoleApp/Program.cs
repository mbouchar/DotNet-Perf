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

namespace ConsoleApp
{
    class Program
    {
        static private HttpClient client = null;

        /* 
         * set ASPNETCORE_ENVIRONMENT=Production
         * dotnet build -c Release
         * dotnet run -c Release --no-build --no-restore
         * 
         */
        static void Main(string[] args)
        {
            // SOAP Client
            ServiceReference.ServiceClient soapClient = new ServiceReference.ServiceClient(ServiceReference.ServiceClient.EndpointConfiguration.ServiceWithAnonymousAuthentication);
//            ServiceReference.ServiceClient soapClientHttps = new ServiceReference.ServiceClient(
//                ServiceReference.ServiceClient.EndpointConfiguration.ServiceWithAnonymousAuthentication,
//                "https://localhost/WcfService/Service.svc");

            // REST Client
            var filter = new HttpClientHandler();
            filter.ServerCertificateCustomValidationCallback = (request, cert, chain, errors) => { return true; };
            client = new HttpClient(filter);

            int NUM_ITERATIONS = 1;
            int CONCURRENT_REQUESTS = 1;

            NUM_ITERATIONS = 500;
            CONCURRENT_REQUESTS = 1;
            // Réveiller les services
            testSOAP(soapClient, NUM_ITERATIONS, CONCURRENT_REQUESTS, null);
   //         testSOAP(soapClientHttps, NUM_ITERATIONS, CONCURRENT_REQUESTS, null);
            testRestLargeData("http://localhost/RestService", NUM_ITERATIONS, CONCURRENT_REQUESTS, null);
            testRestLargeData("https://localhost/RestService", NUM_ITERATIONS, CONCURRENT_REQUESTS, null);
            testRestLargeData("http://localhost:5000", NUM_ITERATIONS, CONCURRENT_REQUESTS, null);
            testRestLargeData("https://localhost:5001", NUM_ITERATIONS, CONCURRENT_REQUESTS, null);

            /* Données simples
             */

            Console.WriteLine("Données simples, Authentification Anonyme");
            NUM_ITERATIONS = 100000;
            CONCURRENT_REQUESTS = 100;
            Console.WriteLine("Nombre d'itérations: " + NUM_ITERATIONS + ", Requêtes concurrentes: " + CONCURRENT_REQUESTS);
            Thread.Sleep(5000);
            // Test réel
            testSOAP(soapClient, NUM_ITERATIONS, CONCURRENT_REQUESTS, "SOAP (HTTP)");
     //       testSOAP(soapClientHttps, NUM_ITERATIONS, CONCURRENT_REQUESTS, "SOAP (HTTPS)");
            Thread.Sleep(5000);
            testRest("http://localhost/RestService", NUM_ITERATIONS, CONCURRENT_REQUESTS, "REST .Net Core 2.0 (HTTP, Reverse Proxy)");
            Thread.Sleep(5000);
            testRest("https://localhost/RestService", NUM_ITERATIONS, CONCURRENT_REQUESTS, "REST .Net Core 2.0 (HTTPS, Reverse Proxy)");
            Thread.Sleep(5000);
            testRest("http://localhost:5000", NUM_ITERATIONS, CONCURRENT_REQUESTS, "REST .Net Core 2.0 (HTTP, Kestrel)");
            Thread.Sleep(5000);
            testRest("https://localhost:5001", NUM_ITERATIONS, CONCURRENT_REQUESTS, "REST .Net Core 2.0 (HTTPS, Kestrel)");
            Thread.Sleep(5000);

            /* Larges données (20 Ko)
             */
            Console.WriteLine("Larges données, Authentification Anonyme");
            NUM_ITERATIONS = 5000;
            CONCURRENT_REQUESTS = 25;
            Console.WriteLine("Nombre d'itérations: " + NUM_ITERATIONS + ", Requêtes concurrentes: " + CONCURRENT_REQUESTS);
            testSOAPLargeData(soapClient, NUM_ITERATIONS, CONCURRENT_REQUESTS, "SOAP (Large Data, HTTP)");
     //       testSOAPLargeData(soapClientHttps, NUM_ITERATIONS, CONCURRENT_REQUESTS, "SOAP (Large Data, HTTPS)");
            Thread.Sleep(5000);
            testRestLargeData("http://localhost/RestService", NUM_ITERATIONS, CONCURRENT_REQUESTS, "REST .Net Core 2.0 (HTTP, Large Data, Reverse Proxy)");
            Thread.Sleep(5000);
            testRestLargeData("https://localhost/RestService", NUM_ITERATIONS, CONCURRENT_REQUESTS, "REST .Net Core 2.0 (HTTPS, Large Data, Reverse Proxy)");
            Thread.Sleep(5000);
            testRestLargeData("http://localhost:5000", NUM_ITERATIONS, CONCURRENT_REQUESTS, "REST .Net Core 2.0 (HTTP, Large Data, Kestrel)");
            Thread.Sleep(5000);
            testRestLargeData("https://localhost:5001", NUM_ITERATIONS, CONCURRENT_REQUESTS, "REST .Net Core 2.0 (HTTPS, Large Data, Kestrel)");

            client.Dispose();
            Console.ReadLine();
        }

        static void testSOAP(ServiceReference.ServiceClient client, int num_iterations, int concurrent_requests, string identifier)
        {
            var watch = Stopwatch.StartNew();
            var tasks = new Task<string>[concurrent_requests];
            var i = 0;
            var n = 0;
            var pos = -1;
            while (i < num_iterations)
            {
                if (n >= concurrent_requests)
                {
                    pos = Task.WaitAny(tasks);
                    var value = tasks[pos].Result as string;
                    tasks[pos].Dispose();
                    tasks[pos] = null;
                    n--;
                }
                else
                    pos++;

                tasks[pos] = client.GetDataAsync("ping");
                i++;
                n++;
            }
            Task.WaitAll(tasks);
            watch.Stop();

            if (identifier != null)
                Console.WriteLine(identifier + ": " + watch.ElapsedMilliseconds);
        }

        static void testSOAPLargeData(ServiceReference.ServiceClient client, int num_iterations, int concurrent_requests, string identifier)
        {
            var watch = Stopwatch.StartNew();
            var tasks = new Task<ServiceReference.LargeDataStructures>[concurrent_requests];
            var i = 0;
            var n = 0;
            var pos = -1;
            while (i < num_iterations)
            {
                if (n >= concurrent_requests)
                {
                    pos = Task.WaitAny(tasks);
                    var list = tasks[pos].Result as ServiceReference.LargeDataStructures;
                    tasks[pos].Dispose();
                    tasks[pos] = null;
                    n--;
                }
                else
                    pos++;

                tasks[pos] = client.GetLargeDataAsync();
                i++;
                n++;
            }
            Task.WaitAll(tasks);
            watch.Stop();

            if (identifier != null)
                Console.WriteLine(identifier + ": " + watch.ElapsedMilliseconds);
        }

        static void testRest(string baseUrl, int num_iterations, int concurrent_requests, string identifier)
        {
            var watch = Stopwatch.StartNew();
            var tasks = new Task<string>[concurrent_requests];
            var i = 0;
            var n = 0;
            var pos = -1;
            while (i < num_iterations)
            {
                if (n >= concurrent_requests)
                {
                    pos = Task.WaitAny(tasks);
                    var value = tasks[pos].Result as string;
                    tasks[pos].Dispose();
                    tasks[pos] = null;
                    n--;
                }
                else
                    pos++;

                tasks[pos] = client.GetStringAsync(baseUrl + "/api/values/");
                i++;
                n++;
            }
            Task.WaitAll(tasks);
            watch.Stop();
            Console.WriteLine(identifier + ": " + watch.ElapsedMilliseconds);
        }

        static void testRestLargeData(string baseUrl, int num_iterations, int concurrent_requests, string identifier)
        {
            var serializer = new JsonSerializer();

            var watch = Stopwatch.StartNew();
            var tasks = new Task<Stream>[concurrent_requests];
            var i = 0;
            var n = 0;
            var pos = -1;
            while (i < num_iterations)
            {
                if (n >= concurrent_requests)
                {
                    pos = Task.WaitAny(tasks);
                    using (var sr = new StreamReader(tasks[pos].Result))
                    {
                        using (var jsonTextReader = new JsonTextReader(sr))
                        {
                            var list = serializer.Deserialize<List<LargeDataStructure>>(jsonTextReader);
                        }
                    }
                    tasks[pos].Dispose();
                    tasks[pos] = null;
                    n--;
                }
                else
                    pos++;

                tasks[pos] = client.GetStreamAsync(baseUrl + "/api/largevalues/");
                i++;
                n++;
            }
            Task.WaitAll(tasks);
            watch.Stop();

            if (identifier != null)
                Console.WriteLine(identifier + ": " + watch.ElapsedMilliseconds);
        }
    }
}
