﻿using System;
using System.Diagnostics;
using Deepflow.Common.Model;
using Deepflow.Common.Model.Model;
using Deepflow.Ingestion.Service.Configuration;
using Deepflow.Ingestion.Service.Metrics;
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
            /*var database = "appmetricsdemo";
            var uri = new Uri("http://54.252.216.203:8086");

            services.AddMetrics(options =>
                {
                    options.WithGlobalTags((globalTags, info) =>
                    {
                        globalTags.Add("app", info.EntryAssemblyName);
                        globalTags.Add("env", "stage");
                    });
                })
                .AddHealthChecks()
                .AddJsonSerialization()
                .AddReporting(
                    factory =>
                    {
                        factory.AddInfluxDb(
                            new InfluxDBReporterSettings
                            {
                                InfluxDbSettings = new InfluxDBSettings(database, uri),
                                ReportInterval = TimeSpan.FromSeconds(5)
                            });
                    })
                .AddMetricsMiddleware(options => options.IgnoredHttpStatusCodes = new[] { 404 });*/

            //services.AddMvc(options => options.AddMetricsResourceFilter());

            /*services.AddSingleton<StatsN.IStatsd>(provider => new StatsN.Statsd(new StatsN.StatsdOptions()
            {
                HostOrIp = "127.0.0.1",
                Port = 8125
            }));*/
            
            services.AddMvc();

            var seriesConfiguration = new Common.Model.SeriesConfiguration();
            Configuration.GetSection("Series").Bind(seriesConfiguration);
            services.AddSingleton(seriesConfiguration);

            var modelConfigurationLoader = new ModelConfigurationLoader();
            var modelConfiguration = modelConfigurationLoader.Load("model.csv");
            services.AddSingleton(modelConfiguration);

            var ingestionConfiguration = new IngestionConfiguration();
            Configuration.GetSection("Ingestion").Bind(ingestionConfiguration);
            services.AddSingleton(ingestionConfiguration);

            var realtimeConfiguration = new RealtimeConfiguration();
            Configuration.GetSection("Realtime").Bind(realtimeConfiguration);
            services.AddSingleton(realtimeConfiguration);
            
            if (ingestionConfiguration.PersistencePlugin == PersistencePlugin.Cassandra)
            {
                services.AddSingleton<IPersistentDataProvider, CassandraPersistentDataProvider>();

                var cassandraConfiguration = new CassandraConfiguration();
                Configuration.GetSection("Cassandra").Bind(cassandraConfiguration);
                services.AddSingleton(cassandraConfiguration);
            }
            else if (ingestionConfiguration.PersistencePlugin == PersistencePlugin.DynamoDb)
            {
                /*services.AddSingleton<IPersistentDataProvider, DynamoDbPersistentDataProvider>();

                var dynamoDbConfiguration = new DynamoDbConfiguration();
                Configuration.GetSection("DynamoDb").Bind(dynamoDbConfiguration);
                services.AddSingleton(dynamoDbConfiguration);*/
            }
            else if (ingestionConfiguration.PersistencePlugin == PersistencePlugin.Noop)
            {
                services.AddSingleton<IPersistentDataProvider, NoopPersistenceProvider>();
            }

            services.AddSingleton<IIngestionProcessor, IngestionProcessor>();
            services.AddSingleton<ICachedDataProvider, RedisCachedDataProvider>();
            services.AddSingleton<IDataAggregator, DataAggregator>();
            services.AddSingleton<IModelProvider, ModelProvider>();
            services.AddSingleton<RealtimeIngestionReceiver>();
            services.AddSingleton<IDataMessenger>(options => options.GetService<RealtimeIngestionReceiver>());
            services.AddSingleton<IRealtimeSubscriptions>(options => options.GetService<RealtimeIngestionReceiver>());
            
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
            services.AddSingleton<IWebsocketsReceiver>(options => options.GetService<RealtimeIngestionReceiver>());

            services.AddSingleton<TripCounterFactory>();

            services.AddCors();

            /*services.AddApplicationInsightsTelemetry(options =>
            {
                options.EnableAdaptiveSampling = true;
                options.InstrumentationKey = "0def8f5e-9482-48ec-880d-4d2a81834a49";
                options.EnableDebugLogger = false;
            });*/
        }

        public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory, IApplicationLifetime lifetime, IServiceProvider serviceProvider)
        {
            loggerFactory.AddConsole(Configuration.GetSection("Logging"));
            loggerFactory.AddApplicationInsights(serviceProvider, LogLevel.Warning);
            
            /*app.UseMetrics();
            app.UseMetricsReporting(lifetime);*/


            app.UseCors(builder => builder.AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod().AllowCredentials());

            app.UseMvc();
            
            app.UseCors(builder => builder.AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod().AllowCredentials());

            app.UseWebSocketsHandler("/ws/v1");
        }
    }
}
