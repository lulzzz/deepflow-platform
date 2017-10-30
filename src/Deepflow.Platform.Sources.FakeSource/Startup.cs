using Deepflow.Platform.Abstractions.Realtime;
using Deepflow.Platform.Abstractions.Series;
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
            services.AddSingleton<IDataAggregator, DataAggregator>();

            services.AddSingleton<IRangeCreator<AggregatedDataRange>, AggregatedRangeCreator>();
            services.AddSingleton<IRangeAccessor<AggregatedDataRange>, AggregatedRangeAccessor>();
            services.AddSingleton<IRangeFilteringPolicy<AggregatedDataRange>, AggregateRangeFilteringPolicy>();
            services.AddSingleton<IRangeFilterer<AggregatedDataRange>, RangeFilterer<AggregatedDataRange>>();
            services.AddSingleton<IRangeJoiner<AggregatedDataRange>, RangeJoiner<AggregatedDataRange>>();
            services.AddSingleton<IRangeMerger<AggregatedDataRange>, RangeMerger<AggregatedDataRange>>();

            services.AddSingleton<IRangeCreator<RawDataRange>, RawDataRangeCreator>();
            services.AddSingleton<IRangeAccessor<RawDataRange>, RawDataRangeAccessor>();
            services.AddSingleton<IRangeFilteringPolicy<RawDataRange>, RawDataRangeFilteringPolicy>();
            services.AddSingleton<IRangeFilterer<RawDataRange>, RangeFilterer<RawDataRange>>();
            services.AddSingleton<IRangeJoiner<RawDataRange>, RangeJoiner<RawDataRange>>();
            services.AddSingleton<IRangeMerger<RawDataRange>, RangeMerger<RawDataRange>>();

            var generatorConfiguration = new GeneratorConfiguration();
            Configuration.GetSection("Generator").Bind(generatorConfiguration);
            services.AddSingleton(generatorConfiguration);

            if (generatorConfiguration.GeneratorPlugin == GeneratorPlugin.Deterministic)
            {
                services.AddSingleton<IDataGenerator, DeterministicDataGenerator>();
            }
            else if (generatorConfiguration.GeneratorPlugin == GeneratorPlugin.Random)
            {
                services.AddSingleton<IDataGenerator, RandomDataGenerator>();
            }
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory)
        {
            loggerFactory.AddConsole(Configuration.GetSection("Logging"));
            loggerFactory.AddDebug();

            app.UseWebSocketsHandler("/ws/v1");

            app.UseMvc();
        }
    }
}
