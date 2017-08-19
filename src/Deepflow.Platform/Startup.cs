using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime;
using Deepflow.Platform.Abstractions.Realtime;
using Deepflow.Platform.Abstractions.Series;
using Deepflow.Platform.Controllers;
using Deepflow.Platform.Realtime;
using Deepflow.Platform.Series;
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
        }

        public IConfigurationRoot Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddMvc();
            services.AddOptions();

            GCSettings.LatencyMode = GCLatencyMode.Batch;

            Configuration.GetSection("Series").Bind(OrleansStartup.SeriesConfiguration);

            services.AddSingleton<IWebsocketsManager, WebsocketsManager>();
            services.AddSingleton<IWebsocketsSender, WebsocketsManager>();
            services.AddSingleton<IWebsocketsReceiver, DataMessageHandler>();

            /*var orleansServices = OrleansStartup.Services = new OrleansServiceProvider(services.BuildServiceProvider());

            orleansServices.AddSingleton<IDataAggregator, DataAggregator>();
            orleansServices.AddSingleton<IDataFilterer, DataFilterer>();
            orleansServices.AddSingleton<IDataJoiner, DataJoiner>();
            orleansServices.AddSingleton<IDataMerger, DataMerger>();
            orleansServices.AddSingleton<ISeriesKnower, SeriesKnower>();
            orleansServices.AddSingleton<ITimeFilterer, TimeFilterer>();
            
            return orleansServices;*/
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory, IServiceProvider serviceProvider)
        {
            loggerFactory.AddConsole(Configuration.GetSection("Logging"));
            loggerFactory.AddDebug();

            app.UseMvc();

            app.UseWebSocketsHandler();

            app.UseOrleans<OrleansStartup>(48880);

            GrainClient.Initialize(ClientConfiguration.LocalhostSilo());
        }
    }

    public class OrleansStartup
    {
        //public static OrleansServiceProvider Services;
        public static SeriesConfiguration SeriesConfiguration = new SeriesConfiguration();

        public OrleansStartup()
        {
            
        }

        public IServiceProvider ConfigureServices(IServiceCollection services)
        {
            services.AddSingleton<IDataAggregator, DataAggregator>();
            services.AddSingleton<IDataFilterer, DataFilterer>();
            services.AddSingleton<IDataJoiner, DataJoiner>();
            services.AddSingleton<IDataMerger, DataMerger>();
            services.AddSingleton<ISeriesKnower, SeriesKnower>();
            services.AddSingleton<ITimeFilterer, TimeFilterer>();
            services.AddSingleton<IDataProvider, SimpleRandomDataProvider>();

            services.AddSingleton(SeriesConfiguration);

            return services.BuildServiceProvider();
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
