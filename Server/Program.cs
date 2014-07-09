using System;
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
            using (var host = new NancyHost(hc, uri))
            {
                host.Start();

                Console.WriteLine("Your application is running on " + uri);
                Console.WriteLine("Press any [Enter] to close the host.");
                Console.ReadLine();
            }
        }
    }
}
