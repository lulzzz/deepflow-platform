using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Deepflow.Platform.Agent.Client;
using Deepflow.Platform.Agent.Core;
using Deepflow.Platform.Agent.Processor;
using Deepflow.Platform.Agent.Provider;
using Deepflow.Platform.Series.DynamoDB;
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
            services.AddMvc();
            services.AddSingleton<ISourceDataProvider, FakeSourceDataProvider>();
            services.AddSingleton<IIngestionClient, IngestionClient>();
            services.AddSingleton<IAgentProcessor, AgentProcessor>();

            var fakeSourceConfiguration = new FakeSourceConfiguration();
            Configuration.GetSection("FakeSource").Bind(fakeSourceConfiguration);
            services.AddSingleton(fakeSourceConfiguration);

            var ingestionConfiguration = new Core.AgentIngestionConfiguration();
            Configuration.GetSection("Ingestion").Bind(ingestionConfiguration);
            services.AddSingleton(ingestionConfiguration);
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory)
        {
            loggerFactory.AddConsole(Configuration.GetSection("Logging"));
            loggerFactory.AddDebug();

            var client = app.ApplicationServices.GetService<IIngestionClient>();
            app.ApplicationServices.GetService<IAgentProcessor>().Start();
            client.Start();

            app.UseMvc();
        }
    }
}
