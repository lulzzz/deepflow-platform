/*using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net;
using Microsoft.Extensions.DependencyInjection;*/

namespace Deepflow.Platform.Console
{
    class Program
    {
        static void Main(string[] args)
        {
            /*var config = ClusterConfiguration.LocalhostPrimarySilo();
            //config.UseStartupType<TStartup>();

            var siloHost = new SiloHost(Dns.GetHostName(), config);
            siloHost.DeploymentId = "Test";
            siloHost.Name = "Test";
            siloHost.Type = Orleans.Runtime.Silo.SiloType.Primary;
            siloHost.InitializeOrleansSilo();
            siloHost.StartOrleansSilo();
            
            while (true)
            {
                using (var stopwatch = Stopwatch.StartNew())
                {
                    ITestGrain grain = GrainClient.GrainFactory.GetGrain<ITestGrain>(Guid.NewGuid());
                    var thing = grain.GetThing();
                    Console.WriteLine(stopwatch.ElapsedMilliseconds);
                }
            }*/
            

            //Console.WriteLine("Hello World!");
        }
    }
/*
    public interface ITestGrain
    {
        string GetThing();
    }

    public class TestGrain : Grain, ITestGrain, IGrainWithGuidKey
    {
        public string GetThing()
        {
            return "Thing";
        }
    }*/

    /*public class Startup
    {
        public OrleansStartup()
        {

        }

        public IServiceProvider ConfigureServices(IServiceCollection services)
        {
            
            return services.BuildServiceProvider();
        }
    }*/
}