using System.Net;
using Microsoft.AspNetCore.Builder;
using Orleans.Runtime.Configuration;
using Orleans.Runtime.Host;
using Orleans.Storage;

namespace Deepflow.Platform.Silo
{
    public static class OrleansExtensions
    {
        public static void UseOrleans<TStartup>(this IApplicationBuilder app, int port)
        {
            var config = ClusterConfiguration.LocalhostPrimarySilo();
            config.UseStartupType<TStartup>();

            var siloHost = new SiloHost(Dns.GetHostName(), config);
            siloHost.DeploymentId = "Test";
            siloHost.Name = "Test";
            siloHost.Type = Orleans.Runtime.Silo.SiloType.Primary;
            siloHost.InitializeOrleansSilo();
            siloHost.StartOrleansSilo();
        }
    }
}
