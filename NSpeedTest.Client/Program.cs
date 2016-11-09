using System;
using System.Collections.Generic;
using System.Linq;
using NSpeedTest.Models;
using System.IO;

namespace NSpeedTest.Client
{
    class Program
    {
        private static SpeedTestClient client;
        private static Settings settings;
        private const string DefaultCountry = "Belgium";
        private const string ResultPath = @"C:/Users/Admin/Documents/speedresult.txt";

        static void Main()
        {
            Console.WriteLine("Getting speedtest.net settings and server list...");
            client = new SpeedTestClient();
            settings = client.GetSettings();
            
            IEnumerable<Server> servers = SelectServers();
            Server bestServer = SelectBestServer(servers);
            
            Console.WriteLine("Testing speed...");
            double downloadSpeed = client.TestDownloadSpeed(bestServer, settings.Download.ThreadsPerUrl);
            PrintSpeed("Download", downloadSpeed);
            double uploadSpeed = client.TestUploadSpeed(bestServer, settings.Upload.ThreadsPerUrl);
            PrintSpeed("Upload", uploadSpeed);

            //Opslaan van resultaten.
            Console.WriteLine("");
            Console.WriteLine("Opslaan van resultaat");
            SaveSpeed("Download", downloadSpeed);
            SaveSpeed("Upload", uploadSpeed);

            Console.WriteLine("Press a key to exit.");
            Console.ReadKey();
        }

        private static Server SelectBestServer(IEnumerable<Server> servers)
        {
            Console.WriteLine();
            Console.WriteLine("Best server by latency:");
            Server bestServer = servers.OrderBy(x => x.Latency).First();
            PrintServerDetails(bestServer);
            Console.WriteLine();
            return bestServer;
        }

        private static IEnumerable<Server> SelectServers()
        {
            Console.WriteLine();
            Console.WriteLine("Selecting best server by distance...");
            List<Server> servers = settings.Servers.Where(s => s.Country.Equals(DefaultCountry)).Take(10).ToList();

            foreach (Server server in servers)
            {
                server.Latency = client.TestServerLatency(server);
                PrintServerDetails(server);
            }
            return servers;
        }

        private static void PrintServerDetails(Server server)
        {
            Console.WriteLine("Hosted by {0} ({1}/{2}), distance: {3}km, latency: {4}ms", server.Sponsor, server.Name,
                server.Country, (int) server.Distance/1000, server.Latency);
        }

        private static void PrintSpeed(string type, double speed)
        {
            if (speed > 1024)
            {
                Console.WriteLine("{0} speed: {1} Mbps", type, Math.Round(speed / 1024, 2));
            }
            else
            {
                Console.WriteLine("{0} speed: {1} Kbps", type, Math.Round(speed, 2));
            }
        }

        private static void SaveSpeed(string type, double speed)
        {
            using (StreamWriter file = new StreamWriter(ResultPath,true))//File openen en tekst bijvoegen.
            {
                if (speed > 1024)
                {
                    file.WriteLine("{0} speed: {1} Mbps at {2} {3}", type, Math.Round(speed / 1024, 2), DateTime.Now.ToShortDateString(), DateTime.Now.ToShortTimeString());
                }
                else
                {
                    file.WriteLine("{0} speed: {1} Kbps at {2} {3}", type, Math.Round(speed, 2), DateTime.Now.ToShortDateString(), DateTime.Now.ToShortTimeString());
                }
            }
        }
    }
}
