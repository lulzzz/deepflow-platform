using Deepflow.Common.Model;
using Deepflow.Common.Model.Model;
using Deepflow.Ingestion.Service.Configuration;
using Deepflow.Ingestion.Service.Processing;
using Deepflow.Ingestion.Service.Realtime;
using Deepflow.Platform.Abstractions.Realtime;
using Deepflow.Platform.Abstractions.Series;
using Deepflow.Platform.Common.Data.Caching;
using Deepflow.Platform.Common.Data.Configuration;
using Deepflow.Platform.Common.Data.Persistence;
using Deepflow.Platform.Core.Tools;
using Deepflow.Platform.Realtime;
using Deepflow.Platform.Series;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Deepflow.Ingestion.Service
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
        
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddMvc();

            var dynamoDbConfiguration = new DynamoDbConfiguration();
            Configuration.GetSection("DynamoDb").Bind(dynamoDbConfiguration);
            services.AddSingleton(dynamoDbConfiguration);

            var seriesConfiguration = new Common.Model.SeriesConfiguration();
            Configuration.GetSection("Series").Bind(seriesConfiguration);
            services.AddSingleton(seriesConfiguration);

            var modelConfiguration = new ModelConfiguration();
            Configuration.GetSection("Model").Bind(modelConfiguration);
            services.AddSingleton(modelConfiguration);

            var ingestionConfiguration = new IngestionConfiguration();
            Configuration.GetSection("Ingestion").Bind(ingestionConfiguration);
            services.AddSingleton(ingestionConfiguration);

            services.AddSingleton<IIngestionProcessor, IngestionProcessor>();
            services.AddSingleton<ICachedDataProvider, RedisCachedDataProvider>();
            services.AddSingleton<IPersistentDataProvider, DynamoDbPersistentDataProvider>();
            services.AddSingleton<IDataAggregator, DataAggregator>();
            services.AddSingleton<IModelProvider, ModelProvider>();
            services.AddSingleton<IDataMessenger, PusherDataMessenger>();

            services.AddSingleton<IRangeCreator<AggregatedDataRange>, AggregatedRangeCreator>();
            services.AddSingleton<IRangeAccessor<AggregatedDataRange>, AggregatedRangeAccessor>();
            services.AddSingleton<IRangeFilteringPolicy<AggregatedDataRange>, AggregateRangeFilteringPolicy>();
            services.AddSingleton<IRangeFilterer<AggregatedDataRange>, RangeFilterer<AggregatedDataRange>>();
            services.AddSingleton<IRangeJoiner<AggregatedDataRange>, RangeJoiner<AggregatedDataRange>>();
            services.AddSingleton<IRangeMerger<AggregatedDataRange>, RangeMerger<AggregatedDataRange>>();

            services.AddSingleton<IRangeCreator<TimeRange>, TimeRangeCreator>();
            services.AddSingleton<IRangeAccessor<TimeRange>, TimeRangeAccessor>();
            services.AddSingleton<IRangeFilteringPolicy<TimeRange>, TimeRangeFilteringPolicy>();
            services.AddSingleton<IRangeFilterer<TimeRange>, RangeFilterer<TimeRange>>();
            services.AddSingleton<IRangeJoiner<TimeRange>, RangeJoiner<TimeRange>>();
            services.AddSingleton<IRangeMerger<TimeRange>, RangeMerger<TimeRange>>();

            services.AddSingleton<IWebsocketsManager, WebsocketsManager>();
            services.AddSingleton<IWebsocketsSender, WebsocketsManager>();
            services.AddSingleton<IWebsocketsReceiver, RealtimeIngestionReceiver>();

            services.AddSingleton<TripCounterFactory>();
        }
        
        public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory)
        {
            loggerFactory.AddConsole(Configuration.GetSection("Logging"));
            loggerFactory.AddDebug();

            app.UseMvc();

            app.UseWebSocketsHandler("/ws/v1");
        }
    }
}
