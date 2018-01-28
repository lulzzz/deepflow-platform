﻿using Deepflow.Platform.Abstractions.Realtime;
using Deepflow.Platform.Abstractions.Series;
using Deepflow.Platform.Agent.Client;
using Deepflow.Platform.Agent.Core;
using Deepflow.Platform.Agent.Errors;
using Deepflow.Platform.Agent.Processor;
using Deepflow.Platform.Agent.Provider;
using Deepflow.Platform.Agent.Realtime;
using Deepflow.Platform.Common.Data.Configuration;
using Deepflow.Platform.Core.Tools;
using Deepflow.Platform.Realtime;
using Deepflow.Platform.Series;
using Deepflow.Platform.Sources.PISim.Provider;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Deepflow.Platform.Agent
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
            services.AddMvc(options => { options.Filters.AddService(typeof(AgentGlobalExceptionFilter)); });
            services.AddSingleton<IIngestionClient, IngestionClient>();
            services.AddSingleton<IAgentProcessor, AgentProcessor>();
            services.AddSingleton<TripCounterFactory>();
            services.AddSingleton<AgentGlobalExceptionFilter>();

            services.AddSingleton<IWebsocketsManager, WebsocketsManager>();
            services.AddSingleton<IWebsocketsSender, WebsocketsManager>();
            services.AddSingleton<IWebsocketsReceiver, RealtimeMessageHandler>();
            services.AddSingleton<IDataAggregator, DataAggregator>();

            var ingestionConfiguration = new Core.AgentIngestionConfiguration();
            Configuration.GetSection("Ingestion").Bind(ingestionConfiguration);
            services.AddSingleton(ingestionConfiguration);

            if (ingestionConfiguration.SourcePlugin == SourcePlugin.FakeSource)
            {
                services.AddSingleton<ISourceDataProvider, FakeSourceDataProvider>();

                var fakeSourceConfiguration = new FakeSourceConfiguration();
                Configuration.GetSection("FakeSource").Bind(fakeSourceConfiguration);
                services.AddSingleton(fakeSourceConfiguration);
            }
            else if (ingestionConfiguration.SourcePlugin == SourcePlugin.PISim)
            {
                services.AddSingleton<ISourceDataProvider, CassandraPiSimDataProvider>();

                var cassandraConfiguration = new CassandraConfiguration();
                Configuration.GetSection("Cassandra").Bind(cassandraConfiguration);
                services.AddSingleton(cassandraConfiguration);
            }

            services.AddCors();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory)
        {
            loggerFactory.AddConsole(Configuration.GetSection("Logging"));
            loggerFactory.AddDebug();

            var client = app.ApplicationServices.GetService<IIngestionClient>();
            app.ApplicationServices.GetService<IAgentProcessor>().Start();
            client.Start();
            
            app.UseCors(builder => builder.AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod().AllowCredentials());

            app.UseMvc();

            app.UseCors(builder => builder.AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod().AllowCredentials());
        }
    }
}
