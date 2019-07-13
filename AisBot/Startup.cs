using System;
using System.Linq;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.AI.QnA;
using Microsoft.Bot.Builder.Azure;
using Microsoft.Bot.Builder.Integration.AspNet.Core;
using Microsoft.Bot.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Bot.Builder.Integration.ApplicationInsights.Core;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;

namespace AisBot
{
    public class Startup
    {        
        private IHostingEnvironment _env;
        private ILoggerFactory _loggerFactory;
        private readonly IConfiguration _configuration;

        public Startup(IHostingEnvironment env, ILoggerFactory loggerFactory, IConfiguration configuration)
        {
            _env = env;
            _loggerFactory = loggerFactory;
            _configuration = configuration;
        }

        public void ConfigureServices(IServiceCollection services)
        {            
            var botConfig = new BotConfig(_configuration);
            services.AddSingleton<BotConfiguration>(botConfig);

            if (_env.IsDevelopment())
            {
                var logger = _loggerFactory.CreateLogger(nameof(ConfigureServices));
                logger.LogInformation($"Bot file secret: {botConfig.Secret}");
                botConfig.Save();
            }

            var storage = new CosmosDbStorage(new CosmosDbStorageOptions { 
                CosmosDBEndpoint = new Uri(botConfig.CosmosStorage.Endpoint),
                AuthKey = botConfig.CosmosStorage.Key,
                DatabaseId = botConfig.CosmosStorage.Database,
                CollectionId = botConfig.CosmosStorage.Collection
            });

            var state = new ConversationState(storage);
            services.AddSingleton(state);

            services.AddBot<AisBot> (options => {
                options.Middleware.Add(new AutoSaveStateMiddleware(state));
            });
            
            services.AddBotApplicationInsights(botConfig);

            services.AddSingleton(new QnAMaker(botConfig.QnAMaker));
        }

        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            _loggerFactory.AddApplicationInsights(app.ApplicationServices, LogLevel.Information);

            app.UseBotApplicationInsights()
                .UseDefaultFiles()
                .UseStaticFiles()
                .UseBotFramework(); 
        }
    }
}
