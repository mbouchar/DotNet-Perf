using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net.Http;
using System.Threading.Tasks;

namespace LegacyConsoleApp
{
    class Program
    {
        static void Main(string[] args)
        {
            // SOAP Client
            ServiceReference.ServiceClient soapClient = new ServiceReference.ServiceClient("ServiceWithAnonymousAuthentication");
            // REST Client
            var restClient = new HttpClient();
            restClient.DefaultRequestHeaders.Accept.Clear();
            restClient.DefaultRequestHeaders.Add("User-Agent", ".NET Foundation Repository Reporter");

            int NUM_ITERATIONS = 1;
            int CONCURRENT_REQUESTS = 1;

            NUM_ITERATIONS = 100000;
            CONCURRENT_REQUESTS = 25;
            Console.WriteLine("Données simples, Authentification Anonyme");
            Console.WriteLine("Nombre d'itérations: " + NUM_ITERATIONS + ", Requêtes concurrentes: " + CONCURRENT_REQUESTS);
            //testSOAPSync(soapClient, NUM_ITERATIONS);
            testSOAPAsync(soapClient, NUM_ITERATIONS, CONCURRENT_REQUESTS);
            testRestIISHttp(restClient, NUM_ITERATIONS, CONCURRENT_REQUESTS);
            testRestIISHttps(restClient, NUM_ITERATIONS, CONCURRENT_REQUESTS);
            testRestKestrelHttp(restClient, NUM_ITERATIONS, CONCURRENT_REQUESTS);
            testRestKestrelHttps(restClient, NUM_ITERATIONS, CONCURRENT_REQUESTS);

            NUM_ITERATIONS = 5000;
            CONCURRENT_REQUESTS = 15;
            Console.WriteLine("Larges données, Authentification Anonyme");
            Console.WriteLine("Nombre d'itérations: " + NUM_ITERATIONS + ", Requêtes concurrentes: " + CONCURRENT_REQUESTS);
            testSOAPLargeDataAsync(soapClient, NUM_ITERATIONS, CONCURRENT_REQUESTS);
            testRESTIISHttpLargeDataAsync(restClient, NUM_ITERATIONS, CONCURRENT_REQUESTS);
            testRESTIISHttpsLargeDataAsync(restClient, NUM_ITERATIONS, CONCURRENT_REQUESTS);
            testRESTKestrelHttpLargeDataAsync(restClient, NUM_ITERATIONS, CONCURRENT_REQUESTS);
            testRESTKestrelHttpsLargeDataAsync(restClient, NUM_ITERATIONS, CONCURRENT_REQUESTS);
            Console.ReadLine();
        }

        static void testSOAPSync(ServiceReference.ServiceClient client, int num_iterations, int concurrent_requests)
        {
            var watch = Stopwatch.StartNew();
            for (var i = 0; i < num_iterations; i++)
            {
                client.GetData("ping");
            }
            watch.Stop();
            Console.WriteLine("SOAP Sync: " + watch.ElapsedMilliseconds);
        }

        static void testSOAPAsync(ServiceReference.ServiceClient client, int num_iterations, int concurrent_requests)
        {
            var watch = Stopwatch.StartNew();
            var tasks = new Task[concurrent_requests];
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
            Console.WriteLine("SOAP Async: " + watch.ElapsedMilliseconds);
        }

        static void testSOAPLargeDataAsync(ServiceReference.ServiceClient client, int num_iterations, int concurrent_requests)
        {
            var watch = Stopwatch.StartNew();
            var tasks = new Task[concurrent_requests];
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
            Console.WriteLine("SOAP (Large Data) Async: " + watch.ElapsedMilliseconds);
        }

        static void testRestIISHttp(HttpClient client, int num_iterations, int concurrent_requests)
        {
            var watch = Stopwatch.StartNew();
            var tasks = new Task[concurrent_requests];
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

                tasks[pos] = client.GetStringAsync("http://localhost/RestService/api/values/");
                i++;
                n++;
            }
            Task.WaitAll(tasks);
            watch.Stop();
            Console.WriteLine("REST HTTP .Net Core 2.0 IIS Async: " + watch.ElapsedMilliseconds);
        }

