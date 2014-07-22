using System;
using System.IO;
using Nancy.Hosting.Self;

namespace Fybr.Server
{
    class Program
    {
        static void Main(string[] args)
        {
            var uri =
                new Uri("http://localhost:8080");

            var hc = new HostConfiguration
            {
                UrlReservations = {CreateAutomatically = true},
            };


            var watcher = new FileSystemWatcher
            {
                Path = Environment.CurrentDirectory,
                NotifyFilter = NotifyFilters.LastAccess | NotifyFilters.LastWrite
                               | NotifyFilters.FileName | NotifyFilters.DirectoryName,
                Filter = "*.*"
            };
            watcher.Changed += OnChanged;
            watcher.Deleted += OnChanged;
            watcher.Created += OnChanged;
            watcher.Renamed += OnChanged;;
            watcher.EnableRaisingEvents = true;
            Brain.Force();
            using (var host = new NancyHost(hc, uri))
            {
                host.Start();

                Console.WriteLine("Your application is running on " + uri);
                Console.WriteLine("Press any [Enter] to close the host.");
                Console.ReadLine();
            }
        }

        private static void OnChanged(object sender, FileSystemEventArgs eventArgs)
        {
            Console.WriteLine(eventArgs.Name + " modified, quitting...");
            Environment.Exit(0);
        }
    }
}
