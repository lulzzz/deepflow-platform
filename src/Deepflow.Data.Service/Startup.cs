using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Deepflow.Common.Model;
using Deepflow.Platform.Abstractions.Model;
using Deepflow.Platform.Abstractions.Realtime;
using Deepflow.Platform.Common.Data.Configuration;
using Deepflow.Platform.Common.Data.Persistence;
using Deepflow.Platform.Controllers;
using Deepflow.Platform.Model;
using Deepflow.Platform.Realtime;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Deepflow.Data.Service
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

            var dynamoDbConfiguration = new DynamoDbConfiguration();
            Configuration.GetSection("DynamoDb").Bind(dynamoDbConfiguration);
            services.AddSingleton(dynamoDbConfiguration);

            var modelConfiguration = new ModelConfiguration();
            Configuration.GetSection("Model").Bind(modelConfiguration);
            services.AddSingleton(modelConfiguration);

            services.AddSingleton<IWebsocketsManager, WebsocketsManager>();
            services.AddSingleton<IWebsocketsSender, WebsocketsManager>();
            services.AddSingleton<IWebsocketsReceiver, RealtimeMessageHandler>();

            services.AddSingleton<IModelMapProvider, InMemoryModelMapProvider>();
            services.AddSingleton<IPersistentDataProvider, DynamoDbPersistentDataProvider>();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory)
        {
            loggerFactory.AddConsole(Configuration.GetSection("Logging"));
            loggerFactory.AddDebug();

            app.UseMvc();
        }
    }
}