        static void testRestIISHttps(HttpClient client, int num_iterations, int concurrent_requests)
        {
            var watch = Stopwatch.StartNew();
            var tasks = new Task[concurrent_requests];
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

                tasks[pos] = client.GetStringAsync("https://localhost/RestService/api/values/");
                i++;
                n++;
            }
            Task.WaitAll(tasks);
            watch.Stop();
            Console.WriteLine("REST HTTPS .Net Core 2.0 IIS Async: " + watch.ElapsedMilliseconds);
        }

        static void testRestKestrelHttp(HttpClient client, int num_iterations, int concurrent_requests)
        {
            var watch = Stopwatch.StartNew();
            var tasks = new Task[concurrent_requests];
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

                tasks[pos] = client.GetStringAsync("http://localhost:5000/api/values/");
                i++;
                n++;
            }
            Task.WaitAll(tasks);
            watch.Stop();
            Console.WriteLine("REST HTTP .Net Core 2.0 Kestrel Async: " + watch.ElapsedMilliseconds);
        }

        static void testRestKestrelHttps(HttpClient client, int num_iterations, int concurrent_requests)
        {
            var watch = Stopwatch.StartNew();
            var tasks = new Task[concurrent_requests];
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

                tasks[pos] = client.GetStringAsync("https://localhost:5001/api/values/");
                i++;
                n++;
            }
            Task.WaitAll(tasks);
            watch.Stop();
            Console.WriteLine("REST HTTPS .Net Core 2.0 Kestrel Async: " + watch.ElapsedMilliseconds);
        }

        static void testRESTIISHttpLargeDataAsync(HttpClient client, int num_iterations, int concurrent_requests)
        {
            var watch = Stopwatch.StartNew();
            var tasks = new Task[concurrent_requests];
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

                tasks[pos] = client.GetStringAsync("http://localhost/RestService/api/largevalues/");
                i++;
                n++;
            }
            Task.WaitAll(tasks);
            watch.Stop();
            Console.WriteLine("REST HTTP .Net Core 2.0 IIS (Large Data) Async: " + watch.ElapsedMilliseconds);
        }

        static void testRESTIISHttpsLargeDataAsync(HttpClient client, int num_iterations, int concurrent_requests)
        {
            var watch = Stopwatch.StartNew();
            var tasks = new Task[concurrent_requests];
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

                tasks[pos] = client.GetStringAsync("https://localhost/RestService/api/largevalues/");
                i++;
                n++;
            }
            Task.WaitAll(tasks);
            watch.Stop();
            Console.WriteLine("REST HTTPS .Net Core 2.0 IIS (Large Data) Async: " + watch.ElapsedMilliseconds);
        }

        static void testRESTKestrelHttpLargeDataAsync(HttpClient client, int num_iterations, int concurrent_requests)
        {
            var watch = Stopwatch.StartNew();
            var tasks = new Task[concurrent_requests];
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

                tasks[pos] = client.GetStringAsync("http://localhost:5000/api/largevalues/");
                i++;
                n++;
            }
            Task.WaitAll(tasks);
            watch.Stop();
            Console.WriteLine("REST HTTP .Net Core 2.0 Kestrel (Large Data) Async: " + watch.ElapsedMilliseconds);
        }

        static void testRESTKestrelHttpsLargeDataAsync(HttpClient client, int num_iterations, int concurrent_requests)
        {
            var watch = Stopwatch.StartNew();
            var tasks = new Task[concurrent_requests];
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

                tasks[pos] = client.GetStringAsync("https://localhost:5001/api/largevalues/");
                i++;
                n++;
            }
            Task.WaitAll(tasks);
            watch.Stop();
            Console.WriteLine("REST HTTPS .Net Core 2.0 Kestrel (Large Data) Async: " + watch.ElapsedMilliseconds);
        }
    }
}
