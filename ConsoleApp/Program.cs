using System;
using System.Diagnostics;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

using System.Collections.Generic;
using System.Runtime.Serialization.Json;
using ConsoleApp.Models;
using System.IO;

namespace ConsoleApp
{
    class Program
    {
        static private HttpClient client = null;

        static void Main(string[] args)
        {
            // SOAP Client
            ServiceReference.ServiceClient soapClient = new ServiceReference.ServiceClient();

            // REST Client
            var filter = new HttpClientHandler();
            filter.ServerCertificateCustomValidationCallback = (request, cert, chain, errors) => { return true; };
            client = new HttpClient(filter);

            int NUM_ITERATIONS = 1;
            int CONCURRENT_REQUESTS = 1;

            NUM_ITERATIONS = 100;
            CONCURRENT_REQUESTS = 1;
            Console.WriteLine("Réveiller les services");
            testSOAP(soapClient, NUM_ITERATIONS, CONCURRENT_REQUESTS);
            testRestReverseProxyHttp(NUM_ITERATIONS, CONCURRENT_REQUESTS);
            testRestReverseProxyHttps(NUM_ITERATIONS, CONCURRENT_REQUESTS);
            testRestKestrelHttp(NUM_ITERATIONS, CONCURRENT_REQUESTS);
            testRestKestrelHttps(NUM_ITERATIONS, CONCURRENT_REQUESTS);

            NUM_ITERATIONS = 100000;
            CONCURRENT_REQUESTS = 100;
            Console.WriteLine("Données simples, Authentification Anonyme");
            Console.WriteLine("Nombre d'itérations: " + NUM_ITERATIONS + ", Requêtes concurrentes: " + CONCURRENT_REQUESTS);
            Thread.Sleep(5000);
            // Test réel
            testSOAP(soapClient, NUM_ITERATIONS, CONCURRENT_REQUESTS);
            Thread.Sleep(5000);
            testRestKestrelHttp(NUM_ITERATIONS, CONCURRENT_REQUESTS);
            Thread.Sleep(5000);
            testRestKestrelHttps(NUM_ITERATIONS, CONCURRENT_REQUESTS);
            Thread.Sleep(5000);
            testRestReverseProxyHttp(NUM_ITERATIONS, CONCURRENT_REQUESTS);
            Thread.Sleep(5000);
            testRestReverseProxyHttps(NUM_ITERATIONS, CONCURRENT_REQUESTS);
            Thread.Sleep(5000);

            NUM_ITERATIONS = 5000;
            CONCURRENT_REQUESTS = 25;
            Console.WriteLine("Larges données, Authentification Anonyme");
            Console.WriteLine("Nombre d'itérations: " + NUM_ITERATIONS + ", Requêtes concurrentes: " + CONCURRENT_REQUESTS);
            testSOAPLargeData(soapClient, NUM_ITERATIONS, CONCURRENT_REQUESTS);
            Thread.Sleep(5000);
            testRESTKestrelHttpLargeData(NUM_ITERATIONS, CONCURRENT_REQUESTS);
            Thread.Sleep(5000);
            testRESTKestrelHttpsLargeData(NUM_ITERATIONS, CONCURRENT_REQUESTS);
            Thread.Sleep(5000);
            testRESTReverseProxyHttpLargeData(NUM_ITERATIONS, CONCURRENT_REQUESTS);
            Thread.Sleep(5000);
            testRESTReverseProxyHttpsLargeData(NUM_ITERATIONS, CONCURRENT_REQUESTS);

            client.Dispose();
            Console.ReadLine();
        }

        static void testSOAP(ServiceReference.ServiceClient client, int num_iterations, int concurrent_requests)
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
            Console.WriteLine("SOAP: " + watch.ElapsedMilliseconds);
        }

