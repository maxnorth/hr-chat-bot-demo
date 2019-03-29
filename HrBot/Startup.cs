using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.AI.Luis;
using Microsoft.Bot.Builder.AI.QnA;
using Microsoft.Bot.Builder.Azure;
using Microsoft.Bot.Builder.ApplicationInsights;
using Microsoft.Bot.Builder.Integration.AspNet.Core;
using Microsoft.Bot.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Bot.Builder.Integration.ApplicationInsights.Core;
using Microsoft.Extensions.Logging;
using Microsoft.Bot.Connector.Authentication;

namespace HrBot
{
    public class Startup
    {        
        private ILoggerFactory _loggerFactory;

        public Startup(IHostingEnvironment env, ILoggerFactory loggerFactory)
        {
            _loggerFactory = loggerFactory;
        }

        public void ConfigureServices(IServiceCollection services)
        {            
            var botConfig = BotConfiguration.Load("./HrBot.bot");
            services.AddSingleton(botConfig);

            var endpointService = botConfig.Services.Single(x => x.Type == "endpoint") as EndpointService;

            var cosmosDbService = botConfig.Services.Single(x => x.Type == "cosmosdb") as CosmosDbService;
            var storage = new CosmosDbStorage(new CosmosDbStorageOptions { 
                CosmosDBEndpoint = new Uri(cosmosDbService.Endpoint),
                AuthKey = cosmosDbService.Key,
                DatabaseId = cosmosDbService.Database,
                CollectionId = cosmosDbService.Collection
            });

            var state = new ConversationState(storage);
            services.AddSingleton(state);

            services.AddBot<HrBot>(options => {
                options.Middleware.Add(new AutoSaveStateMiddleware());
            });
            
            services.AddBotApplicationInsights(botConfig);
            
            var luisService = botConfig.Services.Single(x => x.Type == "luis") as LuisService;
            services.AddSingleton(new LuisRecognizer(new LuisApplication(luisService)));

            var qnaService = botConfig.Services.Single(x => x.Type == "qna") as QnAMakerService;
            services.AddSingleton(new QnAMaker(qnaService));
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
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
