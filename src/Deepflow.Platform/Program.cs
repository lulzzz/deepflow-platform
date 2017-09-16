using System;
using System.IO;
using Microsoft.AspNetCore.Hosting;

namespace Deepflow.Platform
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var hostBuilder = new WebHostBuilder()
                .UseKestrel()
                .UseUrls("http://0.0.0.0:5001/")
                .UseContentRoot(Directory.GetCurrentDirectory())
                .UseStartup<Startup>();

            Console.WriteLine("urls: {0}", hostBuilder.GetSetting("urls"));

            var host = hostBuilder.Build();


            host.Run();
        }
    }
}