        static void testSOAPLargeData(ServiceReference.ServiceClient client, int num_iterations, int concurrent_requests)
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
            Console.WriteLine("SOAP (Large Data): " + watch.ElapsedMilliseconds);
        }

        static void testRestReverseProxyHttp(int num_iterations, int concurrent_requests)
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

                tasks[pos] = client.GetStringAsync("http://localhost/RestService/api/values/");
                i++;
                n++;
            }
            Task.WaitAll(tasks);
            watch.Stop();
            Console.WriteLine("REST HTTP .Net Core 2.0 (Reverse Proxy): " + watch.ElapsedMilliseconds);
        }

        static void testRestReverseProxyHttps(int num_iterations, int concurrent_requests)
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

                tasks[pos] = client.GetStringAsync("https://localhost/RestService/api/values/");
                i++;
                n++;
            }
            Task.WaitAll(tasks);
            watch.Stop();
            Console.WriteLine("REST HTTPS .Net Core 2.0 Reverse Proxy: " + watch.ElapsedMilliseconds);
        }

        static void testRestKestrelHttp(int num_iterations, int concurrent_requests)
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

                tasks[pos] = client.GetStringAsync("http://localhost:5000/api/values/");
                i++;
                n++;
            }
            Task.WaitAll(tasks);
            watch.Stop();
            Console.WriteLine("REST HTTP .Net Core 2.0 Kestrel: " + watch.ElapsedMilliseconds);
        }

        static void testRestKestrelHttps(int num_iterations, int concurrent_requests)
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

                tasks[pos] = client.GetStringAsync("https://localhost:5001/api/values/");
                i++;
                n++;
            }
            Task.WaitAll(tasks);
            watch.Stop();
            Console.WriteLine("REST HTTPS .Net Core 2.0 Kestrel: " + watch.ElapsedMilliseconds);
        }

        static void testRESTReverseProxyHttpLargeData(int num_iterations, int concurrent_requests)
        {
            var serializer = new DataContractJsonSerializer(typeof(List<LargeDataStructure>));

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
                    var list = serializer.ReadObject(tasks[pos].Result) as List<LargeDataStructure>;
                    tasks[pos].Dispose();
                    tasks[pos] = null;
                    n--;
                }
                else
                    pos++;

                tasks[pos] = client.GetStreamAsync("http://localhost/RestService/api/largevalues/");
                i++;
                n++;
            }
            Task.WaitAll(tasks);
            watch.Stop();
            Console.WriteLine("REST HTTP .Net Core 2.0 ReverseProxy (Large Data): " + watch.ElapsedMilliseconds);
        }

        static void testRESTReverseProxyHttpsLargeData(int num_iterations, int concurrent_requests)
        {
            var serializer = new DataContractJsonSerializer(typeof(List<LargeDataStructure>));

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
                    var list = serializer.ReadObject(tasks[pos].Result) as List<LargeDataStructure>;
                    tasks[pos].Dispose();
                    tasks[pos] = null;
                    n--;
                }
                else
                    pos++;

                tasks[pos] = client.GetStreamAsync("https://localhost/RestService/api/largevalues/");
                i++;
                n++;
            }
            Task.WaitAll(tasks);
            watch.Stop();
            Console.WriteLine("REST HTTPS .Net Core 2.0 ReverseProxy (Large Data): " + watch.ElapsedMilliseconds);
        }

        static void testRESTKestrelHttpLargeData(int num_iterations, int concurrent_requests)
        {
            var serializer = new DataContractJsonSerializer(typeof(List<LargeDataStructure>));

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
                    var list = serializer.ReadObject(tasks[pos].Result) as List<LargeDataStructure>;
                    tasks[pos].Dispose();
                    tasks[pos] = null;
                    n--;
                }
                else
                    pos++;

                tasks[pos] = client.GetStreamAsync("http://localhost:5000/api/largevalues/");
                i++;
                n++;
            }
            Task.WaitAll(tasks);
            watch.Stop();
            Console.WriteLine("REST HTTP .Net Core 2.0 Kestrel (Large Data): " + watch.ElapsedMilliseconds);
        }

        static void testRESTKestrelHttpsLargeData(int num_iterations, int concurrent_requests)
        {
            var serializer = new DataContractJsonSerializer(typeof(List<LargeDataStructure>));

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
                    var list = serializer.ReadObject(tasks[pos].Result) as List<LargeDataStructure>;
                    tasks[pos].Dispose();
                    tasks[pos] = null;
                    n--;
                }
                else
                    pos++;

                tasks[pos] = client.GetStreamAsync("https://localhost:5001/api/largevalues/");
                i++;
                n++;
            }
            Task.WaitAll(tasks);
            watch.Stop();
            Console.WriteLine("REST HTTPS .Net Core 2.0 Kestrel (Large Data): " + watch.ElapsedMilliseconds);
        }
    }
}
