using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime;
using Deepflow.Platform.Abstractions.Ingestion;
using Deepflow.Platform.Abstractions.Model;
using Deepflow.Platform.Abstractions.Realtime;
using Deepflow.Platform.Abstractions.Series;
using Deepflow.Platform.Abstractions.Sources;
using Deepflow.Platform.Controllers;
using Deepflow.Platform.Ingestion;
using Deepflow.Platform.Model;
using Deepflow.Platform.Realtime;
using Deepflow.Platform.Series;
using Deepflow.Platform.Series.DynamoDB;
using Deepflow.Platform.Series.Providers;
using Deepflow.Platform.Silo;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Orleans;
using Orleans.Runtime.Configuration;

namespace Deepflow.Platform
{
    public class Startup
    {
        private ClusterConfiguration _orleansConfig = ClusterConfiguration.LocalhostPrimarySilo();

        public Startup(IHostingEnvironment env)
        {
            var builder = new ConfigurationBuilder()
                .SetBasePath(env.ContentRootPath)
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .AddJsonFile($"appsettings.{env.EnvironmentName}.json", optional: true)
                .AddEnvironmentVariables();
            Configuration = builder.Build();
            OrleansStartup.Configuration = Configuration;
        }

        public IConfigurationRoot Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddMvc();
            services.AddOptions();

            GCSettings.LatencyMode = GCLatencyMode.Batch;

            services.AddSingleton<IWebsocketsManager, WebsocketsManager>();
            services.AddSingleton<IWebsocketsSender, WebsocketsManager>();
            services.AddSingleton<IWebsocketsReceiver, DataMessageHandler>();
            services.AddSingleton<IIngestionProcessor, IngestionProcessor>();

            var ingestionConfiguration = new IngestionConfiguration();
            Configuration.GetSection("Ingestion").Bind(ingestionConfiguration);
            services.AddSingleton(ingestionConfiguration);
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory, IServiceProvider serviceProvider)
        {
            loggerFactory.AddConsole(Configuration.GetSection("Logging"));
            loggerFactory.AddDebug();

            app.UseMvc();

            app.UseWebSocketsHandler("ws/v1");

            app.UseOrleans<OrleansStartup>(48880);

            GrainClient.Initialize(ClientConfiguration.LocalhostSilo());
        }
    }

    public class OrleansStartup
    {
        public static IConfigurationRoot Configuration;

        public OrleansStartup()
        {
            
        }

        public IServiceProvider ConfigureServices(IServiceCollection services)
        {
            services.AddLogging();

            services.AddSingleton<IDataAggregator, DataAggregator>();
            services.AddSingleton<IDataFilterer<AggregatedDataRange>, DataFilterer<AggregatedDataRange>>();
            services.AddSingleton<IDataJoiner<AggregatedDataRange>, DataJoiner<AggregatedDataRange>>();
            services.AddSingleton<IDataMerger<AggregatedDataRange>, DataMerger<AggregatedDataRange>>();
            services.AddSingleton<ISeriesKnower, SeriesKnower>();
            services.AddSingleton<ITimeFilterer, TimeFilterer>();
            services.AddSingleton<IDataProvider, DynamoDbDataProvider>();
            services.AddSingleton<IDataValidator, DataValidator>();
            services.AddSingleton<ISeriesConfiguration, SeriesConfiguration>();
            services.AddSingleton<IModelMapProvider, InMemoryModelMapProvider>();
            services.AddSingleton<IModelMap>(new ModelMap { SourceToModelMap = new Dictionary<DataSource, Dictionary<SourceName, EntityAttribute>>
            {
                {
                    new DataSource(Guid.Parse("4055083b-c6be-4902-a209-7d2dba99abae")), new Dictionary<SourceName, EntityAttribute>
                    {
                        {
                            new SourceName("tag1"), new EntityAttribute { Entity = Guid.NewGuid(), Attribute = Guid.NewGuid() }
                        }
                    }
                }
            }});

            
            var seriesSettings = new SeriesSettings();
            Configuration.GetSection("Series").Bind(seriesSettings);
            services.AddSingleton(seriesSettings);
            
            var dynamoDbConfiguration = new DynamoDbConfiguration();
            Configuration.GetSection("DynamoDB").Bind(dynamoDbConfiguration);
            services.AddSingleton(dynamoDbConfiguration);

            var serviceProvider = services.BuildServiceProvider();

            var loggerFactory = serviceProvider.GetService<ILoggerFactory>();
            loggerFactory.AddConsole(Configuration.GetSection("Logging"));
            loggerFactory.AddDebug();

            return serviceProvider;
        }
    }

    /*public class OrleansServiceProvider : IServiceProvider
    {
        private readonly IServiceProvider _fallbackServiceProvider;
        private readonly Dictionary<Type, Type> _types = new Dictionary<Type, Type>();
        private readonly Dictionary<Type, object> _instances = new Dictionary<Type, object>();

        public OrleansServiceProvider(IServiceProvider fallbackServiceProvider)
        {
            _fallbackServiceProvider = fallbackServiceProvider;
        }

        public void AddSingleton<TContract, TImplementation>() where TImplementation : TContract
        {
            _types.Add(typeof(TContract), typeof(TImplementation));
        }

        public void AddSingleton<TImplementation>(TImplementation instance)
        {
            _instances.Add(typeof(TImplementation), instance);
        }

        public object GetService(Type serviceType)
        {
            if (_instances.ContainsKey(serviceType))
            {
                return _instances[serviceType];
            }

            if (!_types.ContainsKey(serviceType))
            {
                return _fallbackServiceProvider.GetService(serviceType);
            }

            var implementationType = _types[serviceType];

            if (_instances.ContainsKey(implementationType))
            {
                return _instances[implementationType];
            }

            var constructorArgs = implementationType.GetConstructor(new Type[0]).GetParameters().Select(x => GetService(x.ParameterType));
            var instance = Activator.CreateInstance(implementationType, constructorArgs);
            _instances.Add(implementationType, instance);
            return instance;
        }
    }*/
}
