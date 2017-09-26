using System;
using System.Threading.Tasks;
using Deepflow.Platform.Abstractions.Realtime;
using Deepflow.Platform.Abstractions.Series;
using Deepflow.Platform.Core.Tools;
using Deepflow.Platform.Realtime;
using Deepflow.Platform.Series;
using Deepflow.Platform.Sources.FakeSource.Data;
using Deepflow.Platform.Sources.FakeSource.Realtime;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Deepflow.Platform.Sources.FakeSource
{
    public class Startup
    {
        public Startup(IHostingEnvironment env)
        {
            var builder = new ConfigurationBuilder()
                .SetBasePath(env.ContentRootPath)
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .AddJsonFile($"appsettings.{env.EnvironmentName}.json", optional: true)
                .AddEnvironmentVariables();
            Configuration = builder.Build();
        }

        public IConfigurationRoot Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            // Add framework services.
            services.AddMvc();
            services.AddSingleton<IWebsocketsManager, WebsocketsManager>();
            services.AddSingleton<IWebsocketsReceiver, SubscriptionManager>();
            services.AddSingleton<IDataGenerator, DataGenerator>();
            services.AddSingleton<IDataAggregator, DataAggregator>();

            var generatorConfiguration = new GeneratorConfiguration();
            Configuration.GetSection("Generator").Bind(generatorConfiguration);
            services.AddSingleton(generatorConfiguration);
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory)
        {
            loggerFactory.AddConsole(Configuration.GetSection("Logging"));
            loggerFactory.AddDebug();

            app.UseWebSocketsHandler("/ws/v1");

            app.UseMvc();

            /*var generator = new DataGenerator();

            Task.Run(() =>
            {
                Parallel.For(0, 8, new ParallelOptions { MaxDegreeOfParallelism = 8 }, i =>
                {
                    while (true)
                    {
                        var now = DateTime.Now.SecondsSince1970Utc();
                        var aggregatedTime = now - (now % 300) + 300;
                        var aggregatedTimeRange = new TimeRange(aggregatedTime - 300, aggregatedTime);

                        generator.GenerateData(Guid.NewGuid().ToString(), aggregatedTimeRange, 300);
                    }
                });
            });*/
        }
    }
}
